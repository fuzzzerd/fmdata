using System.Collections.Generic;

namespace FMData.Xml.Responses
{
    /// <summary>
    /// Base Response Class
    /// </summary>
    public class BaseResponse : IResponse
    {
        /// <summary>
        /// Empty Constructor
        /// </summary>
        public BaseResponse() { Messages = new List<ResponseMessage>(); }
        /// <summary>
        /// Useful to new up a quick instance with a response code and message.
        /// </summary>
        public BaseResponse(string code, string message) { Messages = new List<ResponseMessage>() { new ResponseMessage { Code = code, Message = message } }; }

        /// <summary>
        /// The response object, should be overridden in child classes
        /// </summary>
        public virtual object Response { get; set; }
        /// <summary>
        /// The messages from this response.
        /// </summary>
        public IEnumerable<ResponseMessage> Messages { get; set; }
    }
}