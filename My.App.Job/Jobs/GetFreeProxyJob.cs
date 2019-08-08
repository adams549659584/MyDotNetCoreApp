using System;
using System.Collections.Generic;
using System.Text;
using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using My.App.Core;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace My.App.Job
{
    public class GetFreeProxyJob : BaseJob
    {
        private static TimeSpan JobTimerInterval = TimeSpan.FromMinutes(20);
        private static RedisHelper RedisHelper = new RedisHelper("dotnetcore_redis:6379");
        private static string IpProxyCacheKey = "useful_proxy";
        private static int ProxyIpMaxPage = 30;//最大抓取页数

        /// <summary>
        /// 头文件最后修改时间
        /// </summary>
        private static Dictionary<string, DateTime> HeaderFilesLastWriteTime = new Dictionary<string, DateTime>();

        /// <summary>
        /// 头文件是否失效
        /// </summary>
        private static Dictionary<string, bool> HeaderFilesIsExpried = new Dictionary<string, bool>();

        /// <summary>
        /// 暂存ip
        /// </summary>
        /// <typeparam name="string">ip</typeparam>
        /// <typeparam name="bool">随便</typeparam>
        /// <returns></returns>
        private static Dictionary<string, bool> RawProxyIps = new Dictionary<string, bool>();

        public GetFreeProxyJob(ILogger<BaseJob> logger, IHostApplicationLifetime appLifetime) : base(JobTimerInterval, logger, appLifetime)
        {
            //base.Logger.Log(LogLevel.Debug, "测试作业启动");
            // LogHelper.Log("抓取免费IP代理作业启动");
        }

        protected override void DoWork(object state)
        {
            //base.Logger.Log(LogLevel.Debug, "测试作业执行：");
            //LogHelper.Log("抓取免费IP代理作业执行：");
            var dictProxyIps = RedisHelper.HashGetAll(IpProxyCacheKey);
            var usefulProxyIps = dictProxyIps.Keys.ToList();
            Task.WaitAll(
                Task.Run(() => FreeProxyIHuan("", 1, usefulProxyIps.Clone())),
                Task.Run(() => FreeProxy89Ip(1, usefulProxyIps.Clone()))
                );
            ValidProxyIps();
            RawProxyIps.Clear();

            //TestValidProxyIps();
        }

        /// <summary>
        /// https://ip.ihuan.me
        /// </summary>
        void FreeProxyIHuan(string urlParams = "", int page = 1, List<string> usefulProxyIps = null)
        {
            try
            {
                if (page <= ProxyIpMaxPage)
                {
                    Console.WriteLine($"抓取免费IP代理作业开始抓取ihuan第{page}页:");
                    string getIpUrl = $"https://ip.ihuan.me{urlParams}";
                    string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ihuan_header.txt");
                    var fileLastWriteTime = File.GetLastWriteTime(headerFilePath);
                    bool HeaderFileIsExpried = false;
                    DateTime HeaderFileLastWriteTime = DateTime.MinValue;
                    if (!HeaderFilesIsExpried.ContainsKey(headerFilePath))
                    {
                        lock (HeaderFilesIsExpried)
                        {
                            HeaderFilesIsExpried[headerFilePath] = HeaderFileIsExpried;
                        }
                    }
                    if (!HeaderFilesLastWriteTime.ContainsKey(headerFilePath))
                    {
                        lock (HeaderFilesLastWriteTime)
                        {
                            HeaderFilesLastWriteTime[headerFilePath] = HeaderFileLastWriteTime;
                        }
                    }
                    if (HeaderFileIsExpried && fileLastWriteTime <= HeaderFileLastWriteTime)
                    {
                        Console.WriteLine($"抓取免费IP代理作业异常：ihuan cookie 文件头未更新最新，暂不执行作业！");
                        return;
                    }
                    var headerStrs = ReadAllLines(headerFilePath);
                    var dictHeaders = headerStrs.Select(h => h.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(x => x[0], x => x[1]);
                    string ipHtml = string.Empty;
                    var tempProxyIps = new string[usefulProxyIps.Count];
                    usefulProxyIps.CopyTo(tempProxyIps);
                    foreach (var currProxyIp in tempProxyIps)
                    {
                        try
                        {
                            Console.WriteLine($"抓取免费IP代理作业 ihuan 当前使用代理Ip：{currProxyIp}");
                            ipHtml = HttpHelper.Get(getIpUrl, dictHeaders, 10 * 1000, new WebProxy($"http://{currProxyIp}"));
                            usefulProxyIps.RemoveAll(x => x == currProxyIp);
                            usefulProxyIps.Insert(0, currProxyIp);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ihuan 使用代理Ip {currProxyIp} 异常： {ex.Message}");
                            // Console.WriteLine(ex.ToString());
                            // LogHelper.Log(ex);
                        }
                    }
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(ipHtml);
                    var ipTrs = htmlDoc.DocumentNode.SelectNodes("//div[2]//div[2]//table//tbody//tr");
                    if (ipTrs == null)
                    {
                        lock (HeaderFilesIsExpried)
                        {
                            HeaderFilesIsExpried[headerFilePath] = true;
                        }
                        lock (HeaderFilesLastWriteTime)
                        {
                            HeaderFilesLastWriteTime[headerFilePath] = fileLastWriteTime;
                        }
                        Console.WriteLine("抓取免费IP代理作业 ihuan 异常：");
                        Console.WriteLine(ipHtml);
                        NotifyHelper.Weixin("抓取免费IP代理作业 ihuan 异常", ipHtml);
                        return;
                    }
                    foreach (var item in ipTrs)
                    {
                        try
                        {
                            var ip = item.SelectSingleNode($"{item.XPath}//td[1]//a").InnerText;
                            var port = item.SelectSingleNode($"{item.XPath}//td[2]").InnerText;
                            Console.WriteLine($"抓取免费IP代理作业 ihuan 抓取到IP:{ip}:{port}");
                            // RedisHelper.Set(IpProxyCacheKey, $"{ip}:{port}", "1");
                            RawProxyIps[$"{ip}:{port}"] = false;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            // LogHelper.Log(ex);
                        }
                    }
                    var pageEles = htmlDoc.DocumentNode.SelectNodes("//div[2]//nav//ul//li//a");
                    if (pageEles.Count > 1)
                    {
                        int nextPage = 0;
                        string href = "";
                        for (int i = 1; i < pageEles.Count; i++)
                        {
                            href = pageEles[i].GetAttributeValue("href", "");
                            int.TryParse(pageEles[i].InnerText, out nextPage);
                            if (nextPage > page)
                            {
                                break;
                            }
                        }
                        if (nextPage > 0)
                        {
                            FreeProxyIHuan(href, nextPage, usefulProxyIps);
                            return;
                        }
                    }
                }
                Console.WriteLine($"抓取免费IP代理作业抓取ihuan结束");
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }
        }

        /// <summary>
        /// http://www.89ip.cn/index_1.html
        /// </summary>
        void FreeProxy89Ip(int page = 1, List<string> usefulProxyIps = null)
        {
            try
            {
                if (page <= ProxyIpMaxPage)
                {
                    Console.WriteLine($"抓取免费IP代理作业开始抓取89ip第{page}页:");
                    string getIpUrl = $"http://www.89ip.cn/index_{page}.html";
                    string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "89ip_header.txt");
                    var fileLastWriteTime = File.GetLastWriteTime(headerFilePath);
                    bool HeaderFileIsExpried = false;
                    DateTime HeaderFileLastWriteTime = DateTime.MinValue;
                    if (!HeaderFilesIsExpried.ContainsKey(headerFilePath))
                    {
                        lock (HeaderFilesIsExpried)
                        {
                            HeaderFilesIsExpried[headerFilePath] = HeaderFileIsExpried;
                        }
                    }
                    if (!HeaderFilesLastWriteTime.ContainsKey(headerFilePath))
                    {
                        lock (HeaderFilesLastWriteTime)
                        {
                            HeaderFilesLastWriteTime[headerFilePath] = HeaderFileLastWriteTime;
                        }
                    }
                    if (HeaderFileIsExpried && fileLastWriteTime <= HeaderFileLastWriteTime)
                    {
                        Console.WriteLine($"抓取免费IP代理作业异常：89ip cookie 文件头未更新最新，暂不执行作业！");
                        return;
                    }
                    var headerStrs = ReadAllLines(headerFilePath);
                    var dictHeaders = headerStrs.Select(h => h.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(x => x[0], x => x[1]);
                    string ipHtml = string.Empty;
                    var tempProxyIps = new string[usefulProxyIps.Count];
                    usefulProxyIps.CopyTo(tempProxyIps);
                    foreach (var currProxyIp in tempProxyIps)
                    {
                        try
                        {
                            Console.WriteLine($"抓取免费IP代理作业 89ip 当前使用代理Ip：{currProxyIp}");
                            ipHtml = HttpHelper.Get(getIpUrl, dictHeaders, 10 * 1000, new WebProxy($"http://{currProxyIp}"));
                            usefulProxyIps.RemoveAll(x => x == currProxyIp);
                            usefulProxyIps.Insert(0, currProxyIp);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"89ip 使用代理Ip {currProxyIp} 异常： {ex.Message}");
                            // Console.WriteLine(ex.ToString());
                            // LogHelper.Log(ex);
                        }
                    }
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(ipHtml);
                    var ipTrs = htmlDoc.DocumentNode.SelectNodes("//div[3]//div[1]//div//div[1]//table//tbody//tr");
                    if (ipTrs == null)
                    {
                        if (ipHtml.Contains("89免费代理ip"))
                        {
                            Console.WriteLine($"抓取免费IP代理作业抓取89ip结束");
                        }
                        else
                        {
                            // HeaderFilesIsExpried[headerFilePath] = true;
                            // HeaderFilesLastWriteTime[headerFilePath] = fileLastWriteTime;
                            Console.WriteLine("抓取免费IP代理作业 89ip 异常：");
                            Console.WriteLine(ipHtml);
                            NotifyHelper.Weixin("抓取免费IP代理作业 89ip 异常", ipHtml);
                        }
                        return;
                    }
                    foreach (var item in ipTrs)
                    {
                        try
                        {
                            var ip = item.SelectSingleNode($"{item.XPath}//td[1]").InnerText.Trim();
                            var port = item.SelectSingleNode($"{item.XPath}//td[2]").InnerText.Trim();
                            Console.WriteLine($"抓取免费IP代理作业 89ip 抓取到IP:{ip}:{port}");
                            // RedisHelper.Set(IpProxyCacheKey, $"{ip}:{port}", "1");
                            RawProxyIps[$"{ip}:{port}"] = false;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            // LogHelper.Log(ex);
                        }
                    }
                    var pageEles = htmlDoc.DocumentNode.SelectNodes("//div[@id='layui-laypage-1']//a");
                    if (pageEles.Count > 2)
                    {
                        int nextPage = 0;
                        for (int i = 1; i < (pageEles.Count - 1); i++)
                        {
                            int.TryParse(pageEles[i].InnerText, out nextPage);
                            if (nextPage > page)
                            {
                                break;
                            }
                        }
                        if (nextPage > 0)
                        {
                            FreeProxy89Ip(nextPage, usefulProxyIps);
                            return;
                        }
                    }
                }
                Console.WriteLine($"抓取免费IP代理作业抓取89ip结束");
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }
        }

        string[] ReadAllLines(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllLines(path);
            }
            else
            {
                Console.WriteLine($"文件【{path}】不存在");
            }
            return new string[0];
        }

        /// <summary>
        /// 校验代理ip是否可用，可用的放进ip池
        /// </summary>
        void ValidProxyIps()
        {
            Console.WriteLine($"开始校验代理ip是否可用，当前需校验ip数量为{RawProxyIps.Count}");
            int usefulProxyIpCount = 0;
            if (RawProxyIps.Count > 0)
            {
                int threadCount = 50;
                int pageCount = (int)Math.Ceiling(RawProxyIps.Count / (decimal)threadCount);
                var taskList = new Task<List<string>>[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    var proxyIps = RawProxyIps.Keys.Skip(i * pageCount).Take(pageCount).ToArray();
                    taskList[i] = ValidProxyIps(proxyIps, ProxyCheckType.Http);
                }
                var taskResults = Task.WhenAll(taskList).Result;
                var usefulProxyIps = taskResults.SelectMany(x => x).ToList();
                usefulProxyIpCount = usefulProxyIps.Count;
            }
            Console.WriteLine($"结束校验代理ip是否可用，当前共抓取IP数量：{RawProxyIps.Count}，可用IP数量：{usefulProxyIpCount}");
        }

        void TestValidProxyIps()
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            int checkProxyIpCount = 12;
            var RawProxyIps = RedisHelper.HashGetAll(IpProxyCacheKey).Keys.Take(checkProxyIpCount).ToList();
            int threadCount = 12;
            int pageCount = (int)Math.Ceiling(RawProxyIps.Count / (decimal)threadCount);
            var taskList = new Task<List<string>>[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var proxyIps = RawProxyIps.Skip(i * pageCount).Take(pageCount).ToArray();
                taskList[i] = ValidProxyIps(proxyIps, ProxyCheckType.Http);
            }
            var taskResults = Task.WhenAll(taskList).Result;
            watch.Stop();
            var usefulProxyIps = taskResults.SelectMany(x => x).ToList();
            Console.WriteLine($"全部代理IP({checkProxyIpCount})检查完毕，有效IP({usefulProxyIps.Count})，耗时：{watch.Elapsed.TotalSeconds} 秒");
        }

        async Task<List<string>> ValidProxyIps(string[] rawProxyIps, ProxyCheckType proxyCheckType)
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
            await Task.Run(() =>
            {
                foreach (var proxyIp in rawProxyIps)
                {
                    try
                    {
                        var resultIp = HttpHelper.Get(checkUrl, null, 5 * 1000, new WebProxy($"http://{proxyIp}"));
                        if (resultIp.Contains("origin"))
                        {
                            RedisHelper.HashSet(IpProxyCacheKey, proxyIp, "0");
                            usefulProxyIps.Add(proxyIp);
                            Console.WriteLine($"代理IP：{proxyIp} 通过{checkTypeName}校验");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Console.WriteLine(ex.Message);
                        // Console.WriteLine(ex.ToString());
                        Console.WriteLine($"代理IP：{proxyIp} 未通过{checkTypeName}校验：{ex.Message}");
                    }
                }
            });
            return usefulProxyIps;
        }

        enum ProxyCheckType
        {
            Http = 0,
            Https = 1
        }
        class ProxyCheckUrl
        {
            public const string HTTP = "http://httpbin.org/ip";
            public const string HTTPS = "https://httpbin.org/ip";
        }
    }
}
