using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GZCDCPay.Filters;
using GZCDCPay.Services;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace GZCDCPay.Controllers
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        private readonly SignatureService signatureService;

        public TestController(SignatureService signatureService)
        {
            this.signatureService = signatureService;
        }

        [ApiSignatureFilter]
        public IActionResult Index(TestModel model)
        {
            var result = new Dictionary<string, string>
            {
                ["Result"] = "SUCCESS",
                ["Message"] = "OK",
                ["AppId"] = model.AppId,
                ["NonceStr"] = this.signatureService.GenerateNonceStr(),
                ["Content"] = model.Content
            };
            result["Signature"] = this.signatureService.CalculateSignature(result, model.AppId);

            return Json(result.AsEnumerable());
        }

        public class TestModel
        {
            public string AppId { get; set; }
            public string NonceStr { get; set; }
            public string Signature { get; set; }
            public string Content { get; set; }
        }


        [Route("[action]")]
        [AcceptVerbs("Get", "Post")]
        public IActionResult TestSignature(TestModel model)
        {
            string signature = "";
            try
            {
                signature = signatureService.CalculateSignature(new Dictionary<string, string>
                {
                    ["AppId"] = model.AppId,
                    ["NonceStr"] = model.NonceStr,
                    ["Content"] = model.Content
                }, model.AppId);
            }
            catch (KeyNotFoundException)
            {
                return Content("Failed. AppId not found.");
            }

            if (signature == model.Signature)
            {
                return Content($"Passed. Signature {signature} is correct.");
            }
            else
            {
                return Content($"Failed. Signature should be {signature}.");
            }
        }

        [RouteAttribute("[action]")]
        public IActionResult TestSignatureWithArbitraryArguments(string appId, string signature)
        {
            var actualSignature = signatureService.CalculateSignature(
                Request.Query.Where(x=>x.Key != "Signature" )
                .Select(x=>new KeyValuePair<string,string>(x.Key,x.Value)),appId);
            
            if (actualSignature == signature)
            {
                return Content($"Passed. Signature {signature} is correct.");
            }
            else
            {
                return Content($"Failed. Signature should be {signature}.");
            }
        }
    }
}