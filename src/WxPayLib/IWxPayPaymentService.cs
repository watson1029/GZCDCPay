using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WxPayLib.Data;

namespace WxPayLib
{
    /// <summary>
    /// 支付功能
    /// </summary>
    public interface IWxPayPaymentService
    {
        /// <summary>
        /// 支付接口，生成支付订单
        /// </summary>
        /// <param name="appID">公众号ID</param>
        /// <param name="mechantID">商户号，微信支付分配的商户号</param>
        /// <param name="deviceInfo">设备号，终端设备号(门店号或收银设备ID)，PC网页或公众号内支付请传"WEB"</param>
        /// <param name="description">商品简单描述，须严格按照规范传递。
        /// 扫码支付：浏览器打开的网站主页title名 -商品概述，例如“腾讯充值中心-QQ会员充值”
        /// 公众号支付：商家名称-销售商品类目，例如罗辑思维-图书
        /// APP支付：应用市场上的APP名字-商品概述，例如天天爱消除-游戏充值
        /// </param>
        /// <param name="details">可选, 商品详细列表</param>
        /// <param name="attachNote">可选, 附加数据，在查询API和支付通知中原样返回，主要用于商户携带订单的自定义数据</param>
        /// <param name="orderID">商户系统内部的订单号,32个字符内、可包含字母。要求商户订单号保持唯一性（建议根据当前系统时间加随机序列来生成订单号）。重新发起一笔支付要使用原订单号，避免重复支付；已支付过或已调用关单、撤销的订单号不能重新发起支付。</param>
        /// <param name="currency">可选，货币类型,ISO 4217标准的三位字母代码,如CNY，默认为CNY</param>
        /// <param name="totalAmount">订单总金额，货币最小单位，如人民币的分</param>
        /// <param name="clientIP">支付提交的客户的IP地址</param>
        /// <param name="validFrom">可选, 订单生成时间</param>
        /// <param name="expireAt">可选, 订单过期时间</param>
        /// <param name="couponTag">可选，代金券标记</param>
        /// <param name="callbackUrl">支付结果回掉的地址</param>
        /// <param name="apiType">交易的类型，公众号支付，扫码支付，App支付</param>
        /// <param name="productID">可选, 商品ID</param>
        /// <param name="paymentLimit">支付方式限制，例如不允许使用信用卡</param>
        /// <param name="userOpenID">用户openid， trade_type=JSAPI时此参数必传，用户在商户appid下的唯一标识。</param>
        /// <returns>支付接口结果</returns>
        Task<OrderReceipt> PlaceOrder(
            string appID,
            string mechantID,
            string deviceInfo,
            string description,                    
            IEnumerable<GoodsDetails> details,
            string attachNote,     
            string orderID,           
            string currency,             
            int totalAmount,                
            string clientIP,        
            DateTime? validFrom,              
            DateTime? expireAt,             
            string couponTag,               
            string callbackUrl,           
            WxPayApiType apiType,           
            string productID,           
            WxPayPaymentLimit paymentLimit,               
            string userOpenID
            );

        /// <summary>
        /// 扫码支付接口，生成支付订单
        /// </summary>
        /// <param name="description">商品简单描述，须严格按照规范传递。
        /// 扫码支付：浏览器打开的网站主页title名 -商品概述，例如“腾讯充值中心-QQ会员充值”
        /// 公众号支付：商家名称-销售商品类目，例如罗辑思维-图书
        /// APP支付：应用市场上的APP名字-商品概述，例如天天爱消除-游戏充值
        /// </param>
        /// <param name="details">可选, 商品详细列表</param>
        /// <param name="attachNote">可选, 附加数据，在查询API和支付通知中原样返回，主要用于商户携带订单的自定义数据</param>
        /// <param name="orderID">商户系统内部的订单号,32个字符内、可包含字母。要求商户订单号保持唯一性（建议根据当前系统时间加随机序列来生成订单号）。重新发起一笔支付要使用原订单号，避免重复支付；已支付过或已调用关单、撤销的订单号不能重新发起支付。</param>
        /// <param name="currency">可选, 货币类型,ISO 4217标准的三位字母代码,如CNY</param>
        /// <param name="totalAmount">订单总金额，货币最小单位，如人民币的分</param>
        /// <param name="clientIP">支付提交的客户的IP地址</param>
        /// <param name="expireAt">可选, 订单过期时间</param>
        /// <param name="couponTag">可选, 代金券标记</param>
        /// <param name="productID">可选, 商品ID</param>
        /// <param name="paymentLimit">支付方式限制，例如不允许使用信用卡</param>
        /// <param name="callbackUrl">支付结果回掉的地址，输入null则使用配置上的地址</param>
        /// <returns>支付接口结果</returns>
        Task<OrderReceipt> PlaceNativeOrder(
            string description,
            IEnumerable<GoodsDetails> details,
            string attachNote,
            string orderID,
            string currency,
            int totalAmount,
            string clientIP,
            DateTime? expireAt,
            string couponTag,
            string productID,
            WxPayPaymentLimit paymentLimit,
            string callbackUrl
            );

        
        Task<OrderReceipt> PlaceNativeOrder(
            string description,
            string orderID,
            int totalAmount,
            string callbackUrl
        );

        /// <summary>
        /// 公众号支付接口，生成支付订单
        /// </summary>
        /// <param name="description">商品简单描述，须严格按照规范传递。
        /// 扫码支付：浏览器打开的网站主页title名 -商品概述，例如“腾讯充值中心-QQ会员充值”
        /// 公众号支付：商家名称-销售商品类目，例如罗辑思维-图书
        /// APP支付：应用市场上的APP名字-商品概述，例如天天爱消除-游戏充值
        /// </param>
        /// <param name="details">可选, 商品详细列表</param>
        /// <param name="attachNote">可选, 附加数据，在查询API和支付通知中原样返回，主要用于商户携带订单的自定义数据</param>
        /// <param name="orderID">商户系统内部的订单号,32个字符内、可包含字母。要求商户订单号保持唯一性（建议根据当前系统时间加随机序列来生成订单号）。重新发起一笔支付要使用原订单号，避免重复支付；已支付过或已调用关单、撤销的订单号不能重新发起支付。</param>
        /// <param name="currency">可选, 货币类型,ISO 4217标准的三位字母代码,如CNY</param>
        /// <param name="totalAmount">订单总金额，货币最小单位，如人民币的分</param>
        /// <param name="clientIP">支付提交的客户的IP地址</param>
        /// <param name="expireAt">可选, 订单过期时间</param>
        /// <param name="couponTag">可选, 代金券标记</param>
        /// <param name="productID">可选, 商品ID</param>
        /// <param name="paymentLimit">支付方式限制，例如不允许使用信用卡</param>
        /// <param name="userOpenID">用户openid</param>
        /// <param name="callbackUrl">支付结果回掉的地址，输入null则使用配置上的地址</param>
        /// <returns>支付接口结果</returns>
        Task<OrderReceipt> PlaceJsApiOrder(
            string description,
            IEnumerable<GoodsDetails> details,
            string attachNote,
            string orderID,
            string currency,
            int totalAmount,
            string clientIP,
            DateTime? expireAt,
            string couponTag,
            string productID,
            WxPayPaymentLimit paymentLimit,
            string userOpenID,
            string callbackUrl
            );
        


        OrderStatus ProcessCallback(string requestBody);
        OrderStatus ProcessCallback(string requestBody, out string responseBody);

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="transactionID">微信订单号，微信的订单号，优先使用</param>
        /// <param name="orderID">商户订单号，商户系统内部的订单号，当TransactionID为null时使用此项</param>
        /// <returns>订单状态</returns>
        Task<OrderStatus> QueryOrder(string transactionID, string orderID);

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="orderID"></param>
        /// <returns></returns>
        Task CancelOrder(string orderID);

        /// <summary>
        /// 退款申请
        /// </summary>
        /// <param name="transactionID">微信订单号</param>
        /// <param name="orderID">商户订单号</param>
        /// <param name="refundID">商户退款号</param>
        /// <param name="totalAmount">总金额</param>
        /// <param name="refundAmount">退款金额</param>
        /// <param name="currency">货币类型</param>
        /// <returns>退款状态</returns>
        Task<RefundStatus> Refund(string transactionID, string orderID, string refundID, int totalAmount, int refundAmount, string currency);

        /// <summary>
        /// 退款状态查询
        /// </summary>
        /// <param name="transactionID">微信订单号</param>
        /// <param name="orderID">商户订单号</param>
        /// <param name="refundID">商户退款单号</param>
        /// <param name="refundTransactionID">微信退款单号</param>
        /// <returns>退款状态</returns>
        Task<RefundStatus> QueryRefund(string transactionID, string orderID, string refundID, string refundTransactionID);
        event EventHandler<WxPayLib.Data.OrderStatus> OrderStatusUpdated;


        /// <summary>
        /// 查询当日汇率
        /// </summary>
        /// <param name="currency">币种</param>
        /// <returns>汇率</returns>
        Task<ExchangeRate> GetExchangeRate(string currency);
        /// <summary>
        /// 查询汇率
        /// </summary>
        /// <param name="currency">币种</param>
        /// <param name="date">要查询的日期</param>
        /// <returns>汇率</returns>
        Task<ExchangeRate> GetExchangeRate(string currency, DateTime date);
    }
}
