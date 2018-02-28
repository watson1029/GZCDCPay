using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GZCDCPay.Data
{
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // [ConcurrencyCheck]
        public string OrderId { get; set; }

        public string AppId {get;set;}
        public string Channel { get; set; }
        public string ChannelOrderId { get; set; }
        public DateTime? DateIssued { get; set; }
        public int Amount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; }
        public string Description { get; set; }
        public string ApplicantName { get; set; }
        public OrderStatus Status { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime LastUpdated { get; set; }

        public string PayerId {get;set;}
        public string Note { get; set; }
        public string CallbackUrl {get;set;}
        public string NotifyUrl {get;set;}
    }


    public enum OrderStatus
    {
        Preparing,
        Paying,
        Success,
        NotPaid,
        Closed,
        Refunded,
        Revoked,
        Errored
    }

}