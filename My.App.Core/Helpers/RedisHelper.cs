using System;
using System.Collections.Generic;
using System.Text;
using StackExchange.Redis;
using System.Linq;

namespace My.App.Core
{
    public class RedisHelper
    {
        #region 初始化
        private string _configuration = string.Empty;
        private ConnectionMultiplexer _redisClient;
        public ConnectionMultiplexer RedisClient
        {
            get
            {
                if (_redisClient == null)
                {
                    _redisClient = ConnectionMultiplexer.Connect(_configuration);
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
        #endregion

        //private readonly RedisClient redisClient;
        //private ICacheClient cacheClient;

        public object this[string key]
        {
            get
            {
                return this.Get<object>(key);
            }
            set
            {
                if (!this.Set(key, value, 24))
                    throw new Exception("写入缓存失败");
            }
        }

        public RedisHelper()
        {
            this._configuration = "dotnetcore_redis:6379,abortConnect=false";
        }

        public RedisHelper(string configuration)
        {
            this._configuration = configuration;
        }

        public bool Delete(string key)
        {
            return RedisDB.KeyDelete(key);
        }
        public bool Delete(string[] keys)
        {
            var redisKeys = new RedisKey[keys.Length];
            keys.CopyTo(redisKeys, 0);
            return RedisDB.KeyDelete(redisKeys) > 0;
        }
        public bool HashDelete(string key, string hashField)
        {
            return RedisDB.HashDelete(key, hashField);
        }

        public bool HashDelete(string key, string[] hashFields)
        {
            var redisValues = new RedisValue[hashFields.Length];
            hashFields.CopyTo(redisValues, 0);
            return RedisDB.HashDelete(key, redisValues) > 0;
        }

        public bool Exists(string key)
        {
            return RedisDB.KeyExists(key);
        }

        public bool HashExists(string key, string hashField)
        {
            return RedisDB.HashExists(key, hashField);
        }

        public T Get<T>(string key)
        {
            var redisValue = RedisDB.StringGet(key);
            if (redisValue.HasValue)
            {
                return JsonHelper.Deserialize<T>(redisValue);
            }
            return default(T);
        }

        public bool Set(string key, object value, int minute = 24)
        {
            TimeSpan expiresIn = new TimeSpan(0, 0, minute, 0, 0);
            return this.Set(key, value is string ? value : JsonHelper.Serialize(value), expiresIn);
        }
        public bool Set(string key, object value, TimeSpan expiresIn)
        {
            if (expiresIn == null)
                throw new ArgumentNullException("过期时间不能为空");
            return RedisDB.StringSet(key, JsonHelper.Serialize(value), expiresIn);
        }
        public bool HashSet(string key, string hashField, string value)
        {
            return RedisDB.HashSet(key, hashField, value);
        }
        public string HashGet(string key, string hashField)
        {
            return RedisDB.HashGet(key, hashField);
        }
        public Dictionary<string, string> HashGetAll(string key)
        {
            var hashEntrys = RedisDB.HashGetAll(key);
            return hashEntrys.ToDictionary(h => h.Name.HasValue ? h.Name.ToString() : "", h => h.Value.HasValue ? h.Value.ToString() : "");
        }
        public TimeSpan? KeyTimeToLive(string key)
        {
            return RedisDB.KeyTimeToLive(key);
        }
    }
}
