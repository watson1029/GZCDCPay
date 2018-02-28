using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using GZCDCPay.Data;
using System.Threading.Tasks;
using System.Net.Http;
using GZCDCPay.Models;
using Newtonsoft.Json;

namespace GZCDCPay.Services
{
    public class SimpleNotifyService : INotifyService
    {
        private HttpClient client;
        public SimpleNotifyService()
        {
            client = new HttpClient();
        }

        public async Task<bool> NotifyAsync(string requestUri, QueryOrderResult result)
        {
            var content = new FormUrlEncodedContent(result.AsEnumerable().Select(x => new KeyValuePair<string,string>(x.Key, x.Value)));
            var response = await client.PostAsync(requestUri, content);
            if(response.IsSuccessStatusCode)
            {
                var resContent = await response.Content.ReadAsStringAsync();
                var res = JsonConvert.DeserializeObject<NotifyResult>(resContent);
                if(res.Result == "SUCCESS")
                {
                    return true;
                }
            }
            return false;
        }
    }

}