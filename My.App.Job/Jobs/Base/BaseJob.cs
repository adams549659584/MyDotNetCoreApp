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
            try
            {
                DoWork(state);
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        /// <param name="state"></param>
        protected abstract void DoWork(object state);
    }
}
