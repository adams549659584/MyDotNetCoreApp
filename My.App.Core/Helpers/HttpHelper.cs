using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Net;

namespace My.App.Core
{
    public class HttpHelper
    {
        /// <summary>
        /// get 请求
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="dictHeaders">请求头</param>
        /// <param name="timeout">超时 单位/秒</param>
        /// <param name="proxy">代理</param>
        /// <returns></returns>
        public static HttpResponseMessage GetResponse(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                // UseProxy = false
                Proxy = proxy
            };
            using (HttpClient httpClient = new HttpClient(handler))
            {
                if (dictHeaders != null && dictHeaders.Count > 0)
                {
                    foreach (var headerKey in dictHeaders.Keys)
                    {
                        httpClient.DefaultRequestHeaders.Add(headerKey, dictHeaders[headerKey]);
                    }
                }
                if (timeout > 0)
                {
                    httpClient.Timeout = new TimeSpan(0, 0, timeout);
                }
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                return response;
            }
        }

        /// <summary>
        /// get 请求
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="dictHeaders">请求头</param>
        /// <param name="timeout">超时 单位/秒</param>
        /// <param name="proxy">代理</param>
        /// <returns></returns>
        public static string GetResponseString(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            var response = GetResponse(url, dictHeaders, timeout, proxy);
            string resultStr = response.Content.ReadAsStringAsync().Result;
            return resultStr;
        }
    }
}
