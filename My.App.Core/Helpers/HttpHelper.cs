using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;

namespace My.App.Core
{
    public class HttpHelper
    {
        public static HttpResponseMessage GetResponse(string url, Dictionary<string,string> dictHeaders = null)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                if (dictHeaders != null && dictHeaders.Count > 0)
                {
                    foreach (var headerKey in dictHeaders.Keys)
                    {
                        httpClient.DefaultRequestHeaders.Add(headerKey, dictHeaders[headerKey]);
                    }
                }
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                return response;
            }
        }

        public static string GetResponseString(string url, Dictionary<string,string> dictHeaders = null)
        {
            var response = GetResponse(url, dictHeaders);
            string resultStr = response.Content.ReadAsStringAsync().Result;
            return resultStr;
        }
    }
}
