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
using FMData.Responses;
using FMData.Requests;
using System.Reflection;
using System.ComponentModel.DataAnnotations.Schema;

namespace FMData
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
        /// <remarks>Pass through constructor with no real body used for injection.</remarks>
        public FMDataClient(string fmsUri, string file, string user, string pass, string initialLayout) 
            : this(new HttpClient(), fmsUri, file, user, pass, initialLayout) { }

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
            // trim out the trailing slash if they included it
            if (_fmsUri.EndsWith("/", StringComparison.CurrentCultureIgnoreCase))
            {
                _fmsUri = fmsUri.Substring(0, fmsUri.Length - 1);
            }
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

        #region API Endpoint Functions
        /// <summary>
        /// Note we assume _fmsUri has no trailing slash as its cut off in the constructor.
        /// </summary>
        private string _baseEndPoint => $"{_fmsUri}/fmi/rest/api";
        /// <summary>
        /// Generate the appropriate Authentication endpoint uri for this instance of the data client.
        /// </summary>
        /// <returns>The FileMaker Data API Endpoint for Authentication Requests.</returns>
        public string AuthEndpoint() => $"{_baseEndPoint}/auth/{_fileName}";
        /// <summary>
        /// Generate the appropriate Find endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <returns>The FileMaker Data API Endpoint for Find requests.</returns>
        public string FindEndpoint(string layout) => $"{_baseEndPoint}/find/{_fileName}/{layout}";
        /// <summary>
        /// Generate the appropriate Create endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <returns>The FileMaker Data API Endpoint for Create requests.</returns>
        public string CreateEndpoint(string layout) => $"{_baseEndPoint}/record/{_fileName}/{layout}";
        /// <summary>
        /// Generate the appropriate Edit/Update endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <param name="recordid">The record ID of the record to edit.</param>
        /// <returns>The FileMaker Data API Endpoint for Update/Edit requests.</returns>
        public string UpdateEndpoint(string layout, object recordid) => $"{_baseEndPoint}/record/{_fileName}/{layout}/{recordid}";
        /// <summary>
        /// Generate the appropriate Delete endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <param name="recordid">The record ID of the record to edit.</param>
        /// <returns>The FileMaker Data API Endpoint for Delete requests.</returns>
        public string DeleteEndpoint(string layout, object recordid) => $"{_baseEndPoint}/record/{_fileName}/{layout}/{recordid}";
        #endregion

        #region FM Data Token Management

        /// <summary>
        /// <see cref="IFMDataClient.RefreshTokenAsync(string, string, string)"/>
        /// </summary>
        public async Task<AuthResponse> RefreshTokenAsync(string username, string password, string layout)
        {
            // parameter checks
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username is a required parameter.");
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password is a required parameter.");
            if (string.IsNullOrEmpty(layout)) throw new ArgumentException("Layout is a required parameter.");

            // build up the request object/content
            var str = $"{{ \"user\": \"{username}\", \"password\" : \"{password}\", \"layout\": \"{layout}\" }}";
            var httpContent = new StringContent(str, Encoding.UTF8, "application/json");
            // run the post action
            var response = await _client.PostAsync(AuthEndpoint(), httpContent);

            // process the response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<AuthResponse>(responseJson);
                this.dataToken = responseObject.Token;
                return responseObject;
            }
            // something bad happened. TODO: improve non-OK response handling
            throw new Exception("Could not authenticate.");
        }

        /// <summary>
        /// <see cref="IFMDataClient.LogoutAsync"/>
        /// </summary>
        public async Task<BaseDataResponse> LogoutAsync()
        {
            // add a default request header of our data token to nuke
            _client.DefaultRequestHeaders.Add("FM-Data-token", this.dataToken);
            var response = await _client.DeleteAsync(AuthEndpoint());

            // process the response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BaseDataResponse>(responseJson);
                return responseObject;
            }

            throw new Exception("Could not logout.");
        }
        #endregion


        public Task<BaseDataResponse> CreateAsync<T>(T input)
        {
            string lay;
            try
            {
                // try to get the 'layout' name out of the 'table' attribute.
                // not the best but tries to utilize a built in component that is fairly standard vs a custom component dirtying up consumers pocos
                lay = typeof(T).GetTypeInfo().GetCustomAttribute<TableAttribute>().Name;
            }
            catch
            {
                throw new ArgumentException($"Could not load Layout name from TableAttribute on {typeof(T).Name}.");
            }
            var req = new CreateRequest<T>() { Data = input, Layout = lay };
            return ExecuteCreateAsync(req);
        }

        public Task<BaseDataResponse> CreateAsync<T>(string layout, T input)
        {
            var req = new CreateRequest<T>() { Data = input, Layout = layout };
            return ExecuteCreateAsync(req);
        }

        public async Task<BaseDataResponse> ExecuteCreateAsync<T>(CreateRequest<T> req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");

            var str = JsonConvert.SerializeObject(req);
            var httpContent = new StringContent(str, Encoding.UTF8, "application/json");
            httpContent.Headers.Add("FM-Data-token", this.dataToken);
            
            // run the post action
            var response = await _client.PostAsync(CreateEndpoint(req.Layout), httpContent);

            // process the response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BaseDataResponse>(responseJson);
                return responseObject;
            }
            // something bad happened. TODO: improve non-OK response handling
            throw new Exception("Could not Create new record.");
        }

        public async Task<BaseDataResponse> ExecuteEditAsync(EditRequest req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");
            if (string.IsNullOrEmpty(req.RecordId)) throw new ArgumentException("RecordId is required on the request.");

            var str = JsonConvert.SerializeObject(req);
            var httpContent = new StringContent(str, Encoding.UTF8, "application/json");
            httpContent.Headers.Add("FM-Data-token", this.dataToken);
            
            // run the post action
            var response = await _client.PutAsync(UpdateEndpoint(req.Layout, req.RecordId), httpContent);

            // process the response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BaseDataResponse>(responseJson);
                return responseObject;
            }
            // something bad happened. TODO: improve non-OK response handling
            throw new Exception("Could not edit existing record.");
        }

        public async Task<BaseDataResponse> ExecuteDeleteAsync(DeleteRequest req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");
            if (string.IsNullOrEmpty(req.RecordId)) throw new ArgumentException("RecordId is required on the request.");

            // add a default request header of our data token to nuke
            _client.DefaultRequestHeaders.Add("FM-Data-token", this.dataToken);
            var response = await _client.DeleteAsync(DeleteEndpoint(req.Layout, req.RecordId));

            // process the response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BaseDataResponse>(responseJson);
                return responseObject;
            }

            throw new Exception("Could not delete record.");
        }

        public async Task<FindResponse> ExecuteFindAsync(FindRequest req)
        {
            if(string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the find request.");
            if(req.Query == null || req.Query.Count() == 0) throw new ArgumentException("Query parameters are required on the find request.");

            var httpContent = new StringContent(req.ToJson(), Encoding.UTF8, "application/json");
            httpContent.Headers.Add("FM-Data-token", this.dataToken);
            var response = await _client.PostAsync(FindEndpoint(req.Layout), httpContent);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<FindResponse>(responseJson);
                return responseObject;
            }

            throw new Exception("Find Rquest Error");
        }

        public async Task<IEnumerable<T>> FindAsync<T>(FindRequest req)
        {
            if(string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the find request.");
            if(req.Query == null || req.Query.Count() == 0) throw new ArgumentException("Query parameters are required on the find request.");

            var httpContent = new StringContent(req.ToJson(), Encoding.UTF8, "application/json");
            httpContent.Headers.Add("FM-Data-token", this.dataToken);
            var response = await _client.PostAsync(FindEndpoint(req.Layout), httpContent);

            var responseJson = await response.Content.ReadAsStringAsync();

            JObject joResponse = JObject.Parse(responseJson);

            // get JSON result objects into a list
            IList<JToken> results = joResponse["data"].Children()["fieldData"].ToList();
            
            // serialize JSON results into .NET objects
            IList<T> searchResults = new List<T>();
            foreach (JToken result in results)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                T searchResult = result.ToObject<T>();
                searchResults.Add(searchResult);
            }

            return searchResults;
        }

        public async Task<IEnumerable<T>> FindAsync<T>(T request)
        {
            throw new NotImplementedException();
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