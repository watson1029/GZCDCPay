using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace WxPayLib.Data
{
    public class WxPayData : DynamicObject
    {

        SortedDictionary<string, object> dictionary = new SortedDictionary<string, object>();

        private string apiKey;
        public WxPayData(string key)
        {
            this.apiKey = key;
        }

        // 可能要对 goods_detail用 CDATA包起来处理，因为微信做的接口的xml看起来不太符合规范
        public string ToXml()
        {
            XElement elem = new XElement("xml");
            foreach (var pair in dictionary)
            {
                elem.Add(new XElement(pair.Key, pair.Value));
            }
            return elem.ToString();
        }

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(this.ToXml());
        }

        public WxPayData UpdateFromXml(string xmlString)
        {
            var root = XElement.Parse(xmlString);
            if (root.Name == "xml")
            {
                dictionary.Clear();
                foreach (var elem in root.Elements())
                {
                    if(!String.IsNullOrEmpty(elem.Value))
                        dictionary[elem.Name.LocalName.ToLower()] = elem.Value;
                }
            }
            return this;
        }

        public static WxPayData FromXml(string xmlString, string apiKey)
        {
            WxPayData result = new WxPayData(apiKey);
            return result.UpdateFromXml(xmlString);
        }
        

        private string CalculateSignature()
        {
            string arguments = dictionary.Where(x => x.Key != "sign")
                .Aggregate(new StringBuilder(),
                    (sb, next) => sb.Append($"&{next.Key}={next.Value.ToString()}"),
                    result => result.Append($"&key={apiKey}").ToString().TrimStart('&'));
            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(arguments));
            var signature = BitConverter.ToString(hash).Replace("-", string.Empty);
            return signature;

        }

        public bool IsReturnedValid
        {
            get { return dictionary["return_code"].ToString() == "SUCCESS";}  
        }
        public string ReturnedError
        {
            get { return dictionary["return_msg"].ToString();}
        }

        public bool VerifySignature()
        {

            if (!dictionary.ContainsKey("sign"))
            {
                throw new  WxPaySignatureInvalidException("WxPayData signature not exists");
            }
            else if (dictionary["sign"] == null || dictionary["sign"].ToString() == "")
            {
                throw new WxPaySignatureInvalidException("WxPayData signature is empty");
            }

            string signature = dictionary["sign"].ToString();
            string actual = CalculateSignature();
            return signature == actual;
        }


        public void Sign()
        {
            if (!dictionary.ContainsKey("nonce_str"))
            {
                dictionary["nonce_str"] = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            }
            dictionary["sign"] = CalculateSignature();
        }

        public void Sign(string nonce_str)
        {
            if(!String.IsNullOrEmpty(nonce_str))
            {
                dictionary["nonce_str"] = nonce_str;
                Sign();
            }
            else
            {
                dictionary.Remove("nonce_str");
                dictionary["sign"] = CalculateSignature();
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            string name = binder.Name.ToLower();
            if(dictionary.ContainsKey(name))
            {
                return dictionary.TryGetValue(name, out result);
            }
            else
            {
                result = null;
                return true;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if(value != null)
            {
                dictionary[binder.Name.ToLower()] = value;
            }
            else
            {
                dictionary.Remove(binder.Name.ToLower());
            }
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            string index = (string)indexes[0];
            return dictionary.TryGetValue(index,out result);
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if(value != null)
            {
                string index = (string)indexes[0];
                dictionary[index.ToLower()] = value;
            }
            return true;
        }

    }
}
