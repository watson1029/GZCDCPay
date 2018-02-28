using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WxPayLib.Data
{
    /// <summary>
    /// 支付接口回执
    /// </summary>
    public class OrderReceipt
    {
        /// <summary>
        /// 预支付交易会话标识，有效期2小时
        /// </summary>
        public string PrepayID { get; internal set; }
        /// <summary>
        /// 扫码支付所需二维码的内容
        /// </summary>
        public string QRCodeUrl { get; internal set; }

        public static OrderReceipt FromWxPayData(dynamic wxPayData)
        {
            // Should not verify signature, for it has been verified before.

                if(wxPayData.result_code != "SUCCESS")
                {
                    throw new WxPayBusinessException(wxPayData.err_code, wxPayData.err_code_des);
                }
                else
                {
                    return new OrderReceipt(wxPayData.prepay_id, wxPayData.code_url);
                }

        }

        /// <summary>
        /// 支付接口回执生成
        /// </summary>
        /// <param name="prepayID">预支付ID</param>
        /// <param name="qrCodeUrl">二维码内容</param>
        internal OrderReceipt(string prepayID, string qrCodeUrl)
        {
            this.PrepayID = prepayID;
            this.QRCodeUrl = qrCodeUrl;
        }
    }
}
