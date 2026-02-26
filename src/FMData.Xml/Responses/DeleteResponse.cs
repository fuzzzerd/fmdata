using System.Collections.Generic;

namespace FMData.Xml.Responses
{
    /// <summary>
    /// Delete response instance
    /// </summary>
    public class DeleteResponse : IDeleteResponse
    {
        /// <summary>
        /// The response object from the delete request.
        /// </summary>
        public ActionResponse Response { get; set; }

        /// <summary>
        /// The messages from this response.
        /// </summary>
        public IEnumerable<ResponseMessage> Messages { get; set; }
    }
}
