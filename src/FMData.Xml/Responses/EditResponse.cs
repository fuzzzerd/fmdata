using System.Collections.Generic;

namespace FMData.Xml.Responses
{
    /// <summary>
    /// Create response instance
    /// </summary>
    public class EditResponse : IEditResponse
    {
        /// <summary>
        /// The response object from the create request.
        /// </summary>
        public ActionResponse Response { get; set; }

        /// <summary>
        /// The messages from this response.
        /// </summary>
        public IEnumerable<ResponseMessage> Messages { get; set; }
    }
}
