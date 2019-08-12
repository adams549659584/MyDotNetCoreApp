using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Net;
using RestSharp;
using System.Threading.Tasks;

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
                    restClient.AddDefaultHeader(headerKey, dictHeaders[headerKey]);
                    if (headerKey.ToLower().Equals("user-agent"))
                    {
                        restClient.UserAgent = dictHeaders[headerKey];
                    }
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
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"{(int)response.StatusCode}:{response.StatusDescription}");
            }
            return response;
        }
        public static string Get(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            return GetExecuteRes(url, dictHeaders, timeout, proxy).Content;
        }
        public static T Get<T>(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null) where T : new()
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.GET);
            return restClient.Execute<T>(restRequest).Data;
        }

        public static string Post(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.POST);
            return restClient.Execute(restRequest).Content;
        }
        public static T Post<T>(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null) where T : new()
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.POST);
            return restClient.Execute<T>(restRequest).Data;
        }



        private static async Task<IRestResponse> GetExecuteAsyncRes(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            var log = new HttpLogEnt()
            {
                Id = Guid.NewGuid(),
                Url = url,
                ProxyIp = ((WebProxy)proxy).Address.ToString(),
                StartTime = DateTime.Now
            };

            await MongoDBServiceBase.Insert<HttpLogEnt>(log);
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.GET);
            var response = await restClient.ExecuteGetTaskAsync(restRequest);

            log.ResStatusCode = (int)response.StatusCode;
            log.FinishedTime = DateTime.Now;
            log.IsFinished = true;
            MongoDBServiceBase.Replace<HttpLogEnt>(log);

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception($"{(int)response.StatusCode}:{response.StatusDescription}");
            }
            return response;
        }

        private static MongoDBServiceBase MongoDBServiceBase = new MongoDBServiceBase("MyJob");
        public static async Task<string> GetAsync(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null)
        {
            var res = await GetExecuteAsyncRes(url, dictHeaders, timeout, proxy);
            return res.Content;
        }
    }

    public class HttpLogEnt : IMongoEnt
    {
        public Guid Id { get; set; }

        public string Url { get; set; }

        public string ProxyIp { get; set; }

        public DateTime StartTime { get; set; }

        public int ResStatusCode { get; set; }

        public bool IsFinished { get; set; }

        public DateTime FinishedTime { get; set; }
    }
}
