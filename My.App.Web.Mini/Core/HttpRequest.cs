using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace My.App.Web.Mini.Core
{
    public class HttpRequest
    {
        private readonly IHttpRequestFeature _feature;

        public Uri Uri { get; set; }

        public NameValueCollection Headers { get; set; }

        public Stream Body { get; set; }

        public HttpRequest(IFeatureCollection features) => _feature = features.Get<IHttpRequestFeature>();
    }
}
