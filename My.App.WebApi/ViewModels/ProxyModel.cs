using System;
using System.Collections.Generic;
using System.Net;

namespace My.App.WebApi.Controllers
{
  public class ProxyModel
  {
    public string Url { get; set; }

    public Dictionary<string, string> DictHeaders { get; set; }

    public int Timeout { get; set; }

    public IWebProxy Proxy { get; set; }

    public Dictionary<string, object> PostData { get; set; }
  }
}
