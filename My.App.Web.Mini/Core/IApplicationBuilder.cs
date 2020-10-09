using System;
using System.Collections.Generic;
using System.Text;

namespace My.App.Web.Mini.Core
{
    public interface IApplicationBuilder
    {
        IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware);

        RequestDelegate Build();
    }
}
