using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace My.App.Core
{
    public class HttpHelper
    {
        public static HttpResponseMessage GetResponse(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = httpClient.GetAsync(url).Result;
                return response;
            }
        }

        public static string GetResponseString(string url)
        {
            var response = GetResponse(url);
            string resultStr = response.Content.ReadAsStringAsync().Result;
            return resultStr;
        }
    }
}
