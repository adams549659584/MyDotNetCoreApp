using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;

namespace My.App.Core
{
    public class RedisService : IDisposable
    {
        #region 初始化
        private string Configuration { get; }
        private ConnectionMultiplexer _redisClient;
        public ConnectionMultiplexer RedisClient
        {
            get
            {
                if (_redisClient == null)
                {
                    _redisClient = ConnectionMultiplexer.Connect(Configuration);
                }
                _redisClient.GetDatabase();
                return _redisClient;
            }
        }
        public IDatabase RedisDB
        {
            get
            {
                return RedisClient.GetDatabase();
            }
        }
        public ISubscriber Subscriber
        {
            get
            {
                return RedisClient.GetSubscriber();
            }
        }
        #endregion

        public RedisService()
        {
            this.Configuration = "dotnetcore_redis:6379,abortConnect=false";
        }
        public RedisService(string configuration)
        {
            this.Configuration = configuration;
        }

        public void Dispose()
        {
            this.RedisClient.Dispose();
        }
    }
}
