﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace My.App.Web.Mini.Core
{
    public interface IServer
    {
        Task StartAsync(RequestDelegate handler);
    }
}
