using System.Collections.Generic;

namespace FMData.Rest.Responses
{
    /// <summary>
    /// Base Response Class For Other Response Types to Inherit.
    /// </summary>
    public class BaseResponse : IResponse
    {
        /// <summary>
        /// Constructor that news up the list of messages.
        /// </summary>
        public BaseResponse() { Messages = new List<ResponseMessage>(); }
        /// <summary>
        /// Constructor helper for quickly throwing a code/message into the list for testing.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="message"></param>
        public BaseResponse(string code, string message) { Messages = new List<ResponseMessage>() { new ResponseMessage { Code = code, Message = message } }; }

        /// <summary>
        /// The messages that are part of this response.
        /// </summary>
        public IEnumerable<ResponseMessage> Messages { get; set; }
    }
}