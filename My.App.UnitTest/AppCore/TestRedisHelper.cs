using Microsoft.VisualStudio.TestTools.UnitTesting;
using My.App.Core;

namespace My.App.UnitTest
{
    [TestClass]
    public class TestRedisHelper
    {
        [TestMethod]
        public void TestMethod1()
        {
            var redisHelper = new RedisHelper();
            var cacheKey = nameof(TestRedisHelper);

            var cacheVal = "测试康拉德";
            var setResult = redisHelper.Set(cacheKey, cacheVal);
            Assert.IsTrue(setResult, "设置缓存失败");

            var cacheResult = redisHelper.Get<string>(cacheKey);
            Assert.IsTrue(cacheResult.Equals(cacheVal), "获取缓存失败");

            var timeExpried = redisHelper.KeyTimeToLive(cacheKey);
            Assert.IsTrue(timeExpried.HasValue, "获取缓存时间失败");

            var delResult = redisHelper.Delete(cacheKey);
            Assert.IsTrue(delResult, "删除u缓存失败");
        }
    }
}
