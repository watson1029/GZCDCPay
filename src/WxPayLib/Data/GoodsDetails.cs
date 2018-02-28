using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WxPayLib.Data
{
    /// <summary>
    /// 商品详情
    /// </summary>
    public class GoodsDetails
    {
        /// <summary>
        /// 必填 32 商品的编号
        /// </summary>
        public string GoodsID { get; set; }

        /// <summary>
        /// 可选 32 微信支付定义的统一商品编号
        /// </summary>
        public string WxPayGoodsID { get; set; }

        /// <summary>
        /// 必填 256 商品名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 必填 商品数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 必填 商品单价，单位为分
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// 可选 32 商品类目ID
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 可选 1000 商品描述信息
        /// </summary>
        public string Description { get; set; }

    }
}
