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

    public class ActionSignatureFilterAttribute : TypeFilterAttribute
    {
        public ActionSignatureFilterAttribute() : base(typeof(ActionSignatureFilterImpl))
        {
        }
    }

    public class ActionSignatureFilterImpl : IAsyncActionFilter
    {
        private readonly SignatureService signatureService;
        private readonly ILogger<ActionSignatureFilterImpl> logger;
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ModelState.ContainsKey("AppId") &&
            context.ModelState.ContainsKey("NonceStr") &&
            context.ModelState.ContainsKey("Signature"))
            {
                var dict = new SortedDictionary<string, string>();
                foreach (var m in context.ModelState)
                {
                    dict.Add(m.Key, m.Value.AttemptedValue);
                }
                if (VerifySignature(dict))
                {
                    await next();
                }
                else
                {
                    context.Result = new JsonResult(new ApiSignatureFilterResult()
                    {
                        Result = "FAIL",
                        Message = "SignatureInvalid"
                    });
                }
            }
            else if ((
                context.ActionArguments.ContainsKey("AppId") &&
                context.ActionArguments.ContainsKey("NonceStr") &&
                context.ActionArguments.ContainsKey("Signature")
            ))
            {
                var dict = new SortedDictionary<string, string>();
                foreach (var m in context.ActionArguments)
                {
                    dict.Add(m.Key, m.Value.ToString());
                }
                if (VerifySignature(dict))
                {
                    await next();
                }
                else
                {
                    object callbackUrl;
                    if(context.ActionArguments.TryGetValue("CallbackUrl", out callbackUrl))
                    {

                    }
                    context.Result = new RedirectToActionResult("Error","Home",new {Message = "系统错误：接口签名不正确", RedirectUrl = callbackUrl?.ToString()});
                }

            }
            else
            {
                context.Result = new JsonResult(new ApiSignatureFilterResult()
                {
                    Result = "FAIL",
                    Message = "ArgumentsInvalid"
                });
            }
        }

        public ActionSignatureFilterImpl(GZCDCPay.Services.SignatureService signatureService, ILogger<ActionSignatureFilterImpl> logger)
        {
            this.signatureService = signatureService;
            this.logger = logger;
        }

        private bool VerifySignature(IDictionary<string, string> dictionary)
        {
            var appId = dictionary["AppId"];
            var signature = signatureService.CalculateSignature(dictionary.Where(x=>x.Key != "Signature"), appId);
            System.Console.WriteLine($"Signature should be: {signature}");
            return signature == dictionary["Signature"];
        }
    }

    internal class ActionSignatureFilterResult
    {
        public string Result { get; set; }
        public string Message { get; set; }
    }

    public static class ActionSignatureFilterExtensions
    {
        public static IServiceCollection AddActionSignatureFilter(this IServiceCollection services)
        {
            return services.AddScoped<ActionSignatureFilterImpl>();
        }
    }

    public class ActionSignatureFilterOptionsBuilder
    {
        private readonly ApiSignatureFilterImpl impl;
        public ActionSignatureFilterOptionsBuilder(ApiSignatureFilterImpl impl)
        {
            this.impl = impl;
        }
    }
}