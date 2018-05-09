namespace FMData.Rest.Responses
{
    public class AuthResponse : BaseResponse
    {
        public string Token { get; set; }
        public string Layout { get; set; }
    }
}