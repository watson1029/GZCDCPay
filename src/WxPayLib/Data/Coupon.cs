using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WxPayLib.Data
{
    /// <summary>
    /// 代金券
    /// </summary>
    public class Coupon
    {
        /// <summary>
        /// 代金券ID
        /// </summary>
        public string CouponID { get; set; }
        /// <summary>
        /// 代金券批次ID
        /// </summary>
        public string BatchID { get; set; }
        /// <summary>
        /// 代金券类型
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 代金券支付金额
        /// </summary>
        public int Value { get; set; }
    }
}
