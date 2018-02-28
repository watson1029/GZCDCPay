using System;
using System.ComponentModel.DataAnnotations;

namespace GZCDCPay.Data
{
    public class AppClient
    {
        [Key]
        public string AppId{get;set;}
        public string ApiKey {get;set;}
        public string DefaultCallbackUrl {get;set;}
    }
}