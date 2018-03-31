using System.Threading.Tasks;
using FMREST.Responses;

namespace FMREST
{
    interface IFmsClient
    {
        Task<FmsAuthResponse> AuthenticateAsync(
            string username, 
            string password, 
            string layout);
    }
}