using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NotificationService
{
    public class NotificationService
    {
        private IServiceCollection services;
        private ILogger logger;

        private IConfigurationRoot Configuration { get; set; }
        public NotificationService()
        {
            var builder = new ConfigurationBuilder();
            builder.Build();
            services = new ServiceCollection();
            ConfigureServices(services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            services.AddLogging();
            services.AddScoped<IMessageSender,HttpMessageSender>();
            
        }

    }
}