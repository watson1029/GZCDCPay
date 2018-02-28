using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GZCDCPay.Models;
using GZCDCPay.Data;
using System.Threading.Tasks;


namespace GZCDCPay.Services
{
    public interface INotifyService
    {
        Task<bool> NotifyAsync(string requestUri, QueryOrderResult result);

    }
    public class NotifyResult
    {
        public string Result { get; set; }
        public string Message { get; set; }
    }
}