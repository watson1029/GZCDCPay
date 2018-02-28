using System;

namespace GZCDCPay.Models
{
    public class QueryOrderRequest
    {
        public string AppId { get; set; }
        public string NonceStr { get; set; }
        public string Signature { get; set; }
        public string OrderId { get; set; }
    }
}
