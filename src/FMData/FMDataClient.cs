using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FMREST.Responses;

namespace FMREST
{
    /// <summary>
    /// FileMaker Data API Client
    /// </summary>
    public class FMDataClient : IFMDataClient, IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _fmsUri;
        private readonly string _fileName;
        private readonly string _userName;
        private readonly string _password;

        private string dataToken;

        public bool IsAuthenticated => !String.IsNullOrEmpty(dataToken);

        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"/> instance to the class.
        /// </summary>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        /// <param name="initialLayout">Layout to use for the initial authentication request.</param>
        public FMDataClient(string fmsUri, string file, string user, string pass, string initialLayout) : this(new HttpClient(), fmsUri, file, user, pass, initialLayout) { }

        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"> instance to the class.
        /// </summary>
        /// <param name="client">An <see ref="HttpClient"/> instance to utilize for the liftime of this Data Client.</param>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        /// <param name="initialLayout">Layout to use for the initial authentication request.</param>
        public FMDataClient(HttpClient client, string fmsUri, string file, string user, string pass, string initialLayout)
        {
            _client = client;

            _fmsUri = fmsUri;
            _fileName = file;
            _userName = user;
            _password = pass;

            var authResponse = RefreshTokenAsync(_userName, _password, initialLayout);
            authResponse.Wait();
            if (authResponse.Result.Result == "OK")
            {
                dataToken = authResponse.Result.Token;
            }
        }

        public string AuthEndpoint => $"{_fmsUri}/fmi/rest/api/auth/{_fileName}";
        public string FindEndpoint(string layout) => $"{_fmsUri}/fmi/rest/api/find/{_fileName}/{layout}";

        public async Task<AuthResponse> RefreshTokenAsync(string username, string password, string layout)
        {
            var str = $"{{ \"user\": \"{username}\", \"password\" : \"{password}\", \"layout\": \"{layout}\" }}";
            var httpContent = new StringContent(str, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(AuthEndpoint, httpContent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<AuthResponse>(responseJson);
                this.dataToken = responseObject.Token;
                return responseObject;
            }

            throw new Exception("Could not authenticate.");
        }

        public async Task<FindResponse> FindAsync(List<Dictionary<string, string>> findParameters)
        {
            var req = new FindRequest();
            req.Query = findParameters;

            var httpContent = new StringContent(req.ToJson(), Encoding.UTF8, "application/json");
            httpContent.Headers.Add("FM-Data-token", this.dataToken);
            var response = await _client.PostAsync(FindEndpoint("users"), httpContent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<FindResponse>(responseJson);
                return responseObject;
            }

            throw new Exception("Find Rquest Error");
        }

        /// <summary>
        /// Dispose resources opened for this instance of the data client.
        /// </summary>
        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
            }
        }
    }
}