using AnglingClubWebServices.Data;
using AnglingClubWebServices.Helpers;
using AnglingClubWebServices.Interfaces;
using AnglingClubWebServices.Models;
using AnglingClubWebServices.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace AnglingClubWebServices
{
    public class Startup
    {

        private static string _corsPolicy = "AnglingClubWebsiteOrigins";
        public const string LogLevelKey = "LogLevel";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }


        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            var key = Configuration["SyncfusionLicenseKey"];
            if (!string.IsNullOrWhiteSpace(key))
            {
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(key);
            }

            services.AddControllers();

            services.AddLogging(builder =>
            {
                builder.AddConsole();

                LogLevel logLevel = LogLevel.Information; // Default

                var tst = Configuration["TestSecret"];

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
                builder.AddFilter("Microsoft.Hosting", LogLevel.Information);
                builder.AddFilter("Microsoft", LogLevel.Warning);
                builder.AddFilter("System", LogLevel.Error);
                builder.AddFilter("Engine", LogLevel.Warning);
            });

            var configuredOrigins = (Configuration["CORSOrigins"] ?? "")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Example: your Azure Static Web Apps default host prefix and region.
            // Adjust these to YOUR actual SWA defaults.
            // e.g. "wonderful-grass-012345678" and "westeurope"
            var swaDefaultHost = Configuration["SwaDefaultHost"];   // without region/suffix
            var swaRegion = Configuration["SwaRegion"];        // e.g. "westeurope", "uksouth", etc.

            Regex? swaPreviewRegex = null;
            if (!string.IsNullOrWhiteSpace(swaDefaultHost) && !string.IsNullOrWhiteSpace(swaRegion))
            {
                // Matches:
                // https://<DEFAULT>-<anything>.<REGION>.azurestaticapps.net
                // including PRs (-123) and named envs (-dev) etc.
                var pattern = $"^https://{Regex.Escape(swaDefaultHost)}-[a-z0-9-]+\\.{Regex.Escape(swaRegion)}\\.azurestaticapps\\.net$";

                swaPreviewRegex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }

            services.AddCors(options =>
            {
                options.AddPolicy(_corsPolicy, builder =>
                {
                    builder
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(origin =>
                        {
                            // 1) Explicit allowlist from config (prod, localhost, etc.)
                            if (configuredOrigins.Contains(origin))
                                return true;

                            // 2) SWA preview allow (pattern)
                            if (swaPreviewRegex is not null && swaPreviewRegex.IsMatch(origin))
                                return true;

                            return false;
                        })
                        .SetPreflightMaxAge(TimeSpan.FromHours(1));
                });
            });

            // All controller methods use authorization - exclude specific methods using [AllowAnonymous]
            services.AddControllers()
                .AddMvcOptions(x => x.Filters.Add(new AuthorizeAttribute())) //Uncomment this line to add the authorize attribute to all route by default
                ;


            services.AddAutoMapper(typeof(Startup));

            services.Configure<RepositoryOptions>(Configuration);
            services.Configure<AuthOptions>(Configuration);
            services.Configure<EmailOptions>(Configuration);
            services.Configure<StripeOptions>(Configuration);

            services.AddTransient<IWaterRepository, WaterRepository>();
            services.AddTransient<IEventRepository, EventRepository>();
            services.AddTransient<IReferenceDataRepository, ReferenceDataRepository>();
            services.AddTransient<IMatchResultRepository, MatchResultRepository>();
            services.AddTransient<IMatchResultService, MatchResultService>();
            services.AddTransient<IHealthService, HealthService>();
            services.AddTransient<IMemberRepository, MemberRepository>();
            services.AddTransient<INewsRepository, NewsRepository>();
            services.AddTransient<IUserAdminRepository, UserAdminRepository>();
            services.AddTransient<IBackupRepository, BackupRepository>();
            services.AddTransient<IAppSettingRepository, AppSettingRepository>();
            services.AddTransient<IProductMembershipRepository, ProductMembershipRepository>();
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddTransient<ITrophyWinnerRepository, TrophyWinnerRepository>();
            services.AddTransient<IOpenMatchRepository, OpenMatchRepository>();
            services.AddTransient<IOpenMatchRegistrationRepository, OpenMatchRegistrationRepository>();
            services.AddTransient<ITmpFileRepository, TmpFileRepository>();

            services.AddTransient<IPaymentsService, PaymentService>();
            services.AddTransient<ITicketService, TicketService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IUtilityService, UtilityService>();


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

            app.UseCors(_corsPolicy);

            // custom jwt auth middleware
            app.UseMiddleware<JwtMiddleware>();

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
