using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WxPayLib
{
    public class WxPayException : Exception
    {
        public WxPayException() : base() { }
        public WxPayException(string message) : base(message) { }
        public WxPayException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class WxPaySignatureInvalidException : WxPayException
    {
        public WxPaySignatureInvalidException() : base() { }
        public WxPaySignatureInvalidException(string message) : base(message) { }
        public WxPaySignatureInvalidException(string message, Exception inner): base(message, inner) { }
    }

    public class WxPayResponseInvalidException : WxPayException
    {
        public WxPayResponseInvalidException() : base() { }
        public WxPayResponseInvalidException(string message) : base(message) { }
        public WxPayResponseInvalidException(string message, Exception inner) : base(message, inner) { }

    }

    public class WxPayProtocolException: WxPayException
    {
        public string ErrorMessage { get; set; }
        public WxPayProtocolException(string message):base(message)
        {
            this.ErrorMessage = message;
        }
    }

    public class WxPayBusinessException: WxPayException
    {
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public WxPayBusinessException(string code, string message):base(message)
        {
            this.ErrorCode = code;
            this.ErrorMessage = message;
        }
    }
}
