using Microsoft.VisualStudio.TestTools.UnitTesting;
using My.App.Core;

namespace My.App.UnitTest
{
    [TestClass]
    public class TestLogHelper
    {
        [TestMethod]
        public void TestLog()
        {
            LogHelper.Log("test");
        }
    }
}
