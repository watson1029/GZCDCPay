using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GZCDCPay.Services;

namespace GZCDCPay.Filters
{

    public class ApiSignatureFilterAttribute : TypeFilterAttribute
    {
        public ApiSignatureFilterAttribute() : base(typeof(ApiSignatureFilterImpl))
        {
        }
    }

    public class ApiSignatureFilterImpl : IAsyncActionFilter
    {
        private readonly SignatureService signatureService;
        
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
                    context.Result = new JsonResult(new ApiSignatureFilterResult()
                    {
                        Result = "FAIL",
                        Message = "SignatureInvalid"
                    });
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

        public ApiSignatureFilterImpl(GZCDCPay.Services.SignatureService signatureService)
        {
            this.signatureService = signatureService;
        }

        private bool VerifySignature(IDictionary<string, string> dictionary)
        {
            var appId = dictionary["AppId"];
            var signature = signatureService.CalculateSignature(dictionary.Where(x=>x.Key != "Signature"), appId);
            System.Console.WriteLine($"Signature should be: {signature}");
            return signature == dictionary["Signature"];
        }
    }

    internal class ApiSignatureFilterResult
    {
        public string Result { get; set; }
        public string Message { get; set; }
    }

    public static class ApiSignatureFilterExtensions
    {
        public static IServiceCollection AddApiSignatureFilter(this IServiceCollection services)
        {
            return services.AddScoped<ApiSignatureFilterImpl>();
        }
    }

    public class ApiSignatureFilterOptionsBuilder
    {
        private readonly ApiSignatureFilterImpl impl;
        public ApiSignatureFilterOptionsBuilder(ApiSignatureFilterImpl impl)
        {
            this.impl = impl;
        }
    }
}