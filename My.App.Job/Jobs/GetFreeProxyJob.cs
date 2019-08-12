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
using System.Web;
using System.Diagnostics;

namespace My.App.Job
{
    public class GetFreeProxyJob : BaseJob
    {
        private static TimeSpan JobTimerInterval = TimeSpan.FromMinutes(20);
        private static RedisHelper RedisHelper = new RedisHelper("dotnetcore_redis:6379");
        private static MongoDBServiceBase MongoDBServiceBase = new MongoDBServiceBase("MyJob");
        private static string IpProxyCacheKey = "useful_proxy";

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

        private static List<ProxyIpEnt> RawProxyIpList = new List<ProxyIpEnt>();

        private static string CurrentIp
        {
            get
            {
                return RedisHelper.Get<string>("My.App.Job.IpPush.LastIp");
            }
        }

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
            var proxyConfigFullPath = PathHelper.MapFile("Config", "proxyConfig.jsonc");
            var proxyConfigJson = File.ReadAllText(proxyConfigFullPath);
            var proxyConfigs = string.IsNullOrWhiteSpace(proxyConfigJson) ? new List<ProxyConfigEnt>() : JsonHelper.Deserialize<List<ProxyConfigEnt>>(proxyConfigJson);
            var freeProxyTasks = proxyConfigs.Select(proxy =>
            {
                return FreeProxyCommon(proxy, 1, usefulProxyIps.Clone());
            }).ToList();
            // freeProxyTasks.Add(FreeProxyIHuan("", 1, usefulProxyIps.Clone()));
            Task.WaitAll(freeProxyTasks.ToArray());
            ValidProxyIps();
            RawProxyIps.Clear();

            //TestValidProxyIps();
        }

        /// <summary>
        /// https://ip.ihuan.me
        /// </summary>
        async Task FreeProxyIHuan(string urlParams = "", int page = 1, List<string> usefulProxyIps = null)
        {
            try
            {
                var proxyIpMaxPage = DictHelper.GetValue("My.App.Job.GetFreeProxyJob.ProxyIpMaxPage.ihuan").ToInt(5);
                if (page <= proxyIpMaxPage)
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
                            ipHtml = await HttpHelper.GetAsync(getIpUrl, dictHeaders, 10 * 1000, new WebProxy($"http://{currProxyIp}"));
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
                        NotifyHelper.Weixin("抓取免费IP代理作业 ihuan 异常", new MarkdownBuilder().AppendHtml(ipHtml));
                        return;
                    }
                    foreach (var item in ipTrs)
                    {
                        try
                        {
                            var ip = item.SelectSingleNode($"{item.XPath}//td[1]//a").InnerText.Trim();
                            var port = item.SelectSingleNode($"{item.XPath}//td[2]").InnerText.Trim();
                            var location = item.SelectSingleNode($"{item.XPath}//td[3]").InnerText.Trim();
                            var proxyType = item.SelectSingleNode($"{item.XPath}//td[5]").InnerText.Trim();
                            var anonymity = item.SelectSingleNode($"{item.XPath}//td[7]").InnerText.Trim();
                            var proxyIpEnt = new ProxyIpEnt()
                            {
                                Id = Guid.NewGuid(),
                                IP = ip,
                                Port = port.ToInt(),
                                Location = HttpUtility.HtmlDecode(location),
                                Anonymity = anonymity
                            };
                            RawProxyIpList.Add(proxyIpEnt);
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
                            nextPage = pageEles[i].InnerText.ToInt(nextPage);
                            if (nextPage > page)
                            {
                                break;
                            }
                        }
                        if (nextPage > 0)
                        {
                            await FreeProxyIHuan(href, nextPage, usefulProxyIps);
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
        async Task FreeProxy89Ip(int page = 1, List<string> usefulProxyIps = null)
        {
            try
            {
                var proxyIpMaxPage = DictHelper.GetValue("My.App.Job.GetFreeProxyJob.ProxyIpMaxPage.89ip").ToInt(5);
                if (page <= proxyIpMaxPage)
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
                            ipHtml = await HttpHelper.GetAsync(getIpUrl, dictHeaders, 10 * 1000, new WebProxy($"http://{currProxyIp}"));
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
                            NotifyHelper.Weixin("抓取免费IP代理作业 89ip 异常", new MarkdownBuilder().AppendHtml(ipHtml));
                        }
                        return;
                    }
                    foreach (var item in ipTrs)
                    {
                        try
                        {
                            var ip = item.SelectSingleNode($"{item.XPath}//td[1]").InnerText.Trim();
                            var port = item.SelectSingleNode($"{item.XPath}//td[2]").InnerText.Trim();
                            var location = item.SelectSingleNode($"{item.XPath}//td[3]").InnerText.Trim();
                            var proxyIpEnt = new ProxyIpEnt()
                            {
                                Id = Guid.NewGuid(),
                                IP = ip,
                                Port = port.ToInt(),
                                Location = HttpUtility.HtmlDecode(location)
                            };
                            RawProxyIpList.Add(proxyIpEnt);
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
                            nextPage = pageEles[i].InnerText.ToInt(nextPage);
                            if (nextPage > page)
                            {
                                break;
                            }
                        }
                        if (nextPage > 0)
                        {
                            await FreeProxy89Ip(nextPage, usefulProxyIps);
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

        /// <summary>
        /// http://www.xiladaili.com/gaoni/
        /// </summary>
        /// <returns></returns>
        async Task FreeProxyXiLa(int page = 1, List<string> usefulProxyIps = null)
        {
            try
            {
                var proxyIpMaxPage = DictHelper.GetValue("My.App.Job.GetFreeProxyJob.ProxyIpMaxPage.xila").ToInt(5);
                if (page <= proxyIpMaxPage)
                {
                    Console.WriteLine($"抓取免费IP代理作业开始抓取XiLa第{page}页:");
                    string getIpUrl = $"http://www.xiladaili.com/gaoni/{page}/";
                    string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "xila_header.txt");
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
                        Console.WriteLine($"抓取免费IP代理作业异常：XiLa cookie 文件头未更新最新，暂不执行作业！");
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
                            Console.WriteLine($"抓取免费IP代理作业 XiLa 当前使用代理Ip：{currProxyIp}");
                            ipHtml = await HttpHelper.GetAsync(getIpUrl, dictHeaders, 10 * 1000, new WebProxy($"http://{currProxyIp}"));
                            usefulProxyIps.RemoveAll(x => x == currProxyIp);
                            usefulProxyIps.Insert(0, currProxyIp);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"XiLa 使用代理Ip {currProxyIp} 异常： {ex.Message}");
                            // Console.WriteLine(ex.ToString());
                            // LogHelper.Log(ex);
                        }
                    }
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(ipHtml);
                    var ipTrs = htmlDoc.DocumentNode.SelectNodes("//html//body//div//div[3]//div[2]//table//tbody//tr");
                    if (ipTrs == null)
                    {
                        if (ipHtml.Contains("高匿ip非国外免费代理服务器"))
                        {
                            Console.WriteLine($"抓取免费IP代理作业抓取XiLa结束");
                        }
                        else
                        {
                            // HeaderFilesIsExpried[headerFilePath] = true;
                            // HeaderFilesLastWriteTime[headerFilePath] = fileLastWriteTime;
                            Console.WriteLine("抓取免费IP代理作业 XiLa 异常：");
                            Console.WriteLine(ipHtml);
                            NotifyHelper.Weixin("抓取免费IP代理作业 XiLa 异常", new MarkdownBuilder().AppendHtml(ipHtml));
                        }
                        return;
                    }
                    foreach (var item in ipTrs)
                    {
                        try
                        {
                            var ipAndPort = item.SelectSingleNode($"{item.XPath}//td[1]").InnerText.Trim();
                            var ipAndPortArr = ipAndPort.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            var ip = ipAndPortArr[0];
                            var port = ipAndPortArr[1];
                            var location = item.SelectSingleNode($"{item.XPath}//td[4]").InnerText.Trim();
                            var proxyIpEnt = new ProxyIpEnt()
                            {
                                Id = Guid.NewGuid(),
                                IP = ip,
                                Port = port.ToInt(),
                                Location = HttpUtility.HtmlDecode(location)
                            };
                            RawProxyIpList.Add(proxyIpEnt);
                            Console.WriteLine($"抓取免费IP代理作业 XiLa 抓取到IP:{ipAndPort}");
                            // RedisHelper.Set(IpProxyCacheKey, $"{ipAndPort}", "1");
                            RawProxyIps[$"{ipAndPort}"] = false;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            // LogHelper.Log(ex);
                        }
                    }
                    var pageEles = htmlDoc.DocumentNode.SelectNodes("//html//body//div//div[3]//nav//ul//li//a");
                    if (pageEles.Count > 2)
                    {
                        int nextPage = 0;
                        for (int i = 1; i < (pageEles.Count - 1); i++)
                        {
                            nextPage = pageEles[i].InnerText.ToInt(nextPage);
                            if (nextPage > page)
                            {
                                break;
                            }
                        }
                        if (nextPage > 0)
                        {
                            await FreeProxyXiLa(nextPage, usefulProxyIps);
                            return;
                        }
                    }
                }
                Console.WriteLine($"抓取免费IP代理作业抓取XiLa结束");
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }
        }

        /// <summary>
        /// 通用获取代理 document.evaluate("//div[@id='list']//table//tbody//tr", document, null, XPathResult.ANY_TYPE, null).iterateNext().innerText
        /// </summary>
        /// <returns></returns>
        async Task FreeProxyCommon(ProxyConfigEnt proxyConfig, int page = 1, List<string> usefulProxyIps = null)
        {
            try
            {
                string proxyConfigCode = proxyConfig.Code;
                if (page <= proxyConfig.MaxPage)
                {
                    Console.WriteLine($"抓取免费IP代理作业开始抓取{proxyConfigCode}第{page}页:");
                    string getIpUrl = string.Format(proxyConfig.FormatUrl, page);
                    string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", $"{proxyConfigCode}_header.txt");
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
                        Console.WriteLine($"抓取免费IP代理作业异常：{proxyConfigCode} cookie 文件头未更新最新，暂不执行作业！");
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
                            Console.WriteLine($"抓取免费IP代理作业 {proxyConfigCode} 当前使用代理Ip：{currProxyIp}");
                            ipHtml = await HttpHelper.GetAsync(getIpUrl, dictHeaders, 10 * 1000, new WebProxy($"http://{currProxyIp}"));
                            if (!string.IsNullOrEmpty(proxyConfig.FailedKeyWords) && ipHtml.Contains(proxyConfig.FailedKeyWords))
                            {
                                Console.WriteLine($"{proxyConfigCode} 使用代理Ip {currProxyIp} 捕获失败关键词：{proxyConfig.FailedKeyWords}，响应内容：{ipHtml}");
                                continue;
                            }
                            usefulProxyIps.RemoveAll(x => x == currProxyIp);
                            usefulProxyIps.Insert(0, currProxyIp);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{proxyConfigCode} 使用代理Ip {currProxyIp} 异常： {ex.Message}");
                            // Console.WriteLine(ex.ToString());
                            // LogHelper.Log(ex);
                        }
                    }
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(ipHtml);
                    var ipTrs = htmlDoc.DocumentNode.SelectNodes(proxyConfig.RowXPath);
                    if (ipTrs == null)
                    {
                        if (ipHtml.Contains("高匿ip非国外免费代理服务器"))
                        {
                            Console.WriteLine($"抓取免费IP代理作业抓取{proxyConfigCode}结束");
                        }
                        else
                        {
                            // HeaderFilesIsExpried[headerFilePath] = true;
                            // HeaderFilesLastWriteTime[headerFilePath] = fileLastWriteTime;
                            Console.WriteLine($"抓取免费IP代理作业 {proxyConfigCode} 异常：");
                            Console.WriteLine(ipHtml);
                            NotifyHelper.Weixin($"抓取免费IP代理作业 {proxyConfigCode} 异常", new MarkdownBuilder().AppendHtml(ipHtml));
                        }
                        return;
                    }
                    foreach (var item in ipTrs)
                    {
                        try
                        {
                            string ip = string.Empty;
                            string port = string.Empty;
                            if (proxyConfig.IsUnionIpAndPort)
                            {
                                var ipAndPort = item.SelectSingleNode($"{item.XPath}{proxyConfig.IpXPath}").InnerText.Trim();
                                var ipAndPortArr = ipAndPort.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                                ip = ipAndPortArr[0];
                                port = ipAndPortArr[1];
                            }
                            else
                            {
                                ip = item.SelectSingleNode($"{item.XPath}{proxyConfig.IpXPath}").InnerText.Trim();
                                port = item.SelectSingleNode($"{item.XPath}{proxyConfig.PortXPath}").InnerText.Trim();
                            }
                            var location = item.SelectSingleNode($"{item.XPath}{proxyConfig.LocationXPath}").InnerText.Trim();
                            var proxyIpEnt = new ProxyIpEnt()
                            {
                                Id = Guid.NewGuid(),
                                IP = ip,
                                Port = port.ToInt(),
                                Location = HttpUtility.HtmlDecode(location)
                            };
                            RawProxyIpList.Add(proxyIpEnt);
                            Console.WriteLine($"抓取免费IP代理作业 {proxyConfigCode} 抓取到IP:{ip}:{port}");
                            // RedisHelper.Set(IpProxyCacheKey, $"{ipAndPort}", "1");
                            RawProxyIps[$"{ip}:{port}"] = false;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                            // LogHelper.Log(ex);
                        }
                    }
                    var pageEles = htmlDoc.DocumentNode.SelectNodes(proxyConfig.NextPageXPath);
                    if (pageEles.Count > 2)
                    {
                        int nextPage = 0;
                        for (int i = 1; i < (pageEles.Count - 1); i++)
                        {
                            nextPage = pageEles[i].InnerText.ToInt(nextPage);
                            if (nextPage > page)
                            {
                                break;
                            }
                        }
                        if (nextPage > 0)
                        {
                            await FreeProxyCommon(proxyConfig, nextPage, usefulProxyIps);
                            return;
                        }
                    }
                }
                Console.WriteLine($"抓取免费IP代理作业抓取{proxyConfigCode}结束");
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
            Console.WriteLine($"开始校验代理ip是否可用，当前需校验ip数量为{RawProxyIpList.Count}");
            if (RawProxyIpList.Count > 0)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();
                int threadCount = DictHelper.GetValue("My.App.Job.GetFreeProxyJob.ValidProxyIp.ThreadCount").ToInt(50);
                int pageCount = (int)Math.Ceiling(RawProxyIpList.Count / (decimal)threadCount);
                // http
                var httpTaskList = new Task<List<string>>[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    var proxyIps = RawProxyIpList.Skip(i * pageCount).Take(pageCount).ToArray();
                    httpTaskList[i] = Task.Run(() => ValidProxyIps(proxyIps, ProxyCheckType.Http));
                }
                var httpTaskResults = Task.WhenAll(httpTaskList).Result;
                watch.Stop();
                var httpUsefulProxyIps = httpTaskResults.SelectMany(x => x).ToList();
                Console.WriteLine($"全部代理IP({RawProxyIpList.Count})HTTP检查完毕，有效IP({httpUsefulProxyIps.Count})，耗时：{watch.Elapsed.TotalSeconds} 秒");

                // https
                watch.Restart();
                var httpsTaskList = new Task<List<string>>[threadCount];
                for (int i = 0; i < threadCount; i++)
                {
                    var proxyIps = RawProxyIpList.Skip(i * pageCount).Take(pageCount).ToArray();
                    httpsTaskList[i] = Task.Run(() => ValidProxyIps(proxyIps, ProxyCheckType.Https));
                }
                var httpsTaskResults = Task.WhenAll(httpsTaskList).Result;
                watch.Stop();
                var httpsUsefulProxyIps = httpsTaskResults.SelectMany(x => x).ToList();
                Console.WriteLine($"全部代理IP({RawProxyIpList.Count})HTTPS检查完毕，有效IP({httpsUsefulProxyIps.Count})，耗时：{watch.Elapsed.TotalSeconds} 秒");
            }
            else
            {
                Console.WriteLine($"结束校验代理ip是否可用，当前共抓取IP数量：0");
            }
        }

        void TestValidProxyIps()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            int checkProxyIpCount = 12;
            var RawProxyIps = RedisHelper.HashGetAll(IpProxyCacheKey).Keys.Take(checkProxyIpCount).Select(x =>
            {
                var ips = x.Split(":");
                var ent = new ProxyIpEnt()
                {
                    IP = ips[0],
                    Port = ips[1].ToInt()
                };
                return ent;
            }).ToList();
            int threadCount = 12;
            int pageCount = (int)Math.Ceiling(RawProxyIps.Count / (decimal)threadCount);
            // http
            var httpTaskList = new Task<List<string>>[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var proxyIps = RawProxyIps.Skip(i * pageCount).Take(pageCount).ToArray();
                httpTaskList[i] = ValidProxyIps(proxyIps, ProxyCheckType.Http);
            }
            var httpTaskResults = Task.WhenAll(httpTaskList).Result;
            watch.Stop();
            var httpUsefulProxyIps = httpTaskResults.SelectMany(x => x).ToList();
            Console.WriteLine($"全部代理IP({checkProxyIpCount})HTTP检查完毕，有效IP({httpUsefulProxyIps.Count})，耗时：{watch.Elapsed.TotalSeconds} 秒");

            // https
            watch.Restart();
            var httpsTaskList = new Task<List<string>>[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                var proxyIps = RawProxyIps.Skip(i * pageCount).Take(pageCount).ToArray();
                httpsTaskList[i] = ValidProxyIps(proxyIps, ProxyCheckType.Https);
            }
            var httpsTaskResults = Task.WhenAll(httpsTaskList).Result;
            watch.Stop();
            var httpsUsefulProxyIps = httpsTaskResults.SelectMany(x => x).ToList();
            Console.WriteLine($"全部代理IP({checkProxyIpCount})HTTPS检查完毕，有效IP({httpsUsefulProxyIps.Count})，耗时：{watch.Elapsed.TotalSeconds} 秒");
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
                try
                {
                    stopwatch.Restart();
                    var resultIp = await HttpHelper.GetAsync(checkUrl, null, 5 * 1000, new WebProxy($"http://{proxyIpEnt.IP}:{proxyIpEnt.Port}"));
                    stopwatch.Stop();
                    if (resultIp.Contains("origin"))
                    {
                        RedisHelper.HashSet(IpProxyCacheKey, $"{proxyIpEnt.IP}:{proxyIpEnt.Port}", "0");
                        usefulProxyIps.Add($"{proxyIpEnt.IP}:{proxyIpEnt.Port}");
                        Console.WriteLine($"代理IP：{proxyIpEnt.IP}:{proxyIpEnt.Port} 通过{checkTypeName}校验");
                        await MongoDBServiceBase.GetList<ProxyIpEnt>(x => x.IP == proxyIpEnt.IP && x.Port == proxyIpEnt.Port)
                        .ContinueWith(queryResult =>
                        {
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
                            if (queryResult.Result.Count == 0)
                            {
                                proxyIpEnt.Id = Guid.NewGuid();
                                proxyIpEnt.CreateTime = DateTime.Now;
                                MongoDBServiceBase.Insert(proxyIpEnt)
                                .ContinueWith(insertResult =>
                                {
                                    Console.WriteLine($"{proxyIpEnt.IP}:{proxyIpEnt.Port}插入mongodb成功");
                                });
                            }
                            else
                            {
                                var oldProxyIpEnt = queryResult.Result.FirstOrDefault();
                                if (proxyCheckType == ProxyCheckType.Https)
                                {
                                    proxyIpEnt.Speed = oldProxyIpEnt.Speed;
                                }
                                proxyIpEnt.Id = oldProxyIpEnt.Id;
                                proxyIpEnt.CreateTime = oldProxyIpEnt.CreateTime;
                                MongoDBServiceBase.Replace(proxyIpEnt)
                                .ContinueWith(replaceResult =>
                                {
                                    Console.WriteLine($"{proxyIpEnt.IP}:{proxyIpEnt.Port}更新mongodb成功");
                                });
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    // Console.WriteLine(ex.Message);
                    // Console.WriteLine(ex.ToString());
                    Console.WriteLine($"代理IP：{proxyIpEnt.IP}:{proxyIpEnt.Port} 未通过{checkTypeName}校验：{ex.Message}");
                }
            }
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
