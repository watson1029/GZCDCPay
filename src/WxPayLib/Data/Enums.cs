using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WxPayLib
{
    /// <summary>
    /// 交易状态
    /// </summary>
    public enum WxPayTradeState
    {
        /// <summary>
        /// 支付成功
        /// </summary>
        Success,
        /// <summary>
        /// 转入退款
        /// </summary>
        Refund,
        /// <summary>
        /// 未支付
        /// </summary>
        NotPay,
        /// <summary>
        /// 已关闭
        /// </summary>
        Closed,
        /// <summary>
        /// 已撤销（刷卡支付）
        /// </summary>
        Revoked,
        /// <summary>
        /// 用户支付中
        /// </summary>
        UserPaying,
        /// <summary>
        /// 支付失败(其他原因，如银行返回失败)
        /// </summary>
        PayError
    }




    /// <summary>
    /// 交易类型
    /// </summary>
    public enum WxPayApiType
    {
        /// <summary>
        /// 公众号支付
        /// </summary>
        JsApi,
        /// <summary>
        /// 扫码支付
        /// </summary>
        Native,
        /// <summary>
        /// App支付
        /// </summary>
        App,
        /// <summary>
        /// 刷卡支付
        /// </summary>
        MicroPay
    }

    


    /// <summary>
    /// 支付方式限制
    /// </summary>
    public enum WxPayPaymentLimit
    {
        /// <summary>
        /// 没有限制
        /// </summary>
        NoLimit,
        /// <summary>
        /// 不许允使用信用卡
        /// </summary>
        NoCreditCard
    }

    internal static class WxPayEnumsExtension
    {
        internal static string ToPostDataString(this WxPayApiType value)
        {
            return Enum.GetName(typeof(WxPayApiType), value).ToUpper();
        }
        internal static string ToPostDataString(this WxPayPaymentLimit value)
        {
            if (value == WxPayPaymentLimit.NoCreditCard)
            {
                return "no_credit";
            }
            else
            {
                return null;
            }
        }
        internal static WxPayTradeState TradeStateFromString(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                // 支付通知默认缺省 TradeState
                return WxPayTradeState.Success;
            }
            switch(value.ToUpper())
            {
                case "SUCCESS":
                    return WxPayTradeState.Success;
                case "REFUND":
                    return WxPayTradeState.Refund;
                case "NOTPAY":
                    return WxPayTradeState.NotPay;
                case "CLOSED":
                    return WxPayTradeState.Closed;
                case "REVOKED":
                    return WxPayTradeState.Revoked;
                case "USERPAYING":
                    return WxPayTradeState.UserPaying;
                case "PAYERROR":
                    return WxPayTradeState.PayError;
                default:
                    return WxPayTradeState.PayError;
            }
        }

        internal static WxPayApiType ApiTypeFromString(string value)
        {
            if(String.IsNullOrEmpty(value))
            {
                return WxPayApiType.Native;
            }
            switch(value.ToUpper())
            {
                case "JSAPI":
                    return WxPayApiType.JsApi;
                case "NATIVE":
                    return WxPayApiType.Native;
                case "APP":
                    return WxPayApiType.App;
                case "MICROPAY":
                    return WxPayApiType.MicroPay;
                default:
                    return WxPayApiType.Native;
            }
        }
    }
}
