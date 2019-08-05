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

        /// <summary>
        /// 头文件最后修改时间
        /// </summary>
        private static Dictionary<string,DateTime> HeaderFilesLastWriteTime = new Dictionary<string, DateTime>();

        /// <summary>
        /// 头文件是否失效
        /// </summary>
        private static Dictionary<string,bool> HeaderFilesIsExpried = new Dictionary<string, bool>();

        /// <summary>
        /// 暂存ip
        /// </summary>
        /// <typeparam name="string">ip</typeparam>
        /// <typeparam name="bool">随便</typeparam>
        /// <returns></returns>
        private static Dictionary<string,bool> RawProxyIps = new Dictionary<string, bool>();

        public GetFreeProxyJob(ILogger<BaseJob> logger, IHostApplicationLifetime appLifetime) : base(JobTimerInterval, logger, appLifetime)
        {
            //base.Logger.Log(LogLevel.Debug, "测试作业启动");
            // LogHelper.Log("抓取免费IP代理作业启动");
        }

        protected override void DoWork(object state)
        {
            //base.Logger.Log(LogLevel.Debug, "测试作业执行：");
            //LogHelper.Log("抓取免费IP代理作业执行：");
            RawProxyIps.Clear();
            var task01 = Task.Run(() => FreeProxy01());
            var task02 =Task.Run(() => FreeProxy02());
            Task.WaitAll(task01, task02);
            ValidProxyIps();
        }

        /// <summary>
        /// https://ip.ihuan.me
        /// </summary>
        void FreeProxy01(string urlParams = "", int page = 1, string usableProxyIp = "")
        {
            string getIpUrl = $"https://ip.ihuan.me{urlParams}";
            string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ihuan_header.txt");
            var fileLastWriteTime = File.GetLastWriteTime(headerFilePath);
            bool HeaderFileIsExpried = false;
            DateTime HeaderFileLastWriteTime = DateTime.MinValue;
            if (!HeaderFilesIsExpried.ContainsKey(headerFilePath))
            {
                HeaderFilesIsExpried[headerFilePath] = HeaderFileIsExpried;
            }
            if (!HeaderFilesLastWriteTime.ContainsKey(headerFilePath))
            {
                HeaderFilesLastWriteTime[headerFilePath] = HeaderFileLastWriteTime;
            }
            if (HeaderFileIsExpried && fileLastWriteTime <= HeaderFileLastWriteTime)
            {
                Console.WriteLine($"抓取免费IP代理作业异常：ihuan cookie 文件头未更新最新，暂不执行作业！");
                return;
            }
            var headerStrs = ReadAllLines(headerFilePath);
            var dictHeaders = headerStrs.Select(h => h.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(x => x[0], x => x[1]);
            var dictProxyIps = RedisHelper.GetAllEntriesFromHash(IpProxyCacheKey);
            var proxyIps = dictProxyIps.Where(d => d.Value == "0").Select(x => x.Key).ToList();
            if (!string.IsNullOrWhiteSpace(usableProxyIp))
            {
                var usableProxyIps = new List<string>() { usableProxyIp };
                proxyIps.RemoveAll(x => x == usableProxyIp);
                usableProxyIps.AddRange(proxyIps);
                proxyIps = usableProxyIps;
            }
            string ipHtml = string.Empty;
            foreach (var currProxyIp in proxyIps)
            {
                try
                {
                    Console.WriteLine($"抓取免费IP代理作业 ihuan 当前使用代理Ip：{currProxyIp}");
                    var ipResponse = HttpHelper.GetResponse(getIpUrl, dictHeaders, 10, new WebProxy($"http://{currProxyIp}"));
                    if (ipResponse.StatusCode == HttpStatusCode.OK)
                    {
                        ipHtml = ipResponse.Content.ReadAsStringAsync().Result;
                        usableProxyIp = currProxyIp;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    // LogHelper.Log(ex);
                }
            }
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(ipHtml);
            var ipTrs = htmlDoc.DocumentNode.SelectNodes("//div[2]//div[2]//table//tbody//tr");
            if (ipTrs == null)
            {
                HeaderFilesIsExpried[headerFilePath] = true;
                HeaderFilesLastWriteTime[headerFilePath] = fileLastWriteTime;
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
                    Console.WriteLine($"抓取免费IP代理作业开始抓取ihuan第{nextPage}页:");
                    FreeProxy01(href, nextPage, usableProxyIp);
                    return;
                }
            }
            Console.WriteLine($"抓取免费IP代理作业抓取ihuan结束");
        }

        /// <summary>
        /// http://www.89ip.cn/index_1.html
        /// </summary>
        void FreeProxy02(int page = 1, string usableProxyIp = "")
        {
            string getIpUrl = $"http://www.89ip.cn/index_{page}.html";
            string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "89ip_header.txt");
            var fileLastWriteTime = File.GetLastWriteTime(headerFilePath);
            bool HeaderFileIsExpried = false;
            DateTime HeaderFileLastWriteTime = DateTime.MinValue;
            if (!HeaderFilesIsExpried.ContainsKey(headerFilePath))
            {
                HeaderFilesIsExpried[headerFilePath] = HeaderFileIsExpried;
            }
            if (!HeaderFilesLastWriteTime.ContainsKey(headerFilePath))
            {
                HeaderFilesLastWriteTime[headerFilePath] = HeaderFileLastWriteTime;
            }
            if (HeaderFileIsExpried && fileLastWriteTime <= HeaderFileLastWriteTime)
            {
                Console.WriteLine($"抓取免费IP代理作业异常：89ip cookie 文件头未更新最新，暂不执行作业！");
                return;
            }
            var headerStrs = ReadAllLines(headerFilePath);
            var dictHeaders = headerStrs.Select(h => h.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(x => x[0], x => x[1]);
            var dictProxyIps = RedisHelper.GetAllEntriesFromHash(IpProxyCacheKey);
            var proxyIps = dictProxyIps.Where(d => d.Value == "0").Select(x => x.Key).ToList();
            if (!string.IsNullOrWhiteSpace(usableProxyIp))
            {
                var usableProxyIps = new List<string>() { usableProxyIp };
                proxyIps.RemoveAll(x => x == usableProxyIp);
                usableProxyIps.AddRange(proxyIps);
                proxyIps = usableProxyIps;
            }
            string ipHtml = string.Empty;
            foreach (var currProxyIp in proxyIps)
            {
                try
                {
                    Console.WriteLine($"抓取免费IP代理作业 89ip 当前使用代理Ip：{currProxyIp}");
                    var ipResponse = HttpHelper.GetResponse(getIpUrl, dictHeaders, 10, new WebProxy($"http://{currProxyIp}"));
                    if (ipResponse.StatusCode == HttpStatusCode.OK)
                    {
                        ipHtml = ipResponse.Content.ReadAsStringAsync().Result;
                        usableProxyIp = currProxyIp;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
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
                    HeaderFilesIsExpried[headerFilePath] = true;
                    HeaderFilesLastWriteTime[headerFilePath] = fileLastWriteTime;
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
                    Console.WriteLine($"抓取免费IP代理作业开始抓取89ip第{nextPage}页:");
                    FreeProxy02(nextPage, usableProxyIp);
                    return;
                }
            }
            Console.WriteLine($"抓取免费IP代理作业抓取89ip结束");
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
            if (RawProxyIps.Count > 0)
            {
                string checkUrl = "http://httpbin.org/ip";
                foreach (var proxyIp in RawProxyIps.Keys)
                {
                    try
                    {
                        var ipResponse = HttpHelper.GetResponse(checkUrl, null, 5, new WebProxy($"http://{proxyIp}"));
                        if (ipResponse.StatusCode == HttpStatusCode.OK)
                        {
                            var resultIp = ipResponse.Content.ReadAsStringAsync().Result;
                            if (resultIp.Contains("origin"))
                            {
                                RedisHelper.Set(IpProxyCacheKey, proxyIp, "0");
                                RawProxyIps.Remove(proxyIp); 
                                Console.WriteLine($"代理IP：{proxyIp} 通过校验");
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                    Console.WriteLine($"代理IP：{proxyIp} 未通过校验");
                }
            }
        }
    }

    class IpProxyView
    {
        [JsonProperty("row")]
        public int Row { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }
}
