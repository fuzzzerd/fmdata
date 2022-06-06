using System;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FMData.Rest
{
    /// <summary>
    /// Default authentication provider
    /// </summary>
    public class DefaultAuthTokenProvider : IAuthTokenProvider
    {
        /// <summary>
        /// Username for connections.
        /// </summary>
        protected readonly string Username;
        /// <summary>
        /// Password for connections.
        /// </summary>
        protected readonly string Password;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="username">Username to use when making the connection.</param>
        /// <param name="password">Password to use when making the connection.</param>
        public DefaultAuthTokenProvider(string username, string password)
        {
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Get base64 encoded AuthenticationHeaderValue
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<AuthenticationHeaderValue> GetAuthenticationHeaderValue()
        {
            if (string.IsNullOrEmpty(Username)) throw new ArgumentException("Username is a required parameter.");
            if (string.IsNullOrEmpty(Password)) throw new ArgumentException("Password is a required parameter.");
            var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{Username}:{Password}")));
            return Task.FromResult(header);
        }
    }
}
