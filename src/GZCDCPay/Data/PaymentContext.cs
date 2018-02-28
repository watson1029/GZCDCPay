using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GZCDCPay.Data
{
    public class PaymentContext : DbContext
    {
        public PaymentContext(DbContextOptions<PaymentContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
                
            builder.Entity<Order>()
                .HasAlternateKey(o => new {o.OrderId, o.AppId});
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase();
            }
        }

        public DbSet<Order> Orders { get; set; }

        public DbSet<AppClient> AppClients {get;set;}

    }
}