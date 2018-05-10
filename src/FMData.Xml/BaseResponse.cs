using System;
using System.Collections.Generic;
using System.Text;

namespace FMData.Xml
{
    public class BaseResponse : IResponse
    {
        public string ErrorCode { get; set; }
        public string Result { get; set; }
    }
}