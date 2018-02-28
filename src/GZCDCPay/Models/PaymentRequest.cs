using System;

namespace GZCDCPay.Models
{
    public class PaymentRequest
    {
        public Guid Id {get;set;}
        public string OrderId { get; set; }
        public string ApplicantName { get; set; }
        public int? Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public string CallbackUrl { get; set; }
        public string NotifyUrl {get;set;}
        public string AppId {get;set;}
        public string NonceStr{get;set;}
        public string Signature{get;set;}
        public string Account{get;set;}
    }
}