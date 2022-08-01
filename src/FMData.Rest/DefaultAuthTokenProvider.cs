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
        private readonly ConnectionInfo _conn;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="conn">Provide Connection details</param>
        public DefaultAuthTokenProvider(ConnectionInfo conn)
        {
            _conn = conn;
        }

        /// <summary>
        /// Connection config values
        /// </summary>
        public ConnectionInfo ConnectionInfo { get => _conn; }

        /// <summary>
        /// Get base64 encoded AuthenticationHeaderValue
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public Task<AuthenticationHeaderValue> GetAuthenticationHeaderValue()
        {
            if (string.IsNullOrEmpty(_conn.Username)) throw new ArgumentException("Username is a required parameter.");
            if (string.IsNullOrEmpty(_conn.Password)) throw new ArgumentException("Password is a required parameter.");
            var header = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_conn.Username}:{_conn.Password}")));
            return Task.FromResult(header);
        }
    }
}
