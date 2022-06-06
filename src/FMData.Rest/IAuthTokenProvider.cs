using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FMData.Rest
{
    /// <summary>
    /// FileMaker REST Client Auth Provider.
    /// </summary>
    public interface IAuthTokenProvider
    {

        /// <summary>
        /// Provide the AuthenticationHeaderValue
        /// </summary>
        /// <returns>AuthenticationHeaderValue</returns>
        Task<AuthenticationHeaderValue> GetAuthenticationHeaderValue();
    }
}
