using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WxPayLib
{
    public class WxPayServiceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWxPayPaymentService _paymentService;
        private readonly WxPayOptions _options;
        private string _mapPath;

        public WxPayServiceMiddleware(RequestDelegate next, IWxPayPaymentService paymentService, IOptions<WxPayOptions> options)
        {
            this._next = next;
            this._paymentService = paymentService;
            this._options = options.Value;
            _mapPath = _options.MapPath;
        }

 

        public async Task Invoke(HttpContext context)
        {
            if (!String.IsNullOrEmpty(_mapPath) 
                && context.Request.Path.StartsWithSegments(_mapPath)
                && context.Request.Method == "POST")
            {
                try
                {
                    System.IO.StreamReader sr = new System.IO.StreamReader(context.Request.Body);
                    var orderStatus = _paymentService.ProcessCallback(await sr.ReadToEndAsync());
                    await context.Response.WriteAsync(@"<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>");
                    //await _next.Invoke(context);
                }
                catch (WxPayException ex)
                {
                    await context.Response.WriteAsync($"<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[{ex.ToString()}]]></return_msg></xml>");
                }
            }
            else
            {
                await _next.Invoke(context);
            }
            
        }
    }
}
