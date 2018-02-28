using System;

namespace WxPayLib.Data
{
    public class ExchangeRate
    {
        public DateTime Date{get;set;}
        public Decimal Rate{get;set;}
        public string Currency {get;set;}
    }
}