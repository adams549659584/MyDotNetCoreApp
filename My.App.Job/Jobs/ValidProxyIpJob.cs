using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using My.App.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace My.App.Job
{
    public class ValidProxyIpJob : BaseJob
    {
        private static TimeSpan JobTimerInterval = TimeSpan.FromMinutes(20);
        private static RedisHelper RedisHelper = new RedisHelper("dotnetcore_redis:6379");
        private static MongoDBServiceBase MongoDBServiceBase = new MongoDBServiceBase("MyJob");
        private static string CurrentIp
        {
            get
            {
                return RedisHelper.Get<string>("My.App.Job.IpPush.LastIp");
            }
        }
        public ValidProxyIpJob(ILogger<BaseJob> logger, IHostApplicationLifetime appLifetime) : base(JobTimerInterval, logger, appLifetime)
        {
        }

        protected override async Task DoWork(object state)
        {
            var mongoFindOptions = new MongoFindOptions<ProxyIpEnt>()
            {
                Skip = 0,
                Limit = 1000,
                SortConditions = x => x.LastValidTime,
                IsDescending = false
            };
            var waitForValidProxyIpList = await MongoDBServiceBase.GetList<ProxyIpEnt>(x => x.IsDelete == false, mongoFindOptions);
            var timeout = new TimeSpan(1, 0, 0);
            ValidProxyIps(waitForValidProxyIpList).Wait(timeout);
        }

        /// <summary>
        /// 校验代理ip是否可用，可用的放进ip池
        /// </summary>
        async Task ValidProxyIps(List<ProxyIpEnt> rawProxyIpList)
        {
            Console.WriteLine($"开始校验ip池中的代理ip是否可用，当前需校验ip数量为{rawProxyIpList.Count}");
            if (rawProxyIpList.Count > 0)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                int threadCount = DictHelper.GetValue("My.App.Job.GetFreeProxyJob.ValidProxyIp.ThreadCount").ToInt(50);
                int pageCount = (int)Math.Ceiling(rawProxyIpList.Count / (decimal)threadCount);
                // http
                var httpTaskList = new Task<List<string>>[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    var proxyIps = rawProxyIpList.Skip(i * pageCount).Take(pageCount).ToArray();
                    httpTaskList[i] = Task.Run(() => ValidProxyIps(proxyIps, ProxyCheckType.Http));
                }
                var httpTaskResults = await Task.WhenAll(httpTaskList);
                watch.Stop();
                var httpUsefulProxyIps = httpTaskResults.SelectMany(x => x).ToList();
                Console.WriteLine($"本次ip池取出代理IP({rawProxyIpList.Count})HTTP检查完毕，有效IP({httpUsefulProxyIps.Count})，耗时：{watch.Elapsed.TotalSeconds} 秒");

                // https
                watch.Restart();
                var httpsTaskList = new Task<List<string>>[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    var proxyIps = rawProxyIpList.Skip(i * pageCount).Take(pageCount).ToArray();
                    httpsTaskList[i] = Task.Run(() => ValidProxyIps(proxyIps, ProxyCheckType.Https));
                }
                var httpsTaskResults = await Task.WhenAll(httpsTaskList);
                watch.Stop();
                var httpsUsefulProxyIps = httpsTaskResults.SelectMany(x => x).ToList();
                Console.WriteLine($"本次ip池取出代理IP({rawProxyIpList.Count}),HTTPS检查完毕，有效IP({httpsUsefulProxyIps.Count})，耗时：{watch.Elapsed.TotalSeconds} 秒");
            }
            else
            {
                Console.WriteLine($"结束校验ip池中的代理ip是否可用，当前共取出IP数量：0");
            }
        }

        async Task<List<string>> ValidProxyIps(ProxyIpEnt[] rawProxyIps, ProxyCheckType proxyCheckType)
        {
            string checkUrl = string.Empty;
            switch (proxyCheckType)
            {
                case ProxyCheckType.Http:
                    checkUrl = ProxyCheckUrl.HTTP;
                    break;
                case ProxyCheckType.Https:
                    checkUrl = ProxyCheckUrl.HTTPS;
                    break;
            }
            string checkTypeName = proxyCheckType.ToString();
            List<string> usefulProxyIps = new List<string>();
            Stopwatch stopwatch = new Stopwatch();
            foreach (var proxyIpEnt in rawProxyIps)
            {
                var queryIpTask = MongoDBServiceBase.GetList<ProxyIpEnt>(x => x.IP == proxyIpEnt.IP && x.Port == proxyIpEnt.Port);
                try
                {
                    stopwatch.Restart();
                    var resultIp = await HttpHelper.GetAsync(checkUrl, null, 5 * 1000, new WebProxy($"http://{proxyIpEnt.IP}:{proxyIpEnt.Port}"));
                    stopwatch.Stop();
                    if (resultIp.Contains("origin"))
                    {
                        usefulProxyIps.Add($"{proxyIpEnt.IP}:{proxyIpEnt.Port}");
                        // Console.WriteLine($"本次ip池取出代理IP：{proxyIpEnt.IP}:{proxyIpEnt.Port} 通过{checkTypeName}校验");
                        switch (proxyCheckType)
                        {
                            case ProxyCheckType.Http:
                                proxyIpEnt.Speed = stopwatch.Elapsed.TotalMilliseconds;
                                break;
                            case ProxyCheckType.Https:
                                proxyIpEnt.HttpsSpeed = stopwatch.Elapsed.TotalMilliseconds;
                                proxyIpEnt.IsSupportHttps = true;
                                break;
                        }
                        proxyIpEnt.Anonymity = resultIp.Contains(CurrentIp) ? "透明" : "高匿";
                        proxyIpEnt.LastValidTime = DateTime.Now;
                        var queryResult = await queryIpTask;
                        if (queryResult.Count == 0)
                        {
                            proxyIpEnt.Id = Guid.NewGuid();
                            proxyIpEnt.CreateTime = DateTime.Now;
                            MongoDBServiceBase.Insert(proxyIpEnt)
                            .ContinueWith(insertResult =>
                            {
                                // Console.WriteLine($"{proxyIpEnt.IP}:{proxyIpEnt.Port}插入mongodb成功");
                            });
                        }
                        else
                        {
                            var oldProxyIpEnt = queryResult.FirstOrDefault();
                            if (proxyCheckType == ProxyCheckType.Https)
                            {
                                proxyIpEnt.Speed = oldProxyIpEnt.Speed;
                            }
                            proxyIpEnt.Id = oldProxyIpEnt.Id;
                            proxyIpEnt.CreateTime = oldProxyIpEnt.CreateTime;
                            MongoDBServiceBase.Replace(proxyIpEnt)
                            .ContinueWith(replaceResult =>
                            {
                                // Console.WriteLine($"{proxyIpEnt.IP}:{proxyIpEnt.Port}更新mongodb成功");
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Console.WriteLine(ex.Message);
                    // Console.WriteLine(ex.ToString());
                    // Console.WriteLine($"本次ip池取出代理IP：{proxyIpEnt.IP}:{proxyIpEnt.Port} 未通过{checkTypeName}校验：{ex.Message}");
                    var queryResult = await queryIpTask;
                    if (queryResult.Count > 0)
                    {
                        var oldProxyIpEnt = queryResult.FirstOrDefault();
                        oldProxyIpEnt.IsDelete = true;
                        oldProxyIpEnt.LastValidTime = DateTime.Now;
                        MongoDBServiceBase.Replace(oldProxyIpEnt)
                        .ContinueWith(replaceResult =>
                        {
                            // Console.WriteLine($"{oldProxyIpEnt.IP}:{oldProxyIpEnt.Port}已更新为失效");
                        });
                    }
                }
            }
            return usefulProxyIps;
        }
    }
}
