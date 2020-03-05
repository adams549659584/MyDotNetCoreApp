using HtmlAgilityPack;
using MongoDB.Driver;
using My.App.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace My.App.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestRedisHelper();
            //TestLogHelper();
            // TestNotifyHelper();
            //TestUnicodeHelper();
            //TestHttpHelper();
            //TestTask();
            //TestDictHelper();
            //TestMongoDB();
            // TestPanDownload();
            TestML();
            Console.ReadLine();
        }

        static void TestRedisHelper()
        {
            var redisHelper = new RedisHelper();
            var cacheKey = "My.App.ConsoleTest";
            var setResult = redisHelper.Set(cacheKey, 1);
            var cacheResult = redisHelper.Get<int>(cacheKey);
            var timeExpried = redisHelper.KeyTimeToLive(cacheKey);
            var delResult = redisHelper.Delete(cacheKey);
            var dictHashResults = redisHelper.HashGetAll("useful_proxy");
            Console.WriteLine(cacheResult);
        }

        static void TestLogHelper()
        {
            LogHelper.Log("测试普通日志信息");
            try
            {
                throw new ArgumentException("测试无参数异常日志信息");
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }

        }

        static void TestNotifyHelper()
        {
            var kadHtml = HttpHelper.Get("http://m.360kad.com");
            NotifyHelper.Weixin("测试一下大数据", new MarkdownBuilder().AppendText("康爱多网页", TextStyleType.Heading4).AppendCode(kadHtml, "html"));
        }

        static void TestUnicodeHelper()
        {
            string rawStr = @"\u4e0d\u8981\u91cd\u590d\u53d1\u9001\u540c\u6837\u7684\u5185\u5bb9";
            var chineseStr = UnicodeHelper.ToChinese(rawStr);
        }

        static void TestHttpHelper()
        {
            RedisHelper redisHelper = new RedisHelper();
            string IpProxyCacheKey = "useful_proxy";
            var proxyIps = redisHelper.HashGetAll(IpProxyCacheKey);
            foreach (var proxyIp in proxyIps.Keys)
            {
                try
                {
                    string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "test_header.txt");
                    var headerStrs = ReadAllLines(headerFilePath);
                    var dictHeaders = headerStrs.Select(h => h.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(x => x[0], x => x[1]);
                    var result = HttpHelper.Get("http://httpbin.org/ip", dictHeaders, 5 * 1000, new System.Net.WebProxy($"http://{proxyIp}"));
                    Console.WriteLine($"{proxyIp}:{result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{proxyIp}:{ex.Message}");
                }
            }
        }

        static string[] ReadAllLines(string path)
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

        static void TestTask()
        {
            Console.WriteLine($"step1,线程ID：{Thread.CurrentThread.ManagedThreadId}");
            AsyncDemo asyncDemo = new AsyncDemo();
            //asyncDemo.AsyncSleep().Wait();//Wait会阻塞当前线程直到AsyncSleep返回
            asyncDemo.AsyncSleep();//不会阻塞当前线程
            Console.WriteLine($"step5，线程ID：{Thread.CurrentThread.ManagedThreadId}");
        }

        static void TestDictHelper()
        {
            // var dict = DictHelper.Get("My.App.ConsoleTest");
            // var redisHelper = new RedisHelper();
            //var sub = redisHelper.RedisClient.GetSubscriber();
            //sub.Publish("My.App.Dict.Configs", "试试试试试试");
            string dictKeyIHuan = "My.App.Job.GetFreeProxyJob.ProxyIpMaxPage.IHuan";
            DictHelper.Update(new DictEnt()
            {
                Key = dictKeyIHuan,
                Value = "30",
                Desc = "IHuan代理ip最大抓取页数"
            });
            string dictKey89Ip = "My.App.Job.GetFreeProxyJob.ProxyIpMaxPage.89Ip";
            DictHelper.Update(new DictEnt()
            {
                Key = dictKey89Ip,
                Value = "30",
                Desc = "89Ip代理ip最大抓取页数"
            });
            string dictKeyXiLa = "My.App.Job.GetFreeProxyJob.ProxyIpMaxPage.XiLa";
            DictHelper.Update(new DictEnt()
            {
                Key = dictKeyXiLa,
                Value = "100",
                Desc = "西拉免费代理IP最大抓取页数"
            });
        }

        static void TestMongoDB()
        {
            //mongodb+srv://<username>:<password>@<cluster-address>/test?w=majority
            //var connectionString = $"mongodb://mongodb:mongo.123456.db@192.168.124.10:27017";
            var connectionString = $"mongodb://192.168.1.88:27019/Kad_Web";
            var mongoDbService = new MongoDBServiceBase(connectionString, new MongoUrl(connectionString).DatabaseName);
            //var lists = new List<MongoTestEnt>();
            //for (int i = 0; i < 10; i++)
            //{
            //    lists.Add(new MongoTestEnt()
            //    {
            //        Id = Guid.NewGuid().ToString("N"),
            //        Name = $"test{i}",
            //        Age = i,
            //        CreateTime = DateTime.Now
            //    });
            //}
            //mongoDbService.InsertMany<MongoTestEnt>(lists);
            var lists = mongoDbService.GetList<MongoTestEnt>();
        }

        static async void TestPanDownload()
        {
            var linkTrees = PanDownloadToAria2("https://www.baiduwp.com/s/1-080vZwjwzA9r13wR5sB9A?pwd=hr06&path=%2F网课%2FPython以及其他视频");
            var linkJson = JsonHelper.Serialize(linkTrees);
        }

        static List<DownloadLinkTree> PanDownloadToAria2(string downloadUrl = "https://www.baiduwp.com/s/1-080vZwjwzA9r13wR5sB9A?pwd=hr06&path=%2F网课%2FPython以及其他视频")
        {
            string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "test_header.txt");
            var headerStrs = ReadAllLines(headerFilePath);
            var dictHeaders = headerStrs.Select(h => h.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(x => x[0], x => x[1]);
            System.Net.WebProxy proxy = new System.Net.WebProxy($"http://127.0.0.1:8888");
            var downloadHtml = HttpHelper.Get(downloadUrl, dictHeaders, 30 * 1000, proxy);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(downloadHtml);
            var downloadLinkContainers = htmlDoc.DocumentNode.SelectNodes("/html/body/div[1]/div[1]/ul/li/a");
            var downloadTrees = new List<DownloadLinkTree>();
            if (downloadLinkContainers != null && downloadLinkContainers.Count > 0)
            {
                foreach (var linkContainer in downloadLinkContainers)
                {
                    var downloadLink = linkContainer.GetAttributeValue("href", "");
                    var downloadName = linkContainer.InnerText;
                    Console.WriteLine($"{downloadName}:{downloadLink}");
                    var tree = new DownloadLinkTree()
                    {
                        Link = downloadLink,
                        Name = linkContainer.InnerText
                    };
                    if (downloadLink.Contains("javascript:void(0)"))
                    {
                        downloadLink = linkContainer.GetAttributeValue("onclick", "");
                        downloadLink = downloadLink.Replace("if (!window.__cfRLUnblockHandlers) return false;", "");
                    }
                    else
                    {
                        tree.Childrens = PanDownloadToAria2($"https://www.baiduwp.com{downloadLink}");
                    }
                    downloadTrees.Add(tree);
                }
            }
            return downloadTrees;
        }

       static void TestML() 
        {
            try
            {
                string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "test_header.txt");
                var headerStrs = ReadAllLines(headerFilePath);
                var dictHeaders = headerStrs.Select(h => h.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(x => x[0], x => x[1]);
                var proxyIp = "210.22.247.196:8090";
                // var proxyIp = "192.168.124.10:8090";
                var result = HttpHelper.Get("http://httpbin.org/ip", dictHeaders, 5 * 1000, new System.Net.WebProxy($"http://{proxyIp}"));
                Console.WriteLine($"{proxyIp}:{result}");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class MongoTestEnt
    {
        public string Id { get; set; }

        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public class DownloadLinkTree
    {
        public string Link { get; set; }
        public string Name { get; set; }
        public List<DownloadLinkTree> Childrens { get; set; }
    }
}
