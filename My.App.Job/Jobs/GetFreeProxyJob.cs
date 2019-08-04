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

namespace My.App.Job
{
    public class GetFreeProxyJob : BaseJob
    {
        private static TimeSpan JobTimerInterval = TimeSpan.FromMinutes(60);
        private static RedisHelper RedisHelper = new RedisHelper("dotnetcore_redis:6379");
        private static string IpProxyCacheKey = "useful_proxy";

        /// <summary>
        /// 头文件最后修改时间
        /// </summary>
        private static DateTime HeaderFileLastWriteTime = DateTime.MinValue;

        /// <summary>
        /// 头文件是否失效
        /// </summary>
        private static bool HeaderFileIsExpried = false;

        public GetFreeProxyJob(ILogger<BaseJob> logger, IHostApplicationLifetime appLifetime) : base(JobTimerInterval, logger, appLifetime)
        {
            //base.Logger.Log(LogLevel.Debug, "测试作业启动");
            LogHelper.Log("抓取免费IP代理作业启动");
        }

        protected override void DoWork(object state)
        {
            //base.Logger.Log(LogLevel.Debug, "测试作业执行：");
            //LogHelper.Log("抓取免费IP代理作业执行：");
            FreeProxy01();
            FreeProxy02();
        }

        /// <summary>
        /// https://ip.ihuan.me
        /// </summary>
        void FreeProxy01(string urlParams = "", int page = 1)
        {
            string getIpUrl = $"https://ip.ihuan.me{urlParams}";
            string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config\\ihuan_header.txt");
            var fileLastWriteTime = File.GetLastWriteTime(headerFilePath);
            if (HeaderFileIsExpried && fileLastWriteTime <= HeaderFileLastWriteTime)
            {
                Console.WriteLine($"抓取免费IP代理作业异常：ihuan cookie 文件头未更新最新，暂不执行作业！");
                return;
            }
            var headerStrs = ReadAllLines(headerFilePath);
            var dictHeaders = headerStrs.Select(h => h.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(x => x[0], x => x[1]);
            string ipHtml = HttpHelper.GetResponseString(getIpUrl, dictHeaders);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(ipHtml);
            var ipTrs = htmlDoc.DocumentNode.SelectNodes("//div[2]//div[2]//table//tbody//tr");
            if (ipTrs == null)
            {
                HeaderFileIsExpried = true;
                HeaderFileLastWriteTime = fileLastWriteTime;
                NotifyHelper.Weixin("抓取免费IP代理作业异常", ipHtml);
                return;
            }
            foreach (var item in ipTrs)
            {
                try
                {
                    var ip = item.SelectSingleNode($"{item.XPath}//td[1]//a").InnerText;
                    var port = item.SelectSingleNode($"{item.XPath}//td[2]").InnerText;
                    Console.WriteLine($"抓取免费IP代理作业抓取到IP:{ip}:{port}");
                    RedisHelper.Set(IpProxyCacheKey, $"{ip}:{port}", "1");
                }
                catch (Exception ex)
                {
                    LogHelper.Log(ex);
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
                    FreeProxy01(href, nextPage);
                }
            }
            Console.WriteLine($"抓取免费IP代理作业抓取ihuan结束");
        }

        void FreeProxy02()
        {

        }

        string[] ReadAllLines(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllLines(path);
            }
            return new string[0];
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
