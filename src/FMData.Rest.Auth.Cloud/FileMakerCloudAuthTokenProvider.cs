using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;

namespace FMData.Rest
{
    /// <summary>
    /// FileMaker Cloud authentication provider (AWS Cognito)
    /// </summary>
    public class FileMakerCloudAuthTokenProvider : IAuthTokenProvider
    {
        private readonly ConnectionInfo _conn;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="conn">Connection config values</param>
        public FileMakerCloudAuthTokenProvider(ConnectionInfo conn)
        {
            _conn = conn;
        }

        /// <summary>
        /// Connection config values
        /// </summary>
        public ConnectionInfo Conn { get => _conn; }

        /// <summary>
        /// Gets the AuthenticationHeaderValue
        /// </summary>
        /// <returns>AuthenticationHeaderValue</returns>
        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderValue()
        {
            string token = await GetToken().ConfigureAwait(false);
            return AuthenticationHeaderValue.Parse("FMID " + token);
        }

        /// <summary>
        /// Provide an AWS Cognito Identiy Token
        /// </summary>
        private async Task<string> GetToken()
        {
            var provider = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), Amazon.RegionEndpoint.USWest2);
            var userpool = new CognitoUserPool(Conn.CognitoUserPoolID, Conn.CognitoClientID, provider);
            var user = new CognitoUser(Conn.Username, Conn.CognitoClientID, userpool, provider);
            var initiateSrpAuthRequest = new InitiateSrpAuthRequest();
            initiateSrpAuthRequest.Password = Conn.Password;

            var response = await user.StartWithSrpAuthAsync(initiateSrpAuthRequest).ConfigureAwait(false);
            return response.AuthenticationResult.IdToken;
        }
    }
}
