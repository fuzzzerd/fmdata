namespace FMREST
{
    using System.Threading.Tasks;
    
    interface IFmsClient
    {
        Task<FmsAuthResponse> AuthenticateAsync(string username, string password, string layout);
    }
}