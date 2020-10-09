using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace My.App.Web.Mini.Core
{
    public class HttpResponse
    {
        private readonly IHttpResponseFeature _feature;

        public NameValueCollection Headers { get; set; }

        public Stream Body { get; set; }

        public int StatusCode { get; set; }

        public HttpResponse(IFeatureCollection features) => _feature = features.Get<IHttpResponseFeature>();
    }
}
