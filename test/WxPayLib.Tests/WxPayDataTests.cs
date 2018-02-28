using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;
using WxPayLib.Data;

namespace WxPayLib.Tests
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class WxPayDataTests
    {
        private readonly dynamic target;
        public WxPayDataTests()
        {
            target = new WxPayLib.Data.WxPayData("192006250b4c09247ec02edce69f6a2d");
        }

        [Fact]
        public void WxPayData_ShouldAssignPropertyDynamically()
        {
            target.first_property = "first_value";
            Assert.Equal("first_value", target.first_property);
            target["second_property"] = "second_value";
            Assert.Equal("second_value", target["second_property"]);
            Assert.Equal("second_value", target.second_property);
            Assert.Equal("first_value", target["first_property"]);
            target.first_property = null;
            target.second_property = null;

        }

        [Fact]
        public void WxPayData_ShouldSignAndVerifyIt()
        {
            target.appid = "wxd930ea5d5a258f4f";
            target.mch_id = "10000100";
            target.device_info = "1000";
            target.body = "test";
            target.nonce_str = "ibuaiVcKdpRxkhJA";
            target.Sign();
            Assert.Equal("9A0A8659F005D6984697E2CA0A9CF3B7",target.sign);
            var actual = target.VerifySignature();
            Assert.True(actual);
        }

        [Fact]
        public void WxPayData_ShouldUpdateFromXml()
        {
            string example = @"
                <xml>
                   <appid>wx2421b1c4370ec43b</appid>
                   <attach>支付测试</attach>
                   <body>JSAPI支付测试</body>
                   <mch_id>10000100</mch_id>
                   <detail><![CDATA[{ ""goods_detail"":[{ ""goods_id"":""iphone6s_16G"", ""wxpay_goods_id"":""1001"", ""goods_name"":""iPhone6s 16G"", ""quantity"":1, ""price"":528800, ""goods_category"":""123456"", ""body"":""苹果手机"" }, { ""goods_id"":""iphone6s_32G"", ""wxpay_goods_id"":""1002"", ""goods_name"":""iPhone6s 32G"", ""quantity"":1, ""price"":608800, ""goods_category"":""123789"", ""body"":""苹果手机"" } ] }]]></detail>
                   <nonce_str>1add1a30ac87aa2db72f57a2375d8fec </nonce_str>
                   <notify_url>http://wxpay.weixin.qq.com/pub_v2/pay/notify.v2.php</notify_url>
                   <openid>oUpF8uMuAJO_M2pxb1Q9zNjWeS6o</openid>
                   <out_trade_no>1415659990</out_trade_no>
                   <spbill_create_ip>14.23.150.211</spbill_create_ip>
                   <total_fee>1</total_fee>
                   <trade_type>JSAPI</trade_type>
                   <sign>0CB01533B8C1EF103065174F50BCA001</sign>
                </xml>";
            target.UpdateFromXml(example);
            Assert.Equal("wx2421b1c4370ec43b", target.appid);
            Assert.Equal("10000100", target.mch_id);
            Assert.Equal(@"{ ""goods_detail"":[{ ""goods_id"":""iphone6s_16G"", ""wxpay_goods_id"":""1001"", ""goods_name"":""iPhone6s 16G"", ""quantity"":1, ""price"":528800, ""goods_category"":""123456"", ""body"":""苹果手机"" }, { ""goods_id"":""iphone6s_32G"", ""wxpay_goods_id"":""1002"", ""goods_name"":""iPhone6s 32G"", ""quantity"":1, ""price"":608800, ""goods_category"":""123789"", ""body"":""苹果手机"" } ] }", target.detail);
            
        }

        [Fact]
        public void WxPayData_ShouldSerializeToXml()
        {
            target.appid = "123456";
            string actual = target.ToXml();
            XElement elem = XElement.Parse(actual);

            Assert.Equal("xml", elem.Name);
            Assert.Equal("123456", elem.Element("appid").Value);
        }
    }
}
