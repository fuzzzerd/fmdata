namespace FMData.Rest.Responses
{
    public class AuthResponse : BaseResponse
    {
        public AuthResponseResult Response { get; set; }
    }

    public class AuthResponseResult
    {
        public string Token { get; set; }
    }
}