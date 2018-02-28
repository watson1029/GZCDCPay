using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WxPayLib.Data
{
    public class RefundStatus
    {
        public string TransactionID { get; set; }
        public string OrderID { get; set; }
        public string RefundID { get; set; }
        public string RefundTransactionID { get; set; }
        public string RefundChannel { get; set; }
        public int RefundAmount { get; set; }
        public int SettlementRefundAmount { get; set; }
        public int TotalAmount { get; set; }
        public int SettlementTotalAmount { get; set; }
        public string Currency { get; set; }
        public int Cash { get; set; }
        public int RefundedCash { get; set; }

        public int RefundedCoupontDiscount { get; set; }
        public int RefundedCouponCount { get; set; }


        public IEnumerable<Coupon> RefundedCoupons { get; set; }
        public string State { get; set; }
        public string RefundBank { get; set; }


        public static RefundStatus FromWxPayData(dynamic data)
        {
            if (data.result_code != "SUCCESS")
            {
                throw new WxPayBusinessException(data.err_code, data.err_code_des);
            }
            else
            {
                var result = new RefundStatus()
                {
                    TransactionID = data.transaction_id,
                    OrderID = data.out_trade_no,
                    RefundID = data.out_refund_no,
                    RefundTransactionID = data.refund_id,
                    RefundChannel = data.refund_channel,
                    RefundAmount = Int64.Parse(data.refund_fee),
                    SettlementRefundAmount = Int64.Parse(data.settlement_refund_fee),
                    TotalAmount = Int64.Parse(data.total_fee),
                    SettlementTotalAmount = Int64.Parse(data.settlement_total_fee),
                    Currency = data.fee_type,
                    Cash = Int64.Parse(data.cash_fee),
                    RefundedCash = Int64.Parse(data.cash_fee),
                    RefundedCoupontDiscount = Int64.Parse(data.coupon_refund_fee ?? 0),
                    RefundedCouponCount = Int64.Parse(data.coupon_refund_count ?? 0)
                };
                result.RefundedCoupons = new List<Coupon>();
                for (int i = 0; i < result.RefundedCouponCount; i++)
                {
                    result.RefundedCoupons.Append(new Coupon()
                    {
                        CouponID = data[$"coupon_refund_id_${i}"],
                        BatchID = data[$"coupon_refund_batch_id_${i}"],
                        Type = data[$"coupon_type_${i}"],
                        Value = Int64.Parse(data[$"coupon_refund_fee_${i}"] ?? 0)
                    });
                }
                return result;
            }
        }
    }
}
