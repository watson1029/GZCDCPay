using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GZCDCPay.Services;
using Microsoft.Extensions.Logging;

namespace GZCDCPay.Filters
{

    public class ActionIPLoggerAttribute : TypeFilterAttribute
    {
        public  ActionIPLoggerAttribute() : base(typeof(ActionIPLoggerImpl))
        {
        }
    }

    public class  ActionIPLoggerImpl : IAsyncActionFilter
    {
        private readonly ILogger<ActionIPLoggerImpl> logger;
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            this.logger.LogDebug($"Accessing from {context.HttpContext.Connection.RemoteIpAddress}");
            await next();
        }

        public  ActionIPLoggerImpl(ILogger<ActionIPLoggerImpl> logger)
        {
            this.logger = logger;
        }

        
    }

    public static class  ActionIPLoggerExtensions
    {
        public static IServiceCollection AddActionIPLogger(this IServiceCollection services)
        {
            return services.AddScoped<ActionIPLoggerImpl>();
        }
    }
}