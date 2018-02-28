using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;

namespace WxPayLib
{
    public static class WxPayLibExtensions
    {
        public static void AddWxPayService(this IServiceCollection services, IConfigurationRoot configuration)
        {
            var section = configuration.GetSection("WxPay");
            WxPayOptions wxPayConfig = new WxPayOptions()
            {
                AppID = section["AppID"],
                AppSecret = section["AppSecret"],
                EncodingAESKey = section["EncodingAESKey"],
                MechantID = section["MechantID"],
                Token = section["Token"],
                DeviceInfo = section["DeviceInfo"],
                APIKey = section["APIKey"],
                MapPath = section["MapPath"],
                Domain = section["Domain"]
            };
            services.AddScoped<IWxPayPaymentService>(provider => new WxPayPaymentService(wxPayConfig));
        }

        public static void UseWxPayService(this IApplicationBuilder app)
        {
            //var section = configuration.GetSection("WxPay");
            //var mapPath = section["CallbackUrl"];
            //if(!String.IsNullOrEmpty(mapPath))
            //{
            //    app.MapWhen((ctx) => ctx.Request.Path.ToString().StartsWith(mapPath),
            //        appBranch => appBranch.UseMiddleware<WxPayServiceMiddleware>()
            //    );
            //}
            app.UseMiddleware<WxPayServiceMiddleware>();
        }
    }
}
