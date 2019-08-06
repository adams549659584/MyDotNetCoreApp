using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Net;

namespace My.App.Core
{
    public class HttpHelper : IDisposable
    {
        private HttpClientHandler _httpClientHandler = null;
        private HttpClient _httpClient = null;

        public HttpHelper()
        {
            _httpClientHandler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip,
                UseProxy = false
            };
            _httpClient = new HttpClient(_httpClientHandler);
        }

        public void Dispose()
        {
            _httpClientHandler?.Dispose();
            _httpClient?.Dispose();
        }

        /// <summary>
        /// get 请求
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="dictHeaders">请求头</param>
        /// <param name="timeout">超时 单位/秒</param>
        /// <param name="proxy">代理</param>
        /// <returns></returns>
        public HttpResponseMessage GetResponse(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            _httpClientHandler.Proxy = proxy;
            if (dictHeaders != null && dictHeaders.Count > 0)
            {
                foreach (var headerKey in dictHeaders.Keys)
                {
                    _httpClient.DefaultRequestHeaders.Add(headerKey, dictHeaders[headerKey]);
                }
            }
            if (timeout > 0)
            {
                _httpClient.Timeout = new TimeSpan(0, 0, timeout);
            }
            HttpResponseMessage response = _httpClient.GetAsync(url).Result;
            return response;

        }

        /// <summary>
        /// get 请求
        /// </summary>
        /// <param name="url">链接</param>
        /// <param name="dictHeaders">请求头</param>
        /// <param name="timeout">超时 单位/秒</param>
        /// <param name="proxy">代理</param>
        /// <returns></returns>
        public string GetResponseString(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            var response = GetResponse(url, dictHeaders, timeout, proxy);
            string resultStr = response.Content.ReadAsStringAsync().Result;
            return resultStr;
        }
    }
}
