using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using My.App.Core;
using System.Linq;
using System.Threading.Tasks;

namespace My.App.Job
{
    public class MpAppPreheatJob : BaseJob
    {
        private static TimeSpan JobTimerInterval = TimeSpan.FromMinutes(30);
        public MpAppPreheatJob(ILogger<BaseJob> logger, IHostApplicationLifetime appLifetime) : base(JobTimerInterval, logger, appLifetime)
        {
        }

        protected override Task DoWork(object state)
        {
            Preheat();
            return Task.CompletedTask;
        }

        void Preheat()
        {
            string sitemapUrl = "http://tstunion.360kad.com/sitemap/toutiao/full.xml";
            var sitemapDoc = XDocument.Load(sitemapUrl);
            var productSitemapUrls = sitemapDoc.Element("sitemapindex").Elements("sitemap").Select(ele => ele.Element("loc").Value);
            var tasks = productSitemapUrls.Select(url =>
             {
                 return Task.Run(() =>
                 {
                     try
                     {
                         var productSitemapDoc = XDocument.Load(url);
                         var mpappUrls = productSitemapDoc.Element("DOCUMENT").Elements("item").Select(item => item.Element("display").Element("lightapp_url").Value);
                         foreach (var mpappUrl in mpappUrls)
                         {
                             string productUrl = mpappUrl.Replace("pages/details/main", "http://mpapp.360kad.com/Product/Detail");
                             var result = HttpHelper.Get(productUrl);
                             Console.WriteLine($"{productUrl}ִ执行完毕");
                             // 休眠200毫秒
                             Task.Delay(200);
                         }
                     }
                     catch (Exception ex)
                     {
                         LogHelper.Log(ex);
                     }
                 });
             }).ToArray();
            Task.WaitAll(tasks);

        }
    }
}
