using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wx = WxPayLib;
using GZCDCPay.Data;
using GZCDCPay.Models;
using GZCDCPay.Services;
using Microsoft.Extensions.Logging;
using GZCDCPay.Filters;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GZCDCPay.Controllers
{
    [Route("[controller]")]
    public class WxPayController : Controller
    {
        private readonly wx.IWxPayPaymentService paymentService;
        private readonly PaymentContext context;
        private readonly SignatureService signatureService;
        private readonly INotifyService notifyService;
        private readonly ILogger<WxPayController> logger;
        public WxPayController(wx.IWxPayPaymentService service, PaymentContext context, ILogger<WxPayController> logger, SignatureService signatureService, INotifyService notifyService)
        {
            this.paymentService = service;
            this.context = context;
            this.logger = logger;
            this.signatureService = signatureService;
            this.notifyService = notifyService;
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> Index(Guid id)
        {
            System.Console.WriteLine($"Id:{id}");

            var order = await context.Orders.SingleOrDefaultAsync(x => x.Id == id);
            if (order == null)
            {
                throw new Exception("No order found.");
            }
            if (order.Status != OrderStatus.Preparing)
            {
                //throw new Exception("Order has been processed.");
                return RedirectToAction("Result", "Payment", new { Id = id });
            }

            try
            {
                var receipt = await paymentService.PlaceNativeOrder(order.Description, order.OrderId, order.Amount, "http://pay.gz-eport.com/WxPay/WxPayCallback");
                order.Channel = "wxpay";
                order.DateIssued = DateTime.Now;
                ViewBag.Url = receipt.QRCodeUrl;
                ViewBag.Id = id;
                await context.SaveChangesAsync();
                return View();
            }
            catch (WxPayLib.WxPayBusinessException ex)
            {
                if (ex.ErrorCode == "ORDERPAID")
                {
                    // Order paid, but server hasn't received callback yet.
                    // So explicitly query order status.
                    var status = await paymentService.QueryOrder(order.ChannelOrderId, order.OrderId);
                    UpdateOrderStatus(status, ref order);
                    await context.SaveChangesAsync();
                    return RedirectToAction("Result", "Payment", new { Id = order.Id });
                }
                else
                {
                    if (order.NotifyUrl != null)
                    {
                        // TODO: 微信报告订单号重复
                        var result = new GZCDCPay.Models.QueryOrderResult()
                        {
                            Result = "FAIL",
                            Message = ex.ErrorCode,
                            AppId = order.AppId,
                            OrderId = order.OrderId,
                            NonceStr = signatureService.GenerateNonceStr(),
                            LastUpdated = DateTime.Now.ToUniversalTime().AddHours(8).ToString("yyyyMMddHHmmss")
                        };
                        result.Signature = signatureService.CalculateSignature(result.AsEnumerable(), order.AppId);
                        // ********** this should enqueue, but not send immediately
                        await notifyService.NotifyAsync(order.NotifyUrl, result);
                        // *********
                    }

                    order.Status = OrderStatus.Errored;
                    context.SaveChanges();
                    return RedirectToAction("Result", "Payment", new { Id = order.Id });
                }

            }


        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> Status(Guid id)
        {
            var order = await context.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
            if (order != null)
            {
                int result = 0;
                if (order.Status == OrderStatus.Paying)
                    result = 1;
                else if (order.Status == OrderStatus.Success)
                    result = 2;
                return Json(new { status = result });
            }
            else
            {
                return Json(new { status = -1 });
            }
        }


        [Route("[action]/{id}")]
        public async Task<IActionResult> Result(Guid id)
        {
            var order = await context.Orders.SingleOrDefaultAsync(x => x.Id == id);
            if (order != null)
            {
                var status = await paymentService.QueryOrder(order.ChannelOrderId, order.OrderId);
                UpdateOrderStatus(status, ref order);
                context.SaveChanges();
            }
            return RedirectToAction("Result", "Payment", id);
        }

        private void UpdateOrderStatus(wx.Data.OrderStatus status, ref Order order)
        {
            switch (status.TradeState)
            {
                case wx.WxPayTradeState.Success:
                    order.Status = OrderStatus.Success;
                    break;
                case wx.WxPayTradeState.Closed:
                    order.Status = OrderStatus.Closed;
                    break;
                case wx.WxPayTradeState.NotPay:
                    order.Status = OrderStatus.NotPaid;
                    break;
                case wx.WxPayTradeState.PayError:
                    order.Status = OrderStatus.Errored;
                    break;
                case wx.WxPayTradeState.Refund:
                    order.Status = OrderStatus.Refunded;
                    break;
                case wx.WxPayTradeState.Revoked:
                    order.Status = OrderStatus.Revoked;
                    break;
                case wx.WxPayTradeState.UserPaying:
                    order.Status = OrderStatus.Paying;
                    break;
                default:
                    order.Status = OrderStatus.Errored;
                    break;
            }

            order.ChannelOrderId = status.TransactinID;
            order.PayerId = status.userOpenID;

        }



        [ActionIPLogger]
        [RouteAttribute("[action]")]
        public async Task<IActionResult> WxPayCallback()
        {
            string responseBody;
            System.IO.StreamReader sr = new System.IO.StreamReader(HttpContext.Request.Body);
            var status = paymentService.ProcessCallback(await sr.ReadToEndAsync(), out responseBody);
            logger.LogDebug($"Callback from server: {status.ToJson()}");

            var order = await context.Orders.SingleOrDefaultAsync(x => x.OrderId == status.OrderID);
            if (order != null)
            {
                UpdateOrderStatus(status, ref order);
                await context.SaveChangesAsync();
                if (!String.IsNullOrEmpty(order.NotifyUrl))
                {
                    QueryOrderResult result = new QueryOrderResult()
                    {
                        Result = "SUCCESS",
                        Message = "OK",
                        AppId = order.AppId,
                        OrderId = order.OrderId,
                        Status = Enum.GetName(typeof(OrderStatus), order.Status),
                        NonceStr = signatureService.GenerateNonceStr(),
                        LastUpdated = status.TransactionTime.HasValue ? status.TransactionTime.Value.ToString("yyyyMMddHHmmss") : DateTime.Now.ToUniversalTime().AddHours(8).ToString("yyyyMMddHHmmss")
                    };
                    result.Signature = this.signatureService.CalculateSignature(result.AsEnumerable(), order.AppId);
                    // ******* this should enqueue, instead of sending immediately
                    await this.notifyService.NotifyAsync(order.NotifyUrl, result);
                    // ***************
                }

            }
            else
            {
                logger.LogDebug($"Order not found.");
            }
            return Content(responseBody);
        }

        [HttpGet("[action]/{currency}")]
        public async Task<Decimal> GetExchangeRate(string currency)
        {
            System.Console.WriteLine("currency: {0}", currency);
            if (currency == "CNY")
            {
                return 1;
            }
            var rate = await paymentService.GetExchangeRate(currency);
            return rate.Rate;
        }


        /*
        [Route("[action]")]
        public IActionResult GetOrders()
        {
            return Json(context.Orders.ToList());
        }

        [Route("[action]")]
        public async Task<IActionResult> UpdateOrder(Guid id, int status)
        {
            var order = await context.Orders.SingleAsync(x => x.Id == id);
            order.Status = (OrderStatus)status;
            await context.SaveChangesAsync();
            return Content("ok");
        }
        [RouteAttribute("[action]")]
        public async Task<IActionResult> QueryOrder(string transactionId, string orderId)
        {
            var status = await paymentService.QueryOrder(transactionId, orderId);
            return Json(status);
        }
        */

    }
}
