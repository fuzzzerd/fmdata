namespace FMData.Rest.Responses
{
    /// <summary>
    /// Authentication Response
    /// </summary>
    public class AuthResponse : BaseResponse
    {
        /// <summary>
        /// Wrapper for the Response object from FileMaker API.
        /// </summary>
        public AuthResponseResult Response { get; set; }
    }
}