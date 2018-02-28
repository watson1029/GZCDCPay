using System;

namespace GZCDCPay.Models
{
    public class PaymentResult
    {
        public bool IsSuccessful{get;set;}
        public string Message {get;set;}
        public string Url {get;set;}
    }
}