using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using My.App.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace My.App.Job
{
    public abstract class BaseJob : IHostedService, IDisposable
    {
        #region 初始化
        private Timer _timer;
        private TimeSpan _timerInterval;

        private ILogger logger;
        protected ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private IHostApplicationLifetime appLifetime;
        protected IHostApplicationLifetime AppLifetime
        {
            get { return appLifetime; }
            set { appLifetime = value; }
        }

        /// <summary>
        /// 作业是否运行中
        /// </summary>
        protected static Dictionary<string, bool> JobRunningStatus = new Dictionary<string, bool>();
        #endregion

        /// <summary>
        /// 作业基类
        /// </summary>
        /// <param name="timerInterval">作业运行的时间间隔</param>
        public BaseJob(TimeSpan timerInterval, ILogger<BaseJob> logger, IHostApplicationLifetime appLifetime)
        {
            _timerInterval = timerInterval;
            this.logger = logger;
            this.appLifetime = appLifetime;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(TimerCallback, null, TimeSpan.Zero, _timerInterval);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        void TimerCallback(object state)
        {
            var jobName = this.GetType().Name;
            lock (JobRunningStatus)
            {
                if (JobRunningStatus.ContainsKey(jobName) && JobRunningStatus[jobName])
                {
                    Console.WriteLine($"上次作业{jobName}暂未执行完毕，本次作业取消");
                    return;
                }
                JobRunningStatus[jobName] = true;
            }
            Console.WriteLine($"作业{jobName}执行开始");
            try
            {
                DoWork(state);
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }
            lock (JobRunningStatus)
            {
                JobRunningStatus[jobName] = false;
            }
            Console.WriteLine($"作业{jobName}执行结束");
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="state"></param>
        protected abstract void DoWork(object state);
    }
}
