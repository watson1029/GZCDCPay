using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;


namespace NotificationService
{
    public class Program
    {
        
        public static void Main(string[] args)
        {
            NotificationService app = new NotificationService();
            // RUN

            Console.WriteLine("Hello World!");
        }

        

        public static void DoWork()
        {
            
        }
    }
}
