using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using My.App.Core;

namespace My.App.Job
{
    public class IpPushJob : BaseJob
    {
        private static TimeSpan JobTimerInterval = TimeSpan.FromMinutes(30);
        private static RedisHelper RedisHelper = new RedisHelper("dotnetcore_redis:6379");
        public IpPushJob(ILogger<BaseJob> logger, IHostApplicationLifetime appLifetime) : base(JobTimerInterval, logger, appLifetime)
        {
            //base.Logger.Log(LogLevel.Debug, "测试作业启动");
            LogHelper.Log("IP推送作业启动");
        }

        protected override void DoWork(object state)
        {
            //base.Logger.Log(LogLevel.Debug, "测试作业执行：");
            //LogHelper.Log("IP推送作业执行：");
            IpPush();
        }

        void IpPush()
        {
            string getIpUrl = "http://ip-api.com/line/?fields=query";
            string nowIp = HttpHelper.GetResponseString(getIpUrl).Trim();
            string ipCacheKey = "My.App.Job.IpPush.LastIp";
            string oldIp = RedisHelper.Get<string>(ipCacheKey);
            // Console.WriteLine($"{DateTime.Now.ToString("yyyyMMddHHmmssfff")}：{oldIp}");
            // 每次设置下，不然缓存过期了就推送了
            RedisHelper.Set(ipCacheKey, nowIp, 60);
            if (nowIp == oldIp)
            {
                Console.WriteLine($"IpPushJob：IP 【{nowIp}】未变更，无需通知");
            }
            else
            {
                string notifyTitle = "Ip变更通知";
                string notifyBody = $"您的外网ip变更为{nowIp}了";
                Console.WriteLine($"IpPushJob：{notifyBody}");
                NotifyHelper.Weixin(notifyTitle, notifyBody);
            }
        }
    }
}
