namespace FMData.Rest.Responses
{
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
