namespace FMREST.Responses
{
    public class AuthResponse : BaseDataResponse
    {
        public string Token { get; set; }
        public string Layout { get; set; }
    }
}