namespace FMData.Rest.Responses
{
    /// <summary>
    /// Edit response instance
    /// </summary>
    public class EditResponse : BaseResponse, IEditResponse
    {
        /// <summary>
        /// The response object from the create request.
        /// </summary>
        public ActionResponse Response { get; set; }
    }
}
