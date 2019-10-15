using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using My.App.Core;

namespace My.App.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProxyController : ControllerBase
    {
        private readonly ILogger<ProxyController> _logger;
        private static MongoDBServiceBase MongoDBServiceBase = new MongoDBServiceBase("MyJob");

        public ProxyController(ILogger<ProxyController> logger)
        {
            _logger = logger;
        }

        private async Task<List<ProxyIpEnt>> GetProxyIps()
        {
            var mongoFindOptions = new MongoFindOptions<ProxyIpEnt>()
            {
                Skip = 0,
                Limit = 1000,
                SortConditions = x => x.LastValidTime,
                IsDescending = true
            };
            var validProxyIpList = await MongoDBServiceBase.GetList<ProxyIpEnt>(x => x.IsDelete == false, mongoFindOptions);
            return validProxyIpList;
        }

        [HttpPost("get")]
        public async Task<ActionResult> Get([FromBody]ProxyModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }
            if (model.Proxy == null)
            {
                var proxyIps = await GetProxyIps();
                for (int i = 0; i < proxyIps.Count; i++)
                {
                    try
                    {
                        model.Proxy = new WebProxy($"http://{proxyIps[i].IP}:{proxyIps[i].Port}");
                        var result = HttpHelper.Get(model.Url, model.DictHeaders, model.Timeout, model.Proxy, model.PostData);
                        return Ok(result);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log(ex);
                        model.Proxy = null;
                    }
                }
                return BadRequest();
            }
            else
            {
                var result = HttpHelper.Get(model.Url, model.DictHeaders, model.Timeout, model.Proxy, model.PostData);
                return Ok(result);
            }
        }

        [HttpPost("post")]
        public async Task<ActionResult> Post([FromBody]ProxyModel model)
        {
            if (model == null)
            {
                return BadRequest();
            }
            if (model.Proxy == null)
            {
                var proxyIps = await GetProxyIps();
                for (int i = 0; i < proxyIps.Count; i++)
                {
                    try
                    {
                        model.Proxy = new WebProxy($"http://{proxyIps[i].IP}:{proxyIps[i].Port}");
                        var result = HttpHelper.Post(model.Url, model.DictHeaders, model.Timeout, model.Proxy, model.PostData);
                        return Ok(result);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log(ex);
                        model.Proxy = null;
                    }
                }
                return BadRequest();
            }
            else
            {
                var result = HttpHelper.Post(model.Url, model.DictHeaders, model.Timeout, model.Proxy, model.PostData);
                return Ok(result);
            }
        }
    }
}
