using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GZCDCPay.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppClients",
                columns: table => new
                {
                    AppId = table.Column<string>(nullable: false),
                    ApiKey = table.Column<string>(nullable: true),
                    DefaultCallbackUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppClients", x => x.AppId);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    Amount = table.Column<int>(nullable: false),
                    AppId = table.Column<string>(nullable: false),
                    ApplicantName = table.Column<string>(nullable: true),
                    CallbackUrl = table.Column<string>(nullable: true),
                    Channel = table.Column<string>(nullable: true),
                    ChannelOrderId = table.Column<string>(nullable: true),
                    Currency = table.Column<string>(maxLength: 3, nullable: true),
                    DateIssued = table.Column<DateTime>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAddOrUpdate", true),
                    Note = table.Column<string>(nullable: true),
                    NotifyUrl = table.Column<string>(nullable: true),
                    OrderId = table.Column<string>(nullable: false),
                    PayerId = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.UniqueConstraint("AK_Orders_OrderId_AppId", x => new { x.OrderId, x.AppId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppClients");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
