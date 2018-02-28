using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WxPayLib
{
    public class WxPayOptions
    {
        public string AppID { get; set; }
        public string AppSecret { get; set; }
        public string MechantID { get; set; }
        public string EncodingAESKey { get; set; }
        public string Token { get; set; }
        public string DeviceInfo { get; set; }
        public string Proxy { get; set; }
        public string MapPath { get; set; }
        public string Domain { get; set; }
        public string APIKey { get; set; }
    }
}