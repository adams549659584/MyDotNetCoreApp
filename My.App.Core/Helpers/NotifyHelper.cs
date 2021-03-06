﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

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
            string url = $"https://sc.ftqq.com/SCU33276T4801adab529b3595e3dc25d37cbe38a35bb5f40021bbd.send";
            //{"errno":0,"errmsg":"success","dataset":"done"}
            //{"errno":1024,"errmsg":"\u4e0d\u8981\u91cd\u590d\u53d1\u9001\u540c\u6837\u7684\u5185\u5bb9"}

            var postData = new Dictionary<string, object>()
            {
                { "text", title },
                { "desp", body },
            };

            IWebProxy proxy = null;
            //debug
            //proxy = new WebProxy($"http://127.0.0.1:8888");
            var result = HttpHelper.Post(url, null, 0, proxy, postData);
            var fangTangResult = JsonHelper.Deserialize<FangTangResultView>(result);
            if (fangTangResult.ErrNo != 0)
            {
                throw new Exception(fangTangResult.ErrMsg);
            }
            return true;
        }

        /// <summary>
        /// 通知到微信
        /// </summary>
        /// <param name="title">消息标题，最长为256，必填</param>
        /// <param name="body">消息内容，最长64Kb</param>
        /// <returns></returns>
        public static bool Weixin(string title, MarkdownBuilder body)
        {
            if (body == null)
            {
                throw new Exception("markdown 不可为空");
            }
            return Weixin(title, body.ToString());
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
