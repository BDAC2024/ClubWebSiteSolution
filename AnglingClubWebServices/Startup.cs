using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDAC.Common.Interfaces;
using BDAC.Common.Models;
using BDAC.Repository;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AnglingClubWebServices
{
    public class Startup
    {

        public const string AwsAccessKey = "AWSAccessId";
        public const string AwsSecretKey = "AWSSecret";
        public const string SimpleDbDomainKey = "SimpleDbDomain";
        public const string LogLevelKey = "LogLevel";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }


        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddLogging(builder =>
            {
                builder.AddConsole();

                LogLevel logLevel = LogLevel.Information; // Default

                switch (Configuration[Startup.LogLevelKey].ToLower())
                {
                    case "debug":
                        logLevel = LogLevel.Debug;
                        break;

                    case "information":
                        logLevel = LogLevel.Information;
                        break;

                    case "warning":
                        logLevel = LogLevel.Warning;
                        break;

                    case "error":
                        logLevel = LogLevel.Error;
                        break;

                    default:
                        logLevel = LogLevel.Information;
                        break;
                }
                builder.SetMinimumLevel(logLevel);
                builder.AddFilter("AnglingClubWebServices", logLevel);
                builder.AddFilter("BDAC.Repository", logLevel);
                builder.AddFilter("Microsoft", LogLevel.Warning);
                //builder.AddFilter("System", LogLevel.Error);``
                //builder.AddFilter("Engine", LogLevel.Warning);
            });

            services.Configure<RepositoryOptions>(Configuration.GetSection("Repository"));

            services.AddTransient<IWaterRepository, WaterRepository>();

            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
            });
        }
    }
}
