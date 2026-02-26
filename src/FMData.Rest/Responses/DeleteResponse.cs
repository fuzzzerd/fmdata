namespace FMData.Rest.Responses
{
    /// <summary>
    /// Delete response instance
    /// </summary>
    public class DeleteResponse : BaseResponse, IDeleteResponse
    {
        /// <summary>
        /// The response object from the delete request.
        /// </summary>
        public ActionResponse Response { get; set; }
    }
}
