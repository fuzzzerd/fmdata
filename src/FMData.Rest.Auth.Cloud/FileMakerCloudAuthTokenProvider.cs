using System.Net.Http.Headers;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.Runtime;
using Amazon;

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
        public ConnectionInfo ConnectionInfo { get => _conn; }

        /// <summary>
        /// Gets the AuthenticationHeaderValue
        /// </summary>
        /// <returns>AuthenticationHeaderValue</returns>
        public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderValue()
        {
            var region = RegionEndpoint.GetBySystemName(_conn.RegionEndpoint);
            var identityProviderClient = new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), region);
            string token = await GetToken(identityProviderClient).ConfigureAwait(false);
            return AuthenticationHeaderValue.Parse("FMID " + token);
        }

        /// <summary>
        /// Provide an AWS Cognito Identiy Token
        /// </summary>
        /// <param name="identityProviderClient"><see href="https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/CognitoIdentityProvider/TCognitoIdentityProviderClient.html">AmazonCognitoIdentityProviderClient</see></param>
        /// <returns>returns the IdToken of the AuthenticationResult</returns>
        private async Task<string> GetToken(AmazonCognitoIdentityProviderClient identityProviderClient)
        {
            var userpool = new CognitoUserPool(ConnectionInfo.CognitoUserPoolID, ConnectionInfo.CognitoClientID, identityProviderClient);
            var user = new CognitoUser(ConnectionInfo.Username, ConnectionInfo.CognitoClientID, userpool, identityProviderClient);
            var initiateSrpAuthRequest = new InitiateSrpAuthRequest(); //SRP means Secure Remote Password
            initiateSrpAuthRequest.Password = ConnectionInfo.Password;

            var response = await user.StartWithSrpAuthAsync(initiateSrpAuthRequest).ConfigureAwait(false);
            return response.AuthenticationResult.IdToken;
        }
    }
}
