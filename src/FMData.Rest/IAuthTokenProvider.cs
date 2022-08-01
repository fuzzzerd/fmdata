using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FMData.Rest
{
    /// <summary>
    /// FileMaker REST Client Auth Provider Interface.
    /// </summary>
    public interface IAuthTokenProvider
    {
        /// <summary>
        /// Connection config values
        /// </summary>
        ConnectionInfo ConnectionInfo { get; }

        /// <summary>
        /// Provide the AuthenticationHeaderValue
        /// </summary>
        /// <returns>AuthenticationHeaderValue</returns>
        Task<AuthenticationHeaderValue> GetAuthenticationHeaderValue();
    }
}
