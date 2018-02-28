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
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;

namespace GZCDCPay.Services
{
    public class QueueNotifyService : INotifyService
    {
        private HttpClient client;
        private ITargetBlock<QueueEntry> target;
        public QueueNotifyService()
        {
            client = new HttpClient();
            var buffer = new BufferBlock<QueueEntry>();
            var send = new ActionBlock<QueueEntry>(e=>{
                client.PostAsync(e.requestUri, new FormUrlEncodedContent(e.result.AsEnumerable())).Wait();
            });
            buffer.LinkTo(send);
            target = buffer;
            
        }

        public async Task<bool> NotifyAsync(string requestUri, QueryOrderResult result)
        {
            return await target.SendAsync(new QueueEntry(){ requestUri = requestUri, result = result});
        }

        public class QueueEntry
        {
            public string requestUri {get;set;}
            public QueryOrderResult result {get;set;}
        }
    }


}