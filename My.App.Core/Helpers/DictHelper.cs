using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace My.App.Core
{
    public class DictHelper
    {
        #region 配置
        private static string DictCacheKey = "My.App.Dict.Configs";

        private static RedisHelper _redisHelper;
        private static RedisHelper RedisHelper
        {
            get
            {
                if (_redisHelper == null)
                {
                    _redisHelper = new RedisHelper();
                }
                return _redisHelper;
            }
        }
        #endregion

        static DictHelper()
        {
            //var sub = RedisHelper.RedisClient.GetSubscriber();
            //sub.Subscribe(DictCacheKey, (channel, message) =>
            //{
            //    Console.WriteLine($"[{DateTime.Now.ToString("yyyyMMddHHmmssfff")}] {message}");
            //});
        }

        public static DictEnt Get(string key)
        {
            var dictText = RedisHelper.HashGet(DictCacheKey, key);
            if (string.IsNullOrWhiteSpace(dictText))
            {
                return null;
            }
            return JsonHelper.Deserialize<DictEnt>(dictText);
        }

        public static string GetValue(string key)
        {
            var dict = Get(key);
            if (dict == null)
            {
                return string.Empty;
            }
            return dict.Value;
        }

        public static List<DictEnt> GetAll()
        {
            var dictConfigs = RedisHelper.HashGetAll(DictCacheKey);
            return dictConfigs.Values.Select(x => JsonHelper.Deserialize<DictEnt>(x)).ToList();
        }
        public static bool Set(DictEnt dict)
        {
            if (string.IsNullOrWhiteSpace(dict.Key))
            {
                throw new ArgumentNullException("key IsNullOrWhiteSpace");
            }
            if (RedisHelper.HashExists(DictCacheKey, dict.Key))
            {
                throw new Exception("字典键已存在，请重新填写或修改已有字典");
            }
            var dictText = JsonHelper.Serialize(dict);
            return RedisHelper.HashSet(DictCacheKey, dict.Key, dictText);
        }
        public static bool Update(DictEnt dict)
        {
            if (string.IsNullOrWhiteSpace(dict.Key))
            {
                throw new ArgumentNullException("key IsNullOrWhiteSpace");
            }
            var dictText = JsonHelper.Serialize(dict);
            return RedisHelper.HashSet(DictCacheKey, dict.Key, dictText);
        }
        public static bool Del(string key)
        {
            return RedisHelper.HashDelete(DictCacheKey, key);
        }
    }

    public class DictEnt
    {
        /// <summary>
        /// 字典键
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 字典值
        /// </summary>

        public string Value { get; set; }

        /// <summary>
        /// 字典描述
        /// </summary>

        public string Desc { get; set; }
    }
}
