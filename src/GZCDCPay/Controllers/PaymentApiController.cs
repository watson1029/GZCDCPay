using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GZCDCPay.Filters;
using GZCDCPay.Data;
using GZCDCPay.Models;
using GZCDCPay.Services;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GZCDCPay.Controllers
{
    [Route("api/Payment")]
    public class PaymentApiController : Controller
    {
        private readonly PaymentContext context;
        private readonly SignatureService signatureService;

        public PaymentApiController(PaymentContext context, SignatureService signatureService)
        {
            this.context = context;
            this.signatureService = signatureService;
        }

        [ApiSignatureFilter]
        [Route("[action]")]
        public async Task<IActionResult> OrderStatus(QueryOrderRequest model)
        {
            var order = await context.Orders.AsNoTracking()
                .SingleOrDefaultAsync(o => o.OrderId == model.OrderId && o.AppId == model.AppId);
            if (order != null)
            {
                var result = new QueryOrderResult()
                {
                    Result = "SUCCESS",
                    Message = "OK",
                    AppId = order.AppId,
                    OrderId = order.OrderId,
                    NonceStr = this.signatureService.GenerateNonceStr(),
                    Status = Enum.GetName(typeof(OrderStatus), order.Status),
                    LastUpdated = order.LastUpdated.ToUniversalTime().ToString("yyyyMMddHHmmss")
                };
                result.Signature = this.signatureService.CalculateSignature(result.AsEnumerable(), order.AppId);

                return Json(result);
            }
            else
            {
                var result = new QueryOrderResult()
                {
                    Result = "FAIL",
                    Message = "OrderNotFound",
                    NonceStr = Guid.NewGuid().ToString().Replace("-", ""),
                    AppId = model.AppId
                };

                result.Signature = this.signatureService.CalculateSignature(result.AsEnumerable(), model.AppId);
                return Json(result);

            }
        }



    }
}