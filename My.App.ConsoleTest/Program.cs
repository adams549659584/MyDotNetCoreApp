using My.App.Core;
using System;

namespace My.App.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRedisHelper();
            //TestLogHelper();
            //TestNotifyHelper();
            //TestUnicodeHelper();
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
            NotifyHelper.Weixin("克拉克订单", "卡的JFK啦奥克兰的放假啊");
        }

        static void TestUnicodeHelper()
        {
            string rawStr = @"\u4e0d\u8981\u91cd\u590d\u53d1\u9001\u540c\u6837\u7684\u5185\u5bb9";
            var chineseStr = UnicodeHelper.ToChinese(rawStr);
        }
    }
}
