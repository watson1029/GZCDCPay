using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using GZCDCPay.Data;

namespace GZCDCPay.Migrations
{
    [DbContext(typeof(PaymentContext))]
    [Migration("20160826053220_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("GZCDCPay.Data.AppClient", b =>
                {
                    b.Property<string>("AppId");

                    b.Property<string>("ApiKey");

                    b.Property<string>("DefaultCallbackUrl");

                    b.HasKey("AppId");

                    b.ToTable("AppClients");
                });

            modelBuilder.Entity("GZCDCPay.Data.Order", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Amount");

                    b.Property<string>("AppId")
                        .IsRequired();

                    b.Property<string>("ApplicantName");

                    b.Property<string>("CallbackUrl");

                    b.Property<string>("Channel");

                    b.Property<string>("ChannelOrderId");

                    b.Property<string>("Currency")
                        .HasAnnotation("MaxLength", 3);

                    b.Property<DateTime?>("DateIssued");

                    b.Property<string>("Description");

                    b.Property<DateTime>("LastUpdated")
                        .ValueGeneratedOnAddOrUpdate();

                    b.Property<string>("Note");

                    b.Property<string>("NotifyUrl");

                    b.Property<string>("OrderId")
                        .IsRequired();

                    b.Property<string>("PayerId");

                    b.Property<int>("Status");

                    b.HasKey("Id");

                    b.HasAlternateKey("OrderId", "AppId");

                    b.ToTable("Orders");
                });
        }
    }
}
