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
    
    /// <summary>
    /// Response result containing the token.
    /// </summary>
    public class AuthResponseResult
    {
        /// <summary>
        /// The token provided as a result of the authentication request.
        /// </summary>
        public string Token { get; set; }
    }
}