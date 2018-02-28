using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WxPayLib.Data;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace WxPayLib
{
    public class WxPayPaymentService : IWxPayPaymentService
    {
        public readonly WxPayOptions _configuration;

        //private HttpClientHandler _httpClientHandler;
        private HttpClient _httpClient = new HttpClient();
        private HttpClient _secureHttpClient = new HttpClient(new HttpClientHandler()
        {
            SslProtocols = System.Security.Authentication.SslProtocols.Tls
        });

        public event EventHandler<OrderStatus> OrderStatusUpdated;

        //private static TimeZoneInfo _ChinaStandardTime = TimeZoneInfo.GetSystemTimeZones().First(x => x.StandardName == "China Standard Time");

        public WxPayPaymentService(WxPayOptions configuration)
        {
            _configuration = configuration;
            _httpClient.BaseAddress = new Uri("https://api.mch.weixin.qq.com/pay/");
            _secureHttpClient.BaseAddress = new Uri("https://api.mch.weixin.qq.com/secapi/pay/");
        }

        public async Task<OrderReceipt> PlaceJsApiOrder(string description, IEnumerable<GoodsDetails> details, string attachNote, string orderID, string currency, int totalAmount, string clientIP, DateTime? expireAt, string couponTag, string productID, WxPayPaymentLimit paymentLimit, string userOpenID, string callbackUrl)
        {

            return await PlaceOrder(_configuration.AppID, _configuration.MechantID, _configuration.DeviceInfo,
                description, details, attachNote, orderID, currency, totalAmount, clientIP, DateTime.Now, expireAt, couponTag,
                callbackUrl, WxPayApiType.JsApi, productID, paymentLimit, userOpenID);
        }

        public async Task<OrderReceipt> PlaceNativeOrder(string description, IEnumerable<GoodsDetails> details, string attachNote, string orderID, string currency, int totalAmount, string clientIP, DateTime? expireAt, string couponTag, string productID, WxPayPaymentLimit paymentLimit, string callbackUrl)
        {
            return await PlaceOrder(_configuration.AppID, _configuration.MechantID, _configuration.DeviceInfo,
                description, details, attachNote, orderID, currency, totalAmount, clientIP, DateTime.Now, expireAt, couponTag,
                callbackUrl, WxPayApiType.Native, productID, paymentLimit, null);
        }

        public async Task<OrderReceipt> PlaceNativeOrder(string description, string orderID, int totalAmount, string callbackUrl)
        {
            if(callbackUrl == null) {
                callbackUrl = "http://" + _configuration.Domain + _configuration.MapPath;
            }
            return await PlaceNativeOrder(description, null, null, orderID, null, totalAmount, "127.0.0.1", null, null, null, WxPayPaymentLimit.NoLimit, callbackUrl);
        }

        private static string ConvertToEast8TimeString(DateTime? datetime)
        {
            if (datetime.HasValue)
            {
                //return TimeZoneInfo.ConvertTime(datetime.Value, _ChinaStandardTime).ToString("yyyyMMddHHmmss");
                return datetime.Value.ToUniversalTime().AddHours(8).ToString("yyyyMMddHHmmss");
            }
            else
            {
                return null;
            }
        }


        public async Task<OrderReceipt> PlaceOrder(string appID, string mechantID, string deviceInfo, string description, IEnumerable<GoodsDetails> details, string attachNote, string orderID, string currency, int totalAmount, string clientIP, DateTime? validFrom, DateTime? expireAt, string couponTag, string callbackUrl, WxPayApiType apiType, string productID, WxPayPaymentLimit paymentLimit, string userOpenID)
        {
            dynamic postData = new WxPayData(_configuration.APIKey);

            postData.appid = appID;
            postData.mch_id = mechantID;
            postData.device_info = deviceInfo;
            postData.body = description;
            postData.detail = SerializeGoodsDetails(details);
            postData.attach = attachNote;
            postData.out_trade_no = orderID;
            postData.fee_type = currency;
            postData.total_fee = totalAmount;
            postData.spbill_create_ip = clientIP;
            postData.time_start = ConvertToEast8TimeString(validFrom);
            postData.time_expire = ConvertToEast8TimeString(expireAt);
            postData.goods_tag = couponTag;
            postData.notify_url = callbackUrl;
            postData.trade_type = apiType.ToPostDataString();
            postData.product_id = productID;
            postData.limit_pay = paymentLimit.ToPostDataString();
            postData.openid = userOpenID;

            postData.Sign();

            HttpContent postContent = new ByteArrayContent(postData.ToByteArray());
            var response = await _httpClient.PostAsync("unifiedorder", postContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new WxPayResponseInvalidException();
            }

            var resContent = await response.Content.ReadAsStringAsync();
            var resData = WxPayData.FromXml(resContent, _configuration.APIKey);
            if (resData.IsReturnedValid)
            {
                if (resData.VerifySignature())
                {
                    return OrderReceipt.FromWxPayData(resData);
                }
                else
                {
                    throw new WxPaySignatureInvalidException("PlaceOrder return error");
                }
            }
            else
            {
                throw new WxPayResponseInvalidException(resData.ReturnedError);
            }


        }

        private string SerializeGoodsDetails(IEnumerable<GoodsDetails> details)
        {
            if (details == null)
            {
                return null;
            }
            JArray ary = new JArray();
            foreach (var detail in details)
            {
                ary.Add(new JObject(
                    new JProperty("goods_id", detail.GoodsID),
                    new JProperty("wxpay_goods_id", detail.WxPayGoodsID),
                    new JProperty("goods_name", detail.Name),
                    new JProperty("goods_num", detail.Count),
                    new JProperty("price", detail.Price),
                    new JProperty("goods_category", detail.Category),
                    new JProperty("body", detail.Description)
                    ));
            }
            var result = new JObject(new JProperty("goods_detail", ary));
            return result.ToString();
        }

        public async Task<OrderStatus> QueryOrder(string transactionID, string orderID)
        {
            dynamic postData = new WxPayData(_configuration.APIKey);
            postData.appid = _configuration.AppID;
            postData.mch_id = _configuration.MechantID;
            postData.transaction_id = transactionID;
            postData.out_trade_no = orderID;
            postData.Sign();

            //System.Console.WriteLine($"postData: {postData.ToXml()}");
             
            HttpContent postContent = new ByteArrayContent(postData.ToByteArray());
            var response = await _httpClient.PostAsync("orderquery", postContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new WxPayResponseInvalidException();
            }
            var resContent = await response.Content.ReadAsStringAsync();

            //System.Console.WriteLine($"resContent:{resContent}");

            var resData = WxPayData.FromXml(resContent, _configuration.APIKey);
            if (resData.VerifySignature())
            {
                return OrderStatus.FromWxPayData(resData);
            }
            else
            {
                throw new WxPaySignatureInvalidException("QueryOrder signature invalid");
            }


        }

        public async Task CancelOrder(string orderID)
        {
            dynamic postData = new WxPayData(_configuration.APIKey);
            postData.appid = _configuration.AppID;
            postData.mch_id = _configuration.MechantID;
            postData.out_trade_no = orderID;
            postData.Sign();
            HttpContent postContent = new ByteArrayContent(postData.ToByteArray());
            var response = await _httpClient.PostAsync("closeorder", postContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new WxPayResponseInvalidException();
            }
            var resContent = await response.Content.ReadAsStringAsync();

            dynamic resData = WxPayData.FromXml(resContent, _configuration.APIKey);
            if (resData.VerifySignature())
            {
                if (resData.return_code != "SUCCESS")
                {
                    throw new WxPayBusinessException(resData.return_code, resData.return_msg);
                }
                else if (resData.result_code != "SUCCESS")
                {
                    throw new WxPayBusinessException(resData.err_code, resData.err_code_des);
                }
            }
            else
            {
                throw new WxPaySignatureInvalidException("PlaceOrder return error");
            }


        }

        public async Task<RefundStatus> Refund(string transactionID, string orderID, string refundID, int totalAmount, int refundAmount, string currency)
        {
            throw new NotImplementedException();
            dynamic postData = new WxPayData(_configuration.APIKey);
            postData.appid = _configuration.AppID;
            postData.mch_id = _configuration.MechantID;
            postData.out_trade_no = orderID;
            postData.transaction_id = transactionID;
            postData.out_refund_no = refundID;
            postData.total_fee = totalAmount;
            postData.refund_fee = refundAmount;
            postData.refund_fee_type = currency;
            postData.op_user_id = _configuration.MechantID;
            postData.Sign();
            HttpContent postContent = new ByteArrayContent(postData.ToByteArray());
            var response = await _secureHttpClient.PostAsync("refund", postContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new WxPayResponseInvalidException();
            }
            var resContent = await response.Content.ReadAsStringAsync();

            dynamic resData = WxPayData.FromXml(resContent, _configuration.APIKey);
            if (resData.VerifySignature())
            {
                if (resData.return_code != "SUCCESS")
                {
                    throw new WxPayBusinessException(resData.return_code, resData.return_msg);
                }
                else if (resData.result_code != "SUCCESS")
                {
                    throw new WxPayBusinessException(resData.err_code, resData.err_code_des);
                }
                else
                {
                    return RefundStatus.FromWxPayData(resData);
                }
            }
            else
            {
                throw new WxPaySignatureInvalidException("PlaceOrder return error");
            }
        }

        public async Task<RefundStatus> QueryRefund(string transactionID, string orderID, string refundID, string refundTransactionID)
        {
            throw new NotImplementedException();
            dynamic postData = new WxPayData(_configuration.APIKey);
            postData.appid = _configuration.AppID;
            postData.mch_id = _configuration.MechantID;
            postData.out_trade_no = orderID;
            postData.transaction_id = transactionID;
            postData.out_refund_no = refundID;
            postData.op_user_id = _configuration.MechantID;
            postData.Sign();
            HttpContent postContent = new ByteArrayContent(postData.ToByteArray());
            var response = await _secureHttpClient.PostAsync("refundquery", postContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new WxPayResponseInvalidException();
            }
            var resContent = await response.Content.ReadAsStringAsync();

            dynamic resData = WxPayData.FromXml(resContent, _configuration.APIKey);
            if (resData.VerifySignature())
            {
                if (resData.return_code != "SUCCESS")
                {
                    throw new WxPayBusinessException(resData.return_code, resData.return_msg);
                }
                else if (resData.result_code != "SUCCESS")
                {
                    throw new WxPayBusinessException(resData.err_code, resData.err_code_des);
                }
                else
                {
                    return RefundStatus.FromWxPayData(resData);
                }
            }
            else
            {
                throw new WxPaySignatureInvalidException("PlaceOrder return error");
            }
        }



        public OrderStatus ProcessCallback(string requestBody)
        {
            WxPayData data = WxPayData.FromXml(requestBody, _configuration.APIKey);
            if (data.VerifySignature())
            {
                var status = OrderStatus.FromWxPayData(data);
                OnOrderStatusUpdated(status);
                return status;
            }
            else
            {
                throw new WxPaySignatureInvalidException();
            }

        }

        public OrderStatus ProcessCallback(string requestBody, out string responseBody)
        {
            try
            {
                var orderStatus = ProcessCallback(requestBody);
                responseBody = @"<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
                return orderStatus;
            }
            catch (WxPaySignatureInvalidException ex)
            {
                responseBody = @"<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[Invalid signature]]></return_msg></xml>";
                throw ex;
            }
        }

        private void OnOrderStatusUpdated(OrderStatus e)
        {
            if (this.OrderStatusUpdated != null)
            {
                this.OrderStatusUpdated(this, e);
            }
        }

        public async Task<ExchangeRate> GetExchangeRate(string currency)
        {
            return await GetExchangeRate(currency, DateTime.Now);
        }
        public async Task<ExchangeRate> GetExchangeRate(string currency, DateTime date)
        {

            dynamic postData = new WxPayData(_configuration.APIKey);

            postData.appid = _configuration.AppID;
            postData.mch_id = _configuration.MechantID;
            postData.fee_type = currency;
            postData.date = ConvertToEast8TimeString(date).Substring(0, 8);
            postData.Sign(null);

            HttpContent postContent = new ByteArrayContent(postData.ToByteArray());
            var response = await _httpClient.PostAsync("queryexchagerate", postContent);
            if (!response.IsSuccessStatusCode)
            {
                throw new WxPayResponseInvalidException();
            }
            var resContent = await response.Content.ReadAsStringAsync();
            dynamic resData = WxPayData.FromXml(resContent, _configuration.APIKey);
            if (resData.IsReturnedValid)
            {
                if (resData.VerifySignature())
                {
                    return new ExchangeRate()
                    {
                        Currency = resData.fee_type,
                        Date = DateTime.ParseExact(resData.rate_time, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
                        Rate = Decimal.Parse(resData.rate) / 100000000
                    };
                }
                else
                {
                    throw new WxPaySignatureInvalidException();
                }
            }
            else
            {
                throw new WxPayResponseInvalidException(resData.ReturnedError);
            }
        }
    }
}
