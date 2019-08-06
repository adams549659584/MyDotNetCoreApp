using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace My.App.Core
{
    public class NotifyHelper
    {

        /// <summary>
        /// 通知到微信
        /// </summary>
        /// <param name="title">消息标题，最长为256，必填</param>
        /// <param name="body">消息内容，最长64Kb，可空，支持MarkDown。可用"     \r\n"换行等等</param>
        /// <returns></returns>
        public static bool Weixin(string title, string body = "")
        {
            #if DEBUG
                return false;
            #endif
            string url = $"https://sc.ftqq.com/SCU33276T4801adab529b3595e3dc25d37cbe38a35bb5f40021bbd.send?text={title}&desp={body}";
            //{"errno":0,"errmsg":"success","dataset":"done"}
            //{"errno":1024,"errmsg":"\u4e0d\u8981\u91cd\u590d\u53d1\u9001\u540c\u6837\u7684\u5185\u5bb9"}
            var fangTangResult = HttpHelper.Get<FangTangResultView>(url);;
            if (fangTangResult.ErrNo != 0)
            {
                throw new Exception(fangTangResult.ErrMsg);
            }
            return true;
        }
    }

    class FangTangResultView
    {
        [JsonProperty("errno")]
        public int ErrNo { get; set; }

        [JsonProperty("errmsg")]
        public string ErrMsg { get; set; }

        [JsonProperty("dataset")]
        public string DataSet { get; set; }
    }
}
