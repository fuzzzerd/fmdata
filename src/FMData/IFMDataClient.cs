using System.Threading.Tasks;
using FMREST.Responses;

namespace FMREST
{
    /// <summary>
    /// FileMaker Data API Client
    /// </summary>
    interface IFMDataClient
    {
        /// <summary>
        /// Refreshes the internally stored authentication token from filemaker server.
        /// </summary>
        /// <param name="username">Username of the account to authenticate.</param>
        /// <param name="password">Password of the account to authenticate.</param>
        /// <param name="layout">The layout to authenticate against.</param>
        /// <returns>An AuthResponse from deserialized from FileMaker Server json response.</returns>
        Task<AuthResponse> RefreshTokenAsync(
            string username,
            string password,
            string layout);

        /// <summary>
        /// Generate the appropriate Authentication endpoint uri for this instance of the data client.
        /// </summary>
        string AuthEndpoint { get; }

        /// <summary>
        /// Generate the appropriate Find endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The layout for the find request.</param>
        string FindEndpoint(string layout);

    }
}