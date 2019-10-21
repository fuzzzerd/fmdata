using FMData.Rest.Requests;
using FMData.Rest.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FMData.Rest
{
    /// <summary>
    /// FileMaker Data API Client Implementation.
    /// </summary>
    public class FileMakerRestClient : FileMakerApiClientBase, IFileMakerApiClient
    {
        #region Request Factories
        /// <summary>
        /// Factory to get a new Create Request of the correct type.
        /// </summary>
        protected override ICreateRequest<T> _createFactory<T>() => new CreateRequest<T>();
        /// <summary>
        /// Factory to get a new Edit Request of the correct type.
        /// </summary>
        protected override IEditRequest<T> _editFactory<T>() => new EditRequest<T>();
        /// <summary>
        /// Factory to get a new Find Request of the correct type.
        /// </summary>
        protected override IFindRequest<T> _findFactory<T>() => new FindRequest<T>();
        /// <summary>
        /// Factory to get a new Delete Request of the correct type.
        /// </summary>
        protected override IDeleteRequest _deleteFactory() => new DeleteRequest();
        #endregion

        /// <summary>
        /// Indicates that the client is authenticated and has a token within the refresh window.
        /// </summary>
        public bool IsAuthenticated => !string.IsNullOrEmpty(dataToken) && DateTime.UtcNow.Subtract(dataTokenLastUse).TotalMinutes <= tokenExpiration;

        #region FM DATA SPECIFIC
        internal readonly int tokenExpiration = 15;
        private string dataToken;
        private DateTime dataTokenLastUse = DateTime.MinValue;

        #region Constructors
        /// <summary>
        /// Create a FileMakerRestClient with a new instance of HttpClient.
        /// </summary>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        [Obsolete("Creates a new HttpClient for this instance, and is generally not good. Inject a managed client.")]
        public FileMakerRestClient(string fmsUri, string file, string user, string pass)
            : this(new HttpClient(), new ConnectionInfo { FmsUri = fmsUri, Database = file, Username = user, Password = pass }) { }

        /// <summary>
        /// FM Data Constructor with HttpClient and ConnectionInfo. Useful for Dependency Injection situations.
        /// </summary>
        /// <param name="client">The HttpClient instance to use.</param>
        /// <param name="conn">The connection information for FMS.</param>
        public FileMakerRestClient(HttpClient client, ConnectionInfo conn)
            : base(client, conn) { }
        #endregion
        private async Task UpdateTokenDateAsync()
        {
            if (!IsAuthenticated) { await RefreshTokenAsync(_userName, _password); }
            dataTokenLastUse = DateTime.UtcNow;
        }
        #region API Endpoint Functions
        /// <summary>
        /// Note we assume _fmsUri has no trailing slash as its cut off in the constructor.
        /// </summary>
        private string _baseEndPoint => $"{_fmsUri}/fmi/data/v1/databases/{_fileName}";

        /// <summary>
        /// Generate the appropriate Authentication endpoint uri for this instance of the data client.
        /// </summary>
        /// <returns>The FileMaker Data API Endpoint for Authentication Requests.</returns>
        public string AuthEndpoint() => $"{_baseEndPoint}/sessions";

        /// <summary>
        /// Generate the appropriate Find endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <returns>The FileMaker Data API Endpoint for Find requests.</returns>
        public string FindEndpoint(string layout) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/_find";

        /// <summary>
        /// Generate the appropriate Create endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <returns>The FileMaker Data API Endpoint for Create requests.</returns>
        public string CreateEndpoint(string layout) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records";

        /// <summary>
        /// Generate the appropriate Get Records endpoint.
        /// </summary>
        /// <param name="layout">The layout to use as the context for the response.</param>
        /// <param name="recordId">The FileMaker record Id for this request.</param>
        /// <returns>The FileMaker Data API Endpoint for Get Records requests.</returns>
        public string GetRecordEndpoint(string layout, int recordId) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records/{recordId}";

        /// <summary>
        /// Generate the appropriate Get Records endpoint.
        /// </summary>
        /// <param name="layout">The layout to use as the context for the response.</param>
        /// <param name="limit">The number of records to return.</param>
        /// <param name="offset">The offset number of records to skip before starting to return records.</param>
        /// <returns>The FileMaker Data API Endpoint for Get Records requests.</returns>
        public string GetRecordsEndpoint(string layout, int limit, int offset) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records?_limit={limit}&_offset={offset}";

        /// <summary>
        /// Generate the appropriate Edit/Update endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <param name="recordId">The record ID of the record to edit.</param>
        /// <returns>The FileMaker Data API Endpoint for Update/Edit requests.</returns>
        public string UpdateEndpoint(string layout, object recordId) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records/{recordId}";

        /// <summary>
        /// Generate the appropriate Delete endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <param name="recordId">The record ID of the record to edit.</param>
        /// <returns>The FileMaker Data API Endpoint for Delete requests.</returns>
        public string DeleteEndpoint(string layout, object recordId) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records/{recordId}";

        /// <summary>
        /// Generate the appropriate Container field endpoint.
        /// </summary>
        /// <param name="layout">The layout to use.</param>
        /// <param name="recordId">the record ID of the record to edit.</param>
        /// <param name="fieldName">The name of the container field.</param>
        /// <param name="repetition">Field repetition number.</param>
        /// <returns></returns>
        public string ContainerEndpoint(string layout, object recordId, string fieldName, int repetition = 1) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records/{recordId}/containers/{Uri.EscapeDataString(fieldName)}/{repetition}";
        #endregion

        #region FM Data Token Management

        /// <summary>
        /// Refreshes the internally stored authentication token from filemaker server.
        /// </summary>
        /// <param name="username">Username of the account to authenticate.</param>
        /// <param name="password">Password of the account to authenticate.</param>
        /// <returns>An AuthResponse from deserialized from FileMaker Server json response.</returns>
        public async Task<AuthResponse> RefreshTokenAsync(string username, string password)
        {
            // parameter checks
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username is a required parameter.");
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password is a required parameter.");

            var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, AuthEndpoint());
            requestMessage.Headers.Authorization = authHeader;
            requestMessage.Content = new StringContent("{ }", Encoding.UTF8, "application/json");

            // do not pass character set. 
            // this is due to fms 18 returning Bad Request when specified
            // this hack is backward compatible for FMS17
            requestMessage.Content.Headers.ContentType.CharSet = null;

            // run the post action
            var response = await _client.SendAsync(requestMessage);

            // process the response even a 401 returns a FMS error to be passed back.
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<AuthResponse>(responseJson);
                this.dataToken = responseObject.Response.Token;

                // got a new token, so update our timestamp
                this.dataTokenLastUse = DateTime.UtcNow;
                // setup the token as an auth bearer header.
                this._client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.dataToken);

                return responseObject;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<AuthResponse>(responseJson);
                return responseObject;
            }

            // something bad happened. TODO: improve non-OK response handling
            throw new Exception("Could not authenticate.");
        }

        /// <summary>
        /// Logs the user out and nullifies the token.
        /// </summary>
        /// <returns>FileMaker Response</returns>
        public async Task<IResponse> LogoutAsync()
        {
            // add a default request header of our data token to nuke
            var response = await _client.DeleteAsync(AuthEndpoint() + $"/{this.dataToken}");

            // process the response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BaseResponse>(responseJson);
                return responseObject;
            }

            throw new Exception("Could not logout.");
        }
        #endregion
        #endregion

        #region Special Implementations
        /// <summary>
        /// General purpose Find Request method. Supports additional syntaxes like the { "omit" : "true" } operation.
        /// This method returns a strongly typed <see cref="IEnumerable{T}"/> but accepts a the more flexible <see cref="Dictionary{TKey, TValue}"/> request parameters.
        /// </summary>
        /// <typeparam name="T">the type of response objects to return.</typeparam>
        /// <param name="layout">The layout to perform the find request on.</param>
        /// <param name="req">The find request dictionary.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        /// <remarks>Can't be a relay method, since we have to process the data specially to get our output</remarks>
        public override async Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req)
        {
            if (string.IsNullOrEmpty(layout)) throw new ArgumentException("Layout is required on the request.");

            var fmdataRequest = new FindRequest<Dictionary<string, string>> { Layout = layout };

            fmdataRequest.AddQuery(req, false);

            var response = await ExecuteRequestAsync(HttpMethod.Post, FindEndpoint(layout), fmdataRequest);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // on 404 return empty set instead of throwing an exception
                // since this is an expected case
                return new List<T>();
            }

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<FindResponse<T>>(responseJson);

                return responseObject.Response.Data.Select(d => d.FieldData);
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// General purpose Find Request method. Supports additional syntaxes like the { "omit" : "true" } operation.
        /// </summary>
        /// <param name="req">The find request field/value dictionary to pass into FileMaker server.</param>
        /// <returns>A <see cref="Dictionary{String,String}"/> wrapped in a FindResponse containing both record data and portal data.</returns>
        public override async Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");

            var uri = FindEndpoint(req.Layout);
            var response = await ExecuteRequestAsync(HttpMethod.Post, uri, req);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // on 404 return empty set instead of throwing an exception
                // since this is an expected case
                return new FindResponse<Dictionary<string, string>>();
            }

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<FindResponse<Dictionary<string, string>>>(responseJson);

                return responseObject;
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="layout">The layout to execute the request on.</param>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <param name="modId">The function to use to map the ModId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns> 
        public override async Task<T> GetByFileMakerIdAsync<T>(
            string layout,
            int fileMakerId,
            Func<T, int, object> fmId = null,
            Func<T, int, object> modId = null)
        {
            if (string.IsNullOrEmpty(layout)) throw new ArgumentException("Layout is required on the request.");

            // normally required, but internally we can route to the regular record request apis
            var uriEndpoint = GetRecordEndpoint(layout, fileMakerId);
            var response = await ExecuteRequestAsync(HttpMethod.Get, uriEndpoint, new FindRequest<T>());

            if (response.StatusCode != HttpStatusCode.OK)
            {
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        return null;
                    case HttpStatusCode.InternalServerError:
                        // attempt to read response content
                        if (response.Content == null) { throw new Exception("Could not read response from Data API."); }
                        var responseJsonEx = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonConvert.DeserializeObject<BaseResponse>(responseJsonEx);
                        if (responseObject.Messages.Any(m => m.Code == "401"))
                        {
                            return null;
                        }
                        throw new Exception(responseObject.Messages.First().Message);
                    default:
                        // other error TODO: Improve handling
                        throw new Exception($"Find Request Error. Request Uri: {response.RequestMessage.RequestUri} responded with {response.StatusCode}");
                }
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            JObject joResponse = JObject.Parse(responseJson);

            // get JSON result objects into a list
            IList<JToken> results = joResponse["response"]["data"].Children().ToList();

            // serialize JSON results into .NET objects
            IList<T> searchResults = new List<T>();
            foreach (JToken result in results)
            {
                // JToken.ToObject is a helper method that uses JsonSerializer internally
                T searchResult = ConvertJTokenToInstance(fmId, modId, result);

                // container handling
                await ProcessContainer(searchResult);

                // add to response list
                searchResults.Add(searchResult);
            }

            return searchResults.FirstOrDefault();
        }
        #endregion

        #region SendAsync Implementations

        /// <summary>
        /// Create a record in the database using the CreateRequest object.
        /// </summary>
        /// <typeparam name="T">The underlying type of record being created.</typeparam>
        /// <param name="req">The request object containing the data to be sent.</param>
        /// <returns></returns>
        public override async Task<ICreateResponse> SendAsync<T>(ICreateRequest<T> req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");

            var responseMessage = await ExecuteRequestAsync(req);

            try
            {
                var responseJson = await responseMessage.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<CreateResponse>(responseJson);
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {responseMessage.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Edit a record utilizing a generic parameter type to house the fields to be edited.
        /// </summary>
        /// <typeparam name="T">Type parameter for this edit.</typeparam>
        /// <param name="req">The edit request object.</param>
        /// <returns></returns>
        public override async Task<IEditResponse> SendAsync<T>(IEditRequest<T> req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");
            if (req.RecordId <= 0) throw new ArgumentException("RecordId is required on the request and non negative.");

            HttpResponseMessage response = await ExecuteRequestAsync(req);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new BaseResponse("404", "Error") as EditResponse;
            }

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<EditResponse>(responseJson);

                return responseObject;
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Delete a record.
        /// </summary>
        /// <param name="req">The delete record request.</param>
        /// <returns></returns>
        public override async Task<IResponse> SendAsync(IDeleteRequest req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");
            if (req.RecordId == 0) throw new ArgumentException("RecordId is required on the request and must not be zero.");

            var response = await ExecuteRequestAsync(req);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new BaseResponse("404", "Error");
            }

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BaseResponse>(responseJson);
                return responseObject;
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="req">The find request parameters.</param>
        /// <param name="fmId">Function to assign the FileMaker RecordId to each instance of {T}.</param>
        /// <param name="modId">Function to assign the FileMaker ModId to each instance of {T}.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public override async Task<IEnumerable<T>> SendAsync<T>(
            IFindRequest<T> req,
            Func<T, int, object> fmId = null,
            Func<T, int, object> modId = null)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the find request.");

            var uri = FindEndpoint(req.Layout);
            var method = HttpMethod.Post;

            if (req.Query == null || req.Query.Count() == 0)
            {
                // if this is an empty query, just punch it in to the Records API instead of the Find API.
                uri = GetRecordsEndpoint(req.Layout, req.Limit, req.Offset);
                method = HttpMethod.Get;
            }

            var response = await ExecuteRequestAsync(method, uri, req);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();

                JObject joResponse = JObject.Parse(responseJson);

                // get JSON result objects into a list
                IList<JToken> results = joResponse["response"]["data"].Children().ToList();

                // serialize JSON results into .NET objects
                IList<T> searchResults = new List<T>();
                foreach (JToken result in results)
                {
                    T searchResult = ConvertJTokenToInstance(fmId, modId, result);

                    // add to response list
                    searchResults.Add(searchResult);
                }

                // make container processing part of the request, IF specified in the original request.
                if (req.LoadContainerData)
                {
                    await ProcessContainers(searchResults);
                }

                return searchResults;
            }

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                try
                {
                    // attempt to read response content
                    if (response.Content == null) { throw new Exception("Could not read response from Data API."); }

                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<BaseResponse>(responseJson);
                    if (responseObject.Messages.Any(m => m.Code == "401"))
                    {
                        // filemaker no records match the find request => empty list.
                        return new List<T>();
                    }

                    throw new Exception(responseObject.Messages.First().Message);
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not read response from Data API.", ex);
                }
            }

            // not found, so return empty list
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<T>();
            }

            // other error TODO: Improve handling
            throw new Exception($"Find Request Error. Request Uri: {response.RequestMessage.RequestUri} responded with {response.StatusCode}");
        }
        #endregion

        /// <summary>
        /// Executes a FileMaker Request to a JSON string.
        /// </summary>
        /// <param name="method">The http method to use for the request.</param>
        /// <param name="requestUri"></param>
        /// <param name="req">The request to execute.</param>
        /// <returns>The JSON string returned from FMS.</returns>
        public async Task<HttpResponseMessage> ExecuteRequestAsync(
            HttpMethod method,
            string requestUri,
            IFileMakerRequest req)
        {
            var str = req.SerializeRequest();
            var httpContent = new StringContent(str, Encoding.UTF8, "application/json");

            // do not pass character set. 
            // this is due to fms 18 returning Bad Request when specified
            // this hack is backward compatible for FMS17
            httpContent.Headers.ContentType.CharSet = null;

            var httpRequest = new HttpRequestMessage(method, requestUri)
            {
                Content = httpContent
            };

            // we're about to use the token so update date used, and refresh if needed.
            await UpdateTokenDateAsync();

            // run and return the action
            var response = await _client.SendAsync(httpRequest);
            return response;
        }

        /// <summary>
        /// Helper For Getting Raw Responses from Data API.
        /// </summary>
        public Task<HttpResponseMessage> ExecuteRequestAsync<T>(ICreateRequest<T> req) => ExecuteRequestAsync(HttpMethod.Post, CreateEndpoint(req.Layout), req);

        /// <summary>
        /// Helper For Getting Raw Responses from Data API.
        /// </summary>
        public Task<HttpResponseMessage> ExecuteRequestAsync<T>(IEditRequest<T> req) => ExecuteRequestAsync(new HttpMethod("PATCH"), UpdateEndpoint(req.Layout, req.RecordId), req);
        /// <summary>
        /// Helper For Getting Raw Responses from Data API.
        /// </summary>
        public Task<HttpResponseMessage> ExecuteRequestAsync<T>(IFindRequest<T> req) => ExecuteRequestAsync(HttpMethod.Post, FindEndpoint(req.Layout), req);

        /// <summary>
        /// Helper For Getting Raw Responses from Data API.
        /// </summary>
        public Task<HttpResponseMessage> ExecuteRequestAsync(IDeleteRequest req) => ExecuteRequestAsync(HttpMethod.Delete, DeleteEndpoint(req.Layout, req.RecordId), req);

        /// <summary>
        /// Set the value of global fields.
        /// // https://fmhelp.filemaker.com/docs/17/en/dataapi/#set-global-fields
        /// </summary>
        /// <param name="baseTable">The base table on which this global field is defined.</param>
        /// <param name="fieldName">The name of the global field to set.</param>
        /// <param name="targetValue">The target value for this global field.</param>
        /// <returns>FileMaker Response</returns>
        public override async Task<IResponse> SetGlobalFieldAsync(string baseTable, string fieldName, string targetValue)
        {
            if (string.IsNullOrEmpty(baseTable)) throw new ArgumentException("baseTable is required on set global.");
            if (string.IsNullOrEmpty(fieldName)) throw new ArgumentException("fieldName is required on set global.");
            if (string.IsNullOrEmpty(targetValue)) throw new ArgumentException("targetValue is required on set global.");

            // build the request for global fields manually
            var str = $"{{ \"globalFields\" : {{ \"{baseTable}::{fieldName}\" : \"{targetValue}\" }} }}";
            var method = new HttpMethod("PATCH");
            var requestMessage = new HttpRequestMessage(method, $"{_baseEndPoint}/globals")
            {
                Content = new StringContent(str, Encoding.UTF8, "application/json")
            };

            await UpdateTokenDateAsync(); // we're about to use the token so update date used

            // run the patch action
            var response = await _client.SendAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new BaseResponse("404", "Error");
            }

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BaseResponse>(responseJson);
                return responseObject;
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Get FileMaker Server Product Information.
        /// </summary>
        /// <returns>An instance of the FileMaker Product Info.</returns>
        public async override Task<ProductInformation> GetProductInformationAsync()
        {
            await UpdateTokenDateAsync(); // we're about to use the token so update date used

            // generate request url
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_fmsUri}/fmi/data/v1/productinfo");

            // run the patch action
            var response = await _client.SendAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            try
            {
                // process json as JObject and only grab the part we're interested in (response.productInfo).
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseJObject = JObject.Parse(responseJson);
                var responseObject = responseJObject["response"]["productInfo"].ToObject<ProductInformation>();
                return responseObject;
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Get the databases the current instance is authorized to access.
        /// </summary>
        /// <returns>The names of the databases the current user is able to connect.</returns>
        public async override Task<IReadOnlyCollection<string>> GetDatabasesAsync()
        {
            // don't need to refresh the token, because this is a basic authentication request

            // generate request url
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"{_fmsUri}/fmi/data/v1/databases");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("basic", Convert.ToBase64String(
                    Encoding.UTF8.GetBytes($"{_userName}:{_password}")
                )
            );

            // run the patch action
            var response = await _client.SendAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            try
            {
                // process json as JObject and only grab the part we're interested in (response.productInfo).
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseJObject = JObject.Parse(responseJson);
                var responseObject = responseJObject["response"]["databases"];
                return responseObject.Select(t => t.Value<string>("name")).ToList();
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Gets all the layouts within a database
        /// </summary>
        /// <param name="database">The database to query.</param>
        /// <returns>The names of the layouts in the specified database.</returns>
        public async override Task<IReadOnlyCollection<LayoutListItem>> GetLayoutsAsync()
        {
            await UpdateTokenDateAsync(); // we're about to use the token so update date used

            // generate request url{
            var uri = $"{_fmsUri}/fmi/data/v1/databases/{_fileName}/layouts";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            // run the patch action
            var response = await _client.SendAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            try
            {
                // process json as JObject and only grab the part we're interested in (response.productInfo).
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseJObject = JObject.Parse(responseJson);
                var responseObject = responseJObject["response"]["layouts"].ToObject<IReadOnlyCollection<LayoutListItem>>();
                return responseObject;
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Gets all the scripts within the database.
        /// </summary>
        /// <returns>The names of the scripts in the specified database.</returns>
        public async override Task<IReadOnlyCollection<ScriptListItem>> GetScriptsAsync()
        {
            await UpdateTokenDateAsync(); // we're about to use the token so update date used

            // generate request url{
            var uri = $"{_fmsUri}/fmi/data/v1/databases/{_fileName}/scripts";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            // run the patch action
            var response = await _client.SendAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            try
            {
                // process json as JObject and only grab the part we're interested in (response.productInfo).
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseJObject = JObject.Parse(responseJson);
                var responseObject = responseJObject["response"]["scripts"].ToObject<IReadOnlyCollection<ScriptListItem>>();
                return responseObject;
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Gets the metadata for a layout object.
        /// </summary>
        /// <param name="layout">The layout to get data about.</param>
        /// <param name="recordId">Optional RecordId, for getting layout data specific to a record. ValueLists, etc.</param>
        /// <returns>An instance of the LayoutMetadata class for the specified layout.</returns>
        public async override Task<LayoutMetadata> GetLayoutAsync(string layout, int? recordId = null)
        {
            await UpdateTokenDateAsync(); // we're about to use the token so update date used

            // generate request url
            var uri = $"{_fmsUri}/fmi/data/v1/databases/{_fileName}/layouts/{layout}";
            if (recordId.HasValue)
            {
                uri += $"?recordId={recordId}";
            }
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri);

            // run the patch action
            var response = await _client.SendAsync(requestMessage);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            try
            {
                // process json as JObject and only grab the part we're interested in (response.productInfo).
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseJObject = JObject.Parse(responseJson);
                var responseObject = responseJObject["response"].ToObject<LayoutMetadata>();
                // set the layout name on this instance, since it doesn't come back from the api
                responseObject.Name = layout;
                return responseObject;
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Puts the contents of the byte array into the specified container field.
        /// </summary>
        /// <param name="layout">The layout to perform this operation on.</param>
        /// <param name="recordId">The FileMaker RecordID of the record we want to update the container on.</param>
        /// <param name="fieldName">Name of the Container Field.</param>
        /// <param name="fileName">The name of the file being inserted into the container field.</param>
        /// <param name="repetition">Field repetition number.</param>
        /// <param name="content">The content to be inserted into the container field.</param>
        /// <returns>The FileMaker Server Response from this operation.</returns>
        public override async Task<IEditResponse> UpdateContainerAsync(
            string layout,
            int recordId,
            string fieldName,
            string fileName,
            int repetition,
            byte[] content)
        {
            var form = new MultipartFormDataContent();

            //var stream = new MemoryStream(content);
            //var streamContent = new StreamContent(stream);
            var uri = ContainerEndpoint(layout, recordId, fieldName, repetition);

            var containerContent = new ByteArrayContent(content);
            containerContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

            form.Add(containerContent, "upload", Path.GetFileName(fileName));

            await UpdateTokenDateAsync();
            var response = await _client.PostAsync(uri, form);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new BaseResponse("404", "Error") as EditResponse;
            }

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<EditResponse>(responseJson);

                return responseObject;
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
            }
        }

        /// <summary>
        /// Utility method that must be overridden in implementations. Takes a containerfield url and populates a byte array utilizing the instance's http client.
        /// </summary>
        /// <param name="containerEndPoint">The container field to load.</param>
        /// <returns>An array of bytes with the data from the container field.</returns>
        protected override async Task<byte[]> GetContainerOnClient(string containerEndPoint)
        {
            var data = await _client.GetAsync(containerEndPoint);
            var dataBytes = await data.Content.ReadAsByteArrayAsync();
            return dataBytes;
        }

        #region Private Helpers and utility methods
        /// <summary>
        /// Converts a JToken instance and maps it to the generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fmId">FileMaker Record Id map function.</param>
        /// <param name="modId">Modification Id map function.</param>
        /// <param name="input">JSON.NET JToken instance from Data Api Response.</param>
        /// <returns></returns>
        private static T ConvertJTokenToInstance<T>(Func<T, int, object> fmId, Func<T, int, object> modId, JToken input) where T : class, new()
        {
            // JToken.ToObject is a helper method that uses JsonSerializer internally
            T searchResult = null;
            try
            {
                searchResult = input["fieldData"].ToObject<T>();
            }
            catch (System.Exception ex)
            {
                // something went wrong converting the base model, so we wrap that exception and push it along
                throw new InvalidDataException($"Error converting Data API response to instance of {typeof(T).Name}", ex);
            }

            try
            {
                var portals = typeof(T).GetTypeInfo().DeclaredProperties.Where(p => p.GetCustomAttribute<PortalDataAttribute>() != null);
                foreach (var portal in portals)
                {
                    var portalDataAttr = portal.GetCustomAttribute<PortalDataAttribute>();
                    var namedPortal = portalDataAttr.NamedPortalInstance;
                    var portalInstanceType = portal.PropertyType.GetTypeInfo().GenericTypeArguments[0];
                    var pt = portal.PropertyType;
                    JToken portalJ = input["portalData"][namedPortal];

                    // .ToList() here so we iterate on a different copy of the collection
                    // which allows for calling add/remove on the list ;) clever
                    // https://stackoverflow.com/a/26864676/86860 - explication 
                    // https://stackoverflow.com/a/604843/86860 - solution
                    foreach (JObject jo in portalJ.ToList())
                    {
                        foreach (JProperty jp in jo.Properties().ToList())
                        {
                            if (jp.Name.Contains(portalDataAttr.TablePrefixFieldNames + "::"))
                            {
                                jo.Add(jp.Name.Replace(portalDataAttr.TablePrefixFieldNames + "::", ""), jp.Value);
                                jo.Remove(jp.Name);
                            }
                        }
                    }

                    var x = portalJ.ToObject(pt);
                    portal.SetValue(searchResult, x);
                }
            }
            catch (System.Exception ex)
            {
                throw new InvalidDataException("Error converting Portal Data", ex);
            }

            // recordId
            int fileMakerId = input["recordId"].ToObject<int>();
            fmId?.Invoke(searchResult, fileMakerId);

            // modId
            int fmModId = input["modId"].ToObject<int>();
            modId?.Invoke(searchResult, fmModId);

            return searchResult;
        }
        #endregion

        #region IDisposable Implementation
        /// <summary>
        /// Dispose resources opened for this instance of the data client.
        /// </summary>
        public override void Dispose()
        {
            if (_client != null)
            {
                try
                {
                    // end our token, utilize the threadpool to ensure we do not block 
                    // https://blogs.msdn.microsoft.com/pfxteam/2012/04/13/should-i-expose-synchronous-wrappers-for-asynchronous-methods/
                    Task.Run(() => LogoutAsync()).Wait();
                }
                catch
                {
                    // wrapping in try...catch since if we are disposing due to other errors; 
                    // we could get another one during this attempt to logout the token.
                }
                // specifically don't dispose our httpclient
                // this is actually bad practice for httpclient, even though it's IDisposable.
                // _client.Dispose(); // leaving out specifically
            }
        }
        #endregion
    }
}