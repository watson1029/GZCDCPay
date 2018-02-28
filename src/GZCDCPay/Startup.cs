using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using WxPayLib;
using Microsoft.EntityFrameworkCore;
using GZCDCPay.Filters;
using GZCDCPay.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GZCDCPay
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);

            }
            builder.AddUserSecrets();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<WxPayOptions>(Configuration.GetSection("WxPay"));

            services.AddWxPayService(Configuration);

            services.AddDbContext<Data.PaymentContext>(options => 
                options.UseMySql(@"Server=localhost;database=gzcdc;uid=root;pwd=peach1029;")
                //options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"))
                );

            services.AddSignatureService(options =>
            {
                options.UseDbContext<Data.PaymentContext, Data.AppClient>(
                    context=>context.AppClients,
                    client => client.AppId,
                    client => client.ApiKey);
            });

            services.AddApiSignatureFilter();

             services.AddDbContext<GZCDCPay.Areas.Admin.Data.ApplicationDbContext>(options =>
                //options.UseSqlite(Configuration.GetConnectionString("AdminDbConnection"))
                options.UseMySql(@"Server=localhost;database=gzcdc_admin;uid=root;pwd=peach1029;")
                );

            services.AddIdentity<GZCDCPay.Areas.Admin.Models.ApplicationUser, IdentityRole>(
                options=> {
                    options.Cookies.ApplicationCookie.LoginPath = "/Admin/Account/Login";
                    options.Cookies.ApplicationCookie.LogoutPath = "/Admin/Account/LogOff";
                }
            )
                .AddEntityFrameworkStores<GZCDCPay.Areas.Admin.Data.ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<Areas.Admin.Services.IEmailSender, Areas.Admin.Services.AuthMessageSender>();
            services.AddTransient<Areas.Admin.Services.ISmsSender, Areas.Admin.Services.AuthMessageSender>();    
            services.AddSingleton<INotifyService,QueueNotifyService>();
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddMvc();
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime lifetime)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseWxPayService();

            app.UseIdentity();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "areaRoute",
                    template: "{area:exists}/{controller=Home}/{action=Index}"
                );
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Payment}/{action=Index}/{id?}");
            });

            lifetime.ApplicationStopping.Register(()=>{
                System.Console.WriteLine("Application Stopping");
            });
            
        }
    }
}
