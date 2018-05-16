using System;
using System.Collections.Generic;
using System.Text;

namespace FMData.Xml.Responses
{
    public class BaseResponse : IResponse
    {
        public BaseResponse() { Messages = new List<ResponseMessage>(); }
        public BaseResponse(string code, string message) { Messages = new List<ResponseMessage>() { new ResponseMessage { Code = code, Message = message } }; }

        public virtual object Response { get; set; }
        public IEnumerable<ResponseMessage> Messages { get; set; }
    }
}