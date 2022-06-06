using System;
using System.Net.Http.Headers;
using System.Text;
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
        /// <summary>
        /// Username for connections.
        /// </summary>
        protected readonly ConnectionInfo Conn;

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="conn">ConnectionInfo</param>
        public FileMakerCloudAuthTokenProvider(ConnectionInfo conn)
        {
            Conn = conn;
        }

        /// <summary>
        /// Get base64 encoded AuthenticationHeaderValue
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderValue()
        {
            //we need an AWS cognito Id token for authentication
            string token = await GetToken().ConfigureAwait(false);
            return new AuthenticationHeaderValue("Authorization", "FMID " + token);
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
