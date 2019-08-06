using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace My.App.Job
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureServices((hostContext, services) =>
               {
                   //注册后台THostedService类型服务
                   //services.AddHostedService<TestJob>();
                   services.AddHostedService<IpPushJob>();
                   services.AddHostedService<GetFreeProxyJob>();
                   //services.AddHostedService<MpAppPreheatJob>();
               });
    }
}
