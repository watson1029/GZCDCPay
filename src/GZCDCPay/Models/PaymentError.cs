using System;

namespace GZCDCPay.Models
{
    public class PaymentError
    {
        public string ErrorId { get; set; }
        public string Message { get; set; }
        public string RedirectUrl {get;set;}
    }
}