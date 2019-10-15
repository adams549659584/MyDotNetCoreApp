using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using My.App.Core;

namespace My.App.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            //注册跨域请求服务
            services.AddCors(options =>
            {
                //注册默认策略
                options.AddDefaultPolicy(builder =>
                {
                    var configFullPath = PathHelper.MapFile("Config", "CorsConfig.jsonc");
                    var configJson = File.ReadAllText(configFullPath);
                    var dictCorsConfig = JsonHelper.Deserialize<Dictionary<string, object>>(configJson);
                    var corsOrigins = JsonHelper.Deserialize<string[]>(dictCorsConfig["Origins"].ToString());

                    //builder.AllowAnyOrigin()
                    builder.WithOrigins(corsOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            //全局配置跨域（一定要配置在 app.UseMvc前）
            //1. 默认策略
            app.UseCors();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
