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
    public class FileMakerRestClient : FileMakerApiClientBase, IFileMakerRestClient
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

        private readonly HttpClient _client;
        private readonly string _fmsUri;
        private readonly string _fileName;
        private readonly string _userName;
        private readonly string _password;

        /// <summary>
        /// Indicates that the client is authenticated and has a token within the refresh window.
        /// </summary>
        public bool IsAuthenticated => !String.IsNullOrEmpty(dataToken) && DateTime.UtcNow.Subtract(dataTokenLastUse).TotalMinutes <= tokenExpiration;

        #region Constructors
        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"/> instance to the class.
        /// </summary>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        /// <remarks>Pass through constructor with no real body used for injection.</remarks>
        public FileMakerRestClient(string fmsUri, string file, string user, string pass)
            : this(new HttpClient(), fmsUri, file, user, pass) { }

        /// <summary>
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"></see> instance to the class.
        /// </summary>
        /// <param name="client">An <see ref="HttpClient"/> instance to utilize for the liftime of this Data Client.</param>
        /// <param name="fmsUri">FileMaker Server HTTP Uri Endpoint.</param>
        /// <param name="file">Name of the FileMaker Database to connect to.</param>
        /// <param name="user">Account to connect with.</param>
        /// <param name="pass">Account to connect with.</param>
        public FileMakerRestClient(HttpClient client, string fmsUri, string file, string user, string pass)
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
        }
        #endregion

        #region FM DATA SPECIFIC
        internal readonly int tokenExpiration = 15;
        private string dataToken;
        private DateTime dataTokenLastUse = DateTime.MinValue;
        private async Task UpdateTokenDateAsync()
        {
            if (!IsAuthenticated) { await RefreshTokenAsync(_userName, _password); }
            dataTokenLastUse = DateTime.UtcNow;
        }
        #region API Endpoint Functions
        /// <summary>
        /// Note we assume _fmsUri has no trailing slash as its cut off in the constructor.
        /// </summary>
        private string _baseEndPoint => $"{_fmsUri}/fmi/data/v1/databases/{Uri.EscapeDataString(_fileName)}";
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
        /// <returns>The FileMaker Data API Endpoint for Get Records reqeusts.</returns>
        public string GetRecordEndpoint(string layout, int recordId) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records/{recordId}";

        /// <summary>
        /// Generate the appropriate Get Records endpoint.
        /// </summary>
        /// <param name="layout">The layout to use as the context for the response.</param>
        /// <param name="limit">The number of records to return.</param>
        /// <param name="offset">The offset number of records to skip before starting to return records.</param>
        /// <returns>The FileMaker Data API Endpoint for Get Records reqeusts.</returns>
        public string GetRecordsEndpoint(string layout, int limit, int offset) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records?_limit={limit}&_offset={offset}";

        /// <summary>
        /// Generate the appropriate Edit/Update endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <param name="recordid">The record ID of the record to edit.</param>
        /// <returns>The FileMaker Data API Endpoint for Update/Edit requests.</returns>
        public string UpdateEndpoint(string layout, object recordid) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records/{recordid}";

        /// <summary>
        /// Generate the appropriate Delete endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <param name="recordid">The record ID of the record to edit.</param>
        /// <returns>The FileMaker Data API Endpoint for Delete requests.</returns>
        public string DeleteEndpoint(string layout, object recordid) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records/{recordid}";

        /// <summary>
        /// Generate the appropriate Container field endpoint.
        /// </summary>
        /// <param name="layout">The layout to use.</param>
        /// <param name="recordid">the record ID of the record to edit.</param>
        /// <param name="fieldName">The name of the container field.</param>
        /// <param name="repetition">Field repetition number.</param>
        /// <returns></returns>
        public string ContainerEndpoint(string layout, object recordid, string fieldName, int repetition = 1) => $"{_baseEndPoint}/layouts/{Uri.EscapeDataString(layout)}/records/{recordid}/containers/{Uri.EscapeDataString(fieldName)}/{repetition}";
        #endregion

        #region FM Data Token Management

        /// <summary>
        /// <see cref="IFileMakerRestClient.RefreshTokenAsync(string, string)"/>
        /// </summary>
        public async Task<AuthResponse> RefreshTokenAsync(string username, string password)
        {
            // parameter checks
            if (string.IsNullOrEmpty(username)) throw new ArgumentException("Username is a required parameter.");
            if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password is a required parameter.");

            var authHeader = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")));
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, AuthEndpoint());
            requestMessage.Headers.Authorization = authHeader;
            requestMessage.Content = new StringContent("{ }", Encoding.UTF8, "application/json");

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
        /// <see cref="IFileMakerRestClient.LogoutAsync"/>
        /// </summary>
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
            var response = await GetFindHttpResponseAsync(new FindRequest<Dictionary<string, string>> { Layout = layout, Query = new List<Dictionary<string, string>> { req } });

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
            var response = await GetFindHttpResponseAsync(req);

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
            // normally required, but internally we can route to the regular record request apis
            var uriEndpoint = GetRecordEndpoint(layout, fileMakerId);
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, uriEndpoint);
            await UpdateTokenDateAsync(); // we're about to use the token so update date used
            var response = await _client.SendAsync(requestMessage);

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
                    // JToken.ToObject is a helper method that uses JsonSerializer internally
                    T searchResult = ConvertJTokenToInstance(fmId, modId, result);

                    // container handling
                    await ProcessContainer(searchResult);

                    // add to response list
                    searchResults.Add(searchResult);
                }

                return searchResults.FirstOrDefault();
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
                        return null;
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
                return null;
            }

            // other error TODO: Improve handling
            throw new Exception($"Find Request Error. Request Uri: {response.RequestMessage.RequestUri} responed with {response.StatusCode}");
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

            var str = req.SerializeRequest();
            var httpContent = new StringContent(str, Encoding.UTF8, "application/json");

            await UpdateTokenDateAsync(); // we're about to use the token so update date used

            // run the post action
            var response = await _client.PostAsync(CreateEndpoint(req.Layout), httpContent);

            try
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<CreateResponse>(responseJson);

                return responseObject;
            }
            catch (Exception ex)
            {
                // something bad happened. TODO: improve non-OK response handling
                throw new Exception($"Non-OK Response: Status = {response.StatusCode}.", ex);
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
            HttpResponseMessage response = await GetEditHttpResponse(req);

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

            await UpdateTokenDateAsync(); // we're about to use the token so update date used

            // add a default request header of our data token to nuke
            var response = await _client.DeleteAsync(DeleteEndpoint(req.Layout, req.RecordId));

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
        /// <param name="fmId">Function to assign the FileMaker RecordId to each instnace of {T}.</param>
        /// <param name="modId">Function to assign the FileMaker ModId to each instance of {T}.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public override async Task<IEnumerable<T>> SendAsync<T>(
            IFindRequest<T> req, 
            Func<T, int, object> fmId = null,
            Func<T, int, object> modId = null)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");

            var response = await GetFindHttpResponseAsync(req);

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
            throw new Exception($"Find Request Error. Request Uri: {response.RequestMessage.RequestUri} responed with {response.StatusCode}");
        }
        #endregion

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
        /// Puts the contents of the byte array into the specified container field.
        /// </summary>
        /// <param name="layout">The layout to perform this operation on.</param>
        /// <param name="recordId">The FileMaker RecordID of the record we want to update the container on.</param>
        /// <param name="fieldName">Name of the Container Field.</param>
        /// <param name="fileName">The name of the file being inserted into the container field.</param>
        /// <param name="repetition">Field repetition number.</param>
        /// <param name="content">The content to be inserted into the container field.</param>
        /// <returns>The FileMaker Server Response from this operation.</returns>
        public override async Task<IEditResponse> UpdateContainerAsync (
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
        /// Utility method that must be overridden in implemenations. Takes a containerfield url and populpates a byte array utilizing the instance's http client.
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
        /// Utility method to handle processing of find requests.
        /// </summary>
        /// <typeparam name="T">The type parameter of the data request.</typeparam>
        /// <param name="req">The request object to send.</param>
        /// <returns>The task that will return the http response from this</returns>
        private async Task<HttpResponseMessage> GetFindHttpResponseAsync<T>(IFindRequest<T> req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the find request.");

            if (req.Query == null || req.Query.Count() == 0)
            {
                // normally required, but internally we can route to the regular record request apis
                var uriEndpoint = GetRecordsEndpoint(req.Layout, req.Limit, req.Offset);
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, uriEndpoint);
                await UpdateTokenDateAsync(); // we're about to use the token so update date used
                return await _client.SendAsync(requestMessage);
            }

            var json = req.SerializeRequest();
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var requestMessage2 = new HttpRequestMessage(HttpMethod.Post, FindEndpoint(req.Layout))
            {
                Content = httpContent
            };

            await UpdateTokenDateAsync(); // we're about to use the token so update date used

            var response = await _client.SendAsync(requestMessage2);

            return response;
        }

        /// <summary>
        /// Utility method to handle processing of edit requests.
        /// </summary>
        /// <typeparam name="T">The type parameter of the data request.</typeparam>
        /// <param name="req">The request object to send.</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GetEditHttpResponse<T>(IEditRequest<T> req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");
            if (string.IsNullOrEmpty(req.RecordId)) throw new ArgumentException("RecordId is required on the request.");

            var str = req.SerializeRequest();
            var method = new HttpMethod("PATCH");
            var requestMessage = new HttpRequestMessage(method, UpdateEndpoint(req.Layout, req.RecordId))
            {
                Content = new StringContent(str, Encoding.UTF8, "application/json")
            };
            await UpdateTokenDateAsync(); // we're about to use the token so update date used
            // run the patch action
            var response = await _client.SendAsync(requestMessage);
            return response;
        }

        /// <summary>
        /// Converts a JToken instance and maps it to the generic type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fmId"></param>
        /// <param name="modId"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private static T ConvertJTokenToInstance<T>(Func<T, int, object> fmId, Func<T, int, object> modId, JToken input) where T : class, new()
        {
            // JToken.ToObject is a helper method that uses JsonSerializer internally
            T searchResult = input["fieldData"].ToObject<T>();

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
                // https://stackoverflow.com/a/26864676/86860 - explination 
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

            // recordId
            int fileMakerId = input["recordId"].ToObject<int>();
            fmId?.Invoke(searchResult, fileMakerId);

            // modid
            int fmmodId = input["modId"].ToObject<int>();
            modId?.Invoke(searchResult, fmmodId);
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
                    // wrapping in try...catch since if we are disposing due to other errors; we could get another one during this attempt to logout the token.
                } 
                // dispose our injected http client
                _client.Dispose();
            }
        }
        #endregion
    }
}