using ServiceStack.Caching;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace My.App.Core
{
    public class RedisHelper
    {
        private readonly RedisClient redisClient;
        private ICacheClient cacheClient;

        public object this[string key]
        {
            get
            {
                return this.Get(key);
            }
            set
            {
                if (!this.Set(key, value, 24))
                    throw new Exception("写入缓存失败");
            }
        }

        public RedisHelper()
        {
            this.redisClient = new RedisClient("192.168.1.88", 6379);
            this.cacheClient = (ICacheClient)this.redisClient;
        }

        public RedisHelper(string host)
        {
            this.redisClient = new RedisClient(host);
            this.cacheClient = (ICacheClient)this.redisClient;
        }

        public bool Delete(string key)
        {
            return this.cacheClient.Remove(this.GetKey(key));
        }

        public bool Exists(string key)
        {
            return this.redisClient.Exists(this.GetKey(key)) > 0L;
        }

        public object Get(string key)
        {
            return this.cacheClient.Get<object>(this.GetKey(key));
        }

        public IDictionary<string, T> Get<T>(string[] keys) where T : class
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentNullException("keys");
            for (int index = 0; index < keys.Length; ++index)
                keys[index] = this.GetKey(keys[index]);
            return this.cacheClient.GetAll<T>((IEnumerable<string>)keys);
        }

        public T Get<T>(string key) where T : class
        {
            return this.cacheClient.Get<T>(this.GetKey(key));
        }

        public bool Set(string key, object value, int minute = 24)
        {
            TimeSpan expiresIn = new TimeSpan(0, 0, minute, 0, 0);
            return this.cacheClient.Set<object>(key, value, expiresIn);
        }
        public bool Set(string key, object value, TimeSpan expiresIn)
        {
            if (expiresIn == null)
                throw new ArgumentNullException("过期时间不能为空");
            return this.cacheClient.Set<object>(key, value, expiresIn);
        }
        public long TTL(string key)
        {
            return this.redisClient.Ttl(key);
        }
        private string GetKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException("键不能为空");
            if (key.Length > 250)
                throw new ArgumentException("键值长度不能超过250位");
            return key;
        }
    }
}
