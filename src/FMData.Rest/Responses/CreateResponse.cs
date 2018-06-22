using System.Collections.Generic;

namespace FMData.Rest.Responses
{
    /// <summary>
    /// Create response instance
    /// </summary>
    public class CreateResponse : ICreateResponse
    {
        /// <summary>
        /// The response object from the create request.
        /// </summary>
        public CreateResponseType Response { get; set; }

        /// <summary>
        /// The messages from this response.
        /// </summary>
        public IEnumerable<ResponseMessage> Messages { get; set; }
    }
}