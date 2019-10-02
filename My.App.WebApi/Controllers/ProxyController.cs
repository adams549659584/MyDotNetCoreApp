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

    public ProxyController(ILogger<ProxyController> logger)
    {
      _logger = logger;
    }

    [HttpPost("get")]
    public string Get(ProxyModel model)
    {
      if (model == null)
      {
        return string.Empty;
      }
      return HttpHelper.Get(model.Url, model.DictHeaders, model.Timeout, model.Proxy, model.PostData);
    }

    [HttpPost("post")]
    public string Post(ProxyModel model)
    {
      if (model == null)
      {
        return string.Empty;
      }
      return HttpHelper.Post(model.Url, model.DictHeaders, model.Timeout, model.Proxy, model.PostData);
    }
  }
}
