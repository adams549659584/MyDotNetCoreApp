﻿using My.App.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace My.App.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //TestRedisHelper();
            //TestLogHelper();
            //TestNotifyHelper();
            //TestUnicodeHelper();
            TestHttpHelper();
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

        static void TestHttpHelper()
        {
            RedisHelper redisHelper = new RedisHelper();
            string IpProxyCacheKey = "useful_proxy";
            var proxyIps = redisHelper.HashGetAll(IpProxyCacheKey);
            foreach (var proxyIp in proxyIps.Keys)
            {
                try
                {
                    string headerFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "test_header.txt");
                    var headerStrs = ReadAllLines(headerFilePath);
                    var dictHeaders = headerStrs.Select(h => h.Split(new string[] { ": " }, StringSplitOptions.RemoveEmptyEntries)).ToDictionary(x => x[0], x => x[1]);
                    var result = HttpHelper.Get("http://httpbin.org/ip", dictHeaders, 5*1000, new System.Net.WebProxy($"http://{proxyIp}"));
                    Console.WriteLine($"{proxyIp}:{result}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{proxyIp}:{ex.Message}");
                }
            }
        }

        static string[] ReadAllLines(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllLines(path);
            }
            else
            {
                Console.WriteLine($"文件【{path}】不存在");
            }
            return new string[0];
        }
    }
}
