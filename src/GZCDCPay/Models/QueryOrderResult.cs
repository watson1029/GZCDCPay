using System;
using System.Collections.Generic;
using System.Linq;

namespace GZCDCPay.Models
{
    public class QueryOrderResult
    {
        public string Result {get;set;}
        public string Message {get;set;}
        public string AppId {get;set;}
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string NonceStr { get; set; }
        public string Signature { get; set; }
        public string LastUpdated { get; set; }
    }

    public static class QueryOrderModelExtension
    {
        public static IEnumerable<KeyValuePair<string,string>> AsEnumerable(this QueryOrderResult model)
        {
            return new Dictionary<string,string>
            {
                ["Result"] = model.Result,
                ["Message"] = model.Message,
                ["AppId"] = model.AppId,
                ["OrderId"] = model.OrderId,
                ["Status"] = model.Status,
                ["NonceStr"] = model.NonceStr,
                ["LastUpdated"] = model.LastUpdated
            }.AsEnumerable();
        }


    }
}
