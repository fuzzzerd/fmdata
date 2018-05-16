using System.Collections.Generic;

namespace FMData.Rest.Responses
{
    public class BaseResponse : IResponse
    {
        public BaseResponse() { Messages = new List<ResponseMessage>(); }
        public BaseResponse(string code, string message) { Messages = new List<ResponseMessage>() { new ResponseMessage { Code = code, Message = message } }; }

        public IEnumerable<ResponseMessage> Messages { get; set; }
    }
}