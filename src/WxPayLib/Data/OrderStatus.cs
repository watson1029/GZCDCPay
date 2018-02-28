using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WxPayLib.Data
{

    /// <summary>
    /// 支付结果
    /// </summary>
    public class OrderStatus
    {
        /// <summary>
        /// 用户OpenID
        /// </summary>
        public string userOpenID { get; internal set; }
        /// <summary>
        /// 用户是否关注公众号
        /// </summary>
        public bool IsUserSubscribed { get;  internal set; }
        /// <summary>
        /// 支付接口类型
        /// </summary>
        public WxPayApiType ApiType { get;  internal set; }
        /// <summary>
        /// 交易状态
        /// </summary>
        public WxPayTradeState TradeState { get; internal set; }
        /// <summary>
        /// 交易状态描述, 如支付失败，请重新下单支付
        /// </summary>
        public string TradeStateDescription { get; internal set; }
        /// <summary>
        /// 付款银行
        /// </summary>
        public string Bank { get;  internal set; }
        /// <summary>
        /// 订单金额
        /// </summary>
        public int TotalAmount { get;  internal set; }
        /// <summary>
        /// 应结订单金额。 =订单金额-非充值代金券金额
        /// </summary>
        public int SettlementTotal { get;  internal set; }
        /// <summary>
        /// 货币种类
        /// </summary>
        public string Currency { get;  internal set; }
        /// <summary>
        /// 现金支付金额
        /// </summary>
        public int Cash { get;  internal set; }
        /// <summary>
        /// 现金支付货币类型
        /// </summary>
        public string CashCurrency { get;  internal set; }
        /// <summary>
        /// 代金券金额。 =订单金额-现金支付金额
        /// </summary>
        public int CouponDiscount { get;  internal set;}
        /// <summary>
        /// 代金券使用数量
        /// </summary>
        public int CouponCount { get;  internal set; }
        /// <summary>
        /// 使用了代金券
        /// </summary>
        public IEnumerable<Coupon> Coupons { get;  internal set; }
        /// <summary>
        /// 微信支付订单号
        /// </summary>
        public string TransactinID { get;  internal set; }
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string OrderID { get;  internal set; }
        /// <summary>
        /// 商户附加数据
        /// </summary>
        public string AttachNote { get;  internal set; }
        /// <summary>
        /// 支付完成时间
        /// </summary>
        public DateTime? TransactionTime { get; internal set; }

        public static OrderStatus FromWxPayData(dynamic data)
        {
            if(data.result_code != "SUCCESS")
            {
                throw new WxPayBusinessException(data.err_code, data.err_code_des);
            }
            else
            {
                var result = new OrderStatus()
                {
                    userOpenID = data.openid,
                    IsUserSubscribed = data.is_subscribe == "Y",
                    TradeStateDescription = data.trade_state_desc,
                    Bank = data.bank_type,
                    TotalAmount = Int32.Parse(data.total_fee ?? "0"),
                    Currency = data.fee_type,
                    Cash = Int32.Parse(data.cash_fee ?? "0"),
                    CashCurrency = data.cash_fee_type,
                    CouponCount = Int32.Parse(data.coupon_count ?? "0"),
                    CouponDiscount = Int32.Parse(data.coupon_fee ?? "0"),
                    Coupons = new List<Coupon>(),
                    TransactinID = data.transaction_id,
                    OrderID = data.out_trade_no,
                    AttachNote = data.attach
                };

                if(data.time_end != null) {
                    result.TransactionTime = DateTime.ParseExact(data.time_end, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal).AddHours(-8);    
                }


                result.TradeState = WxPayEnumsExtension.TradeStateFromString(data.trade_state);
                result.ApiType = WxPayEnumsExtension.ApiTypeFromString(data.trade_type);
                for(int i=0;i<result.CouponCount;i++)
                {
                    result.Coupons.Append(new Coupon() {
                        CouponID = data[$"coupon_id_${i}"],
                        BatchID = data[$"coupon_batch_id_${i}"],
                        Type = data[$"coupon_type_${i}"],
                        Value = Int32.Parse(data[$"coupon_fee_${i}"] ?? "0")
                    });
                }
                return result;
            }
        }

        public string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

    }
}
