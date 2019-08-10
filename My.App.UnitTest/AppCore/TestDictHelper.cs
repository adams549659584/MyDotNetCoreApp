using Microsoft.VisualStudio.TestTools.UnitTesting;
using My.App.Core;
using System.Collections.Generic;
using System.Text;

namespace My.App.UnitTest
{
    [TestClass]
    public class TestDictHelper : TestBase
    {
        [TestMethod]
        public void TestSet()
        {
            string wxNotifyUrlKey = "My.Notify.Weixin.Url";
            DictHelper.Update(new DictEnt()
            {
                Key = wxNotifyUrlKey,
                Value = "https://sc.ftqq.com/SCU33276T4801adab529b3595e3dc25d37cbe38a35bb5f40021bbd.send?text={0}&desp={1}",
                Desc = "֪ͨ方糖测试url"
            });
            var fmUrl = DictHelper.GetValue(wxNotifyUrlKey);
            string title = "test";
            string body = "testtesttesttesttesttesttesttesttesttest";
            string url = string.Format(fmUrl, title, body);
        }
    }
}
