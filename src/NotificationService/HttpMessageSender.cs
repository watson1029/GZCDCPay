using System;
using System.Threading.Tasks;

namespace NotificationService
{
    public class HttpMessageSender : IMessageSender
    {
        public void Send(Uri destination, string content)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendAsync(Uri destination, string content)
        {
            throw new NotImplementedException();
        }
    }
}