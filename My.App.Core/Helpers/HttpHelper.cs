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
        private static (RestClient restClient, RestRequest restRequest) InitRestClient(string url, Dictionary<string, string> dictHeaders, int timeout, IWebProxy proxy, Method method, Dictionary<string, object> postData)
        {
            var restClient = new RestClient();
            if (timeout > 0)
            {
                restClient.ReadWriteTimeout = timeout;
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
            if (postData != null && postData.Count > 0)
            {
                foreach (var key in postData.Keys)
                {
                    restRequest.AddParameter(key, postData[key]);
                }
            }
            return (restClient, restRequest);
        }
        private static IRestResponse GetExecuteRes(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null, Dictionary<string, object> postData = null)
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.GET, postData);
            var response = restClient.Execute(restRequest);
            response.SetResponseEncoding();
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
        public static string Get(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null, Dictionary<string, object> postData = null)
        {
            return GetExecuteRes(url, dictHeaders, timeout, proxy, postData).Content;
        }
        public static T Get<T>(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null, Dictionary<string, object> postData = null) where T : new()
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.GET, postData);
            return restClient.Execute<T>(restRequest).Data;
        }

        public static string Post(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null, Dictionary<string, object> postData = null)
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.POST, postData);
            return restClient.Execute(restRequest).Content;
        }
        public static T Post<T>(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null, Dictionary<string, object> postData = null) where T : new()
        {
            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.POST, postData);
            return restClient.Execute<T>(restRequest).Data;
        }



        private static async Task<IRestResponse> GetExecuteAsyncRes(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null, Dictionary<string, object> postData = null)
        {
            //var log = new HttpLogEnt()
            //{
            //    Id = Guid.NewGuid(),
            //    Url = url,
            //    ProxyIp = ((WebProxy)proxy).Address.ToString(),
            //    StartTime = DateTime.Now
            //};
            //await MongoDBServiceBase.Insert<HttpLogEnt>(log);

            (var restClient, var restRequest) = InitRestClient(url, dictHeaders, timeout, proxy, Method.GET, postData);
            var executeTask = restClient.ExecuteGetTaskAsync(restRequest);
            // 任务超时时间 1 分钟
            if (!executeTask.Wait(60 * 1000))
            {
                //log.ResStatusCode = 444444;
                //log.FinishedTime = DateTime.Now;
                //log.IsFinished = true;
                //MongoDBServiceBase.Replace<HttpLogEnt>(log);
                throw new Exception("Http 请求超时，Task 无响应");
            }
            var response = await executeTask;
            response.SetResponseEncoding();
            //var response = await restClient.ExecuteGetTaskAsync(restRequest);
            //response.SetResponseEncoding();
            //log.ResStatusCode = (int)response.StatusCode;
            //log.FinishedTime = DateTime.Now;
            //log.IsFinished = true;
            //MongoDBServiceBase.Replace<HttpLogEnt>(log);

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
        public static async Task<string> GetAsync(string url, Dictionary<string, string> dictHeaders = null, int timeout = 0, IWebProxy proxy = null, Dictionary<string, object> postData = null)
        {
            var res = await GetExecuteAsyncRes(url, dictHeaders, timeout, proxy, postData);
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
