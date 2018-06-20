using System;
using System.Collections.Generic;
using System.Text;

namespace FMData
{
    /// <summary>
    /// General Response from FileMaker Server API
    /// </summary>
    public interface IResponse
    {
        /// <summary>
        /// The messages returned by the request.
        /// </summary>
        IEnumerable<ResponseMessage> Messages { get; set; }
    }

    /// <summary>
    /// Response Message Wrapper (required by response format for serialization reasons)
    /// </summary>
    public class ResponseMessage
    {
        /// <summary>
        /// The Message Code (typically an error, zero for OK)
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// The Message's message
        /// </summary>
        public string Message { get; set; }
    }
}