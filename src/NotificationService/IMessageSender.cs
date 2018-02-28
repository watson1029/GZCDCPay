using System;
using System.Threading.Tasks;

namespace NotificationService
{
    public interface IMessageSender
    {
        void Send(Uri destination, string content);
        Task<bool> SendAsync(Uri destination, string content);
    }
}