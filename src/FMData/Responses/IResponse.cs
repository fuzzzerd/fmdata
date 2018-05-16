using System;
using System.Collections.Generic;
using System.Text;

namespace FMData
{
    public interface IResponse
    {
        IEnumerable<ResponseMessage> Messages { get; set; }
    }

    public class ResponseMessage
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}