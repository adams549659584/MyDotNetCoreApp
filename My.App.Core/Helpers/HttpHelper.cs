using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Net;
using RestSharp;

namespace My.App.Core
{
    public class HttpHelper
    {
        private static (RestClient restClient, RestRequest restRequest) InitRestClient(string url, Dictionary<string, string> dictHeaders, int timeout, IWebProxy proxy, Method method)
        {
            var restClient = new RestClient();
            if (timeout > 0)
            {
                restClient.Timeout = timeout;
            }
            if (proxy != null)
            {
                restClient.Proxy = proxy;
            }
            var restRequest = new RestRequest(url, method);
            if (dictHeaders != null && dictHeaders.Count > 0)
            {
                foreach (var headerKey in dictHeaders.Keys)
                {
                    restRequest.AddHeader(headerKey, dictHeaders[headerKey]);
                }
            }
            return (restClient, restRequest);
        }
        private static IRestResponse GetExecuteRes(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.GET);
            var response = restClient.Execute(restRequest);
            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            return response;
        }
        public static string Get(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            return GetExecuteRes(url, dictHeaders, timeout, proxy).Content;
        }
        public static T Get<T>(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)  where T : new()
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.GET);
            return restClient.Execute<T>(restRequest).Data;
        }

        public static string Post(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.POST);
            return restClient.Execute(restRequest).Content;
        }
        public static T Post<T>(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)  where T : new()
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.POST);
            return restClient.Execute<T>(restRequest).Data;
        }
    }
}
