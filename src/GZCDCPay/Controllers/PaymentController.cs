using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GZCDCPay.Models;
using GZCDCPay.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GZCDCPay.Services;
using System.Text;
using GZCDCPay.Filters;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GZCDCPay.Controllers
{
    public class PaymentController : Controller
    {
        private readonly PaymentContext context;
        private readonly ILogger<PaymentController> logger;
        private readonly SignatureService signatureService;

        public PaymentController(PaymentContext context, ILogger<PaymentController> logger, SignatureService signatureService)
        {
            this.context = context;
            this.logger = logger;
            this.signatureService = signatureService;
        }

        [ActionIPLoggerAttribute]
        //[ApiSignatureFilter]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> Index(PaymentRequest application)
        {

            application.OrderId = application.OrderId ?? DateTime.Now.ToString("yyyyMMdd") + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 15);
            application.Amount = application.Amount.HasValue ? application.Amount.Value : 8000;
            application.Description = application.Description ?? "电子口岸业务卡工本费";
            application.Currency = application.Currency ?? "CNY";
            application.AppId = application.AppId ?? "testAppId";
            application.CallbackUrl = application.CallbackUrl ?? context.AppClients.AsNoTracking().SingleOrDefault(x => x.AppId == x.AppId)?.DefaultCallbackUrl;

            Order order = null;
            if (ModelState["Id"]?.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Valid)
            {
                order = await context.Orders.SingleOrDefaultAsync(x => x.Id == application.Id);
            }
            else if (application.OrderId != null)
            {
                order = await context.Orders.SingleOrDefaultAsync(x => x.OrderId == application.OrderId);
            }
            if (order == null)
            {
                order = new Order()
                {
                    OrderId = application.OrderId,
                    Amount = application.Amount.Value,
                    Description = application.Description,
                    Currency = application.Currency,
                    Note = application.Note,
                    Status = OrderStatus.Preparing,
                    ApplicantName = application.ApplicantName,
                    CallbackUrl = application.CallbackUrl,
                    AppId = application.AppId,
                    NotifyUrl = application.NotifyUrl
                };
                context.Add(order);
                await context.SaveChangesAsync();
            }
            else if (order.Status == OrderStatus.Preparing)
            {
                application.OrderId = order.OrderId;
                application.Amount = order.Amount;
                application.Description = order.Description;
                application.Currency = order.Currency;
                application.ApplicantName = order.ApplicantName;
            }
            else if (order.Status == OrderStatus.NotPaid)
            {
                // 重新下单
                if (order.Channel == "wxpay")
                {
                    order.Status = OrderStatus.Preparing;
                    await context.SaveChangesAsync();
                    application.OrderId = order.OrderId;
                    application.Amount = order.Amount;
                    application.Description = order.Description;
                    application.Currency = order.Currency;
                    application.ApplicantName = order.ApplicantName;
                }
            }

            else
            {
                return RedirectToAction("Result", new { id = order.Id });

                /*
                var result = new QueryOrderResult()
                {
                    Result = "FAIL",
                    Message = "OrderDuplicated",
                    NonceStr = this.signatureService.GenerateNonceStr()
                };
                if (!String.IsNullOrEmpty(application.CallbackUrl) || !String.IsNullOrEmpty(application.AppId))
                {

                    if (!String.IsNullOrEmpty(application.AppId))
                    {
                        result.AppId = application.AppId;
                        result.Signature = this.signatureService.CalculateSignature(result.AsEnumerable(), application.AppId);
                    }
                    if (!String.IsNullOrEmpty(application.CallbackUrl))
                    {
                        var redirectUrl = application.CallbackUrl +"?"+ result.AsEnumerable().Aggregate(new StringBuilder(),
                                (sb, arg) => sb.Append($"&{arg.Key}={arg.Value}"),
                                sb => sb.ToString().TrimStart('&')
                            );
                        return Redirect(redirectUrl);
                    }
                    else
                    {
                        var client = await this.context.AppClients.AsNoTracking().SingleOrDefaultAsync();
                        if (client != null && !String.IsNullOrEmpty(client.DefaultCallbackUrl))
                        {
                            var redirectUrl = client.DefaultCallbackUrl + "?" + result.AsEnumerable().Aggregate(new StringBuilder(),
                                (sb, arg) => sb.Append($"&{arg.Key}={arg.Value}"),
                                sb => sb.ToString().TrimStart('&')
                            );
                            return Redirect(redirectUrl);
                        }
                        else
                        {
                            return View("Result", new PaymentResult()
                            {
                                IsSuccessful = false,
                                Message = "订单号重复，请重新下单"
                            });
                        }
                    }
                }
                else
                {
                    return View("Result", new PaymentResult()
                    {
                        IsSuccessful = false,
                        Message = "订单号重复，请重新下单"
                    });
                }
                */

            }


            application.Id = order.Id;
            return View(application);
        }

        [Route("[action]/{id}")]
        public IActionResult Result(Guid id, string extraMessage)
        {
            var order = context.Orders.AsNoTracking().SingleOrDefault(x => x.Id == id);
            if (order == null)
            {
                return View(new PaymentResult() { IsSuccessful = false, Message = "没有当前订单信息" });
            }
            // No feedback from mechant yet
            if (order.Status == OrderStatus.Preparing)
            {
                if (order.Channel == null || !order.DateIssued.HasValue)
                {
                    return View(new PaymentResult()
                    {
                        IsSuccessful = false,
                        Message = "订单尚未支付，点击以下链接继续支付",
                        Url = "/?Id=" + order.Id
                    });
                }
                else
                {
                    // order has issued, but no feedback
                    if (order.Channel == "wxpay")
                    {
                        return RedirectToAction("Result", "WxPay", id);
                    }
                    else if (order.Channel == "alipay")
                    {
                        return Content("Not implemented");
                    }
                    else
                    {
                        return Content("Not supported");
                    }
                }
            }
            else if (order.Status == OrderStatus.Success)
            {
                return View(new PaymentResult()
                {
                    IsSuccessful = true,
                    Message = "支付成功，马上转跳商户页面",
                    Url = MakeCallbackArgumentsString(order)
                });
            }
            else if (order.Status == OrderStatus.NotPaid)
            {
                return View(new PaymentResult()
                {
                    IsSuccessful = false,
                    Message = "订单尚未支付，点击以下链接继续支付",
                    Url = "/?Id=" + order.Id
                });
            }
            else
            {
                return View(new PaymentResult()
                {
                    IsSuccessful = false,
                    Message = $"支付失败，请重新下单。错误原因：{extraMessage}",
                    Url = MakeCallbackArgumentsString(order)
                });
            }
        }

        private string MakeCallbackArgumentsString(Order order)
        {
            var arguments = new Dictionary<string, string>
            {
                ["Result"] = "SUCCESS",
                ["Message"] = "OK",
                ["AppId"] = order.AppId,
                ["OrderId"] = order.OrderId,
                ["NonceStr"] = this.signatureService.GenerateNonceStr(),
                ["Status"] = Enum.GetName(typeof(OrderStatus), order.Status),
                ["LastUpdated"] = order.LastUpdated.ToUniversalTime().AddHours(8).ToString("yyyyMMddHHmmss")
            };
            arguments["Signature"] = this.signatureService.CalculateSignature(arguments, order.AppId);
            return order.CallbackUrl + "?" +
            arguments.AsEnumerable().Aggregate(new StringBuilder(),
                (sb, arg) => sb.Append($"&{arg.Key}={arg.Value}"),
                sb => sb.ToString().TrimStart('&')
            );
        }


        /*
        [RouteAttribute("[action]/{status}")]
        public IActionResult TestResult(bool status)
        {

            return View("Result", new PaymentResult()
            {
                IsSuccessful = status == true,
                Message = "Test message",
                Url = "/TestResult/" + !status
            });
        }
        */
    }

}
