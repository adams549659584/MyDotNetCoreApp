using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using My.App.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace My.App.Job
{
    public class TestJob : BaseJob
    {
        private static TimeSpan JobTimerInterval = TimeSpan.FromSeconds(2);
        public TestJob(ILogger<BaseJob> logger, IHostApplicationLifetime appLifetime) : base(JobTimerInterval, logger, appLifetime)
        {
            //base.Logger.Log(LogLevel.Debug, "测试作业启动");
            LogHelper.Log("测试作业启动");
        }

        protected override void DoWork(object state)
        {
            //base.Logger.Log(LogLevel.Debug, "测试作业执行：");
            LogHelper.Log("测试作业执行：");
            Test();
        }

        void Test()
        {
            Console.WriteLine(DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        }
    }
}
