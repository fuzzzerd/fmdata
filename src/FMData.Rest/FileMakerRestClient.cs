using FMData.Rest.Requests;
using FMData.Rest.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FMData.Rest
{
    /// <summary>
    /// FileMaker Data API Client Implementation
    /// </summary>
    public class FileMakerRestClient : FileMakerApiClientBase, IFileMakerRestClient
    {
        private readonly HttpClient _client;
        private readonly string _fmsUri;
        private readonly string _fileName;
        private readonly string _userName;
        private readonly string _password;

        private string dataToken;
        private DateTime dataTokenLastUse = DateTime.MinValue;
        private void UpdateTokenDate() => dataTokenLastUse = DateTime.UtcNow;

        public bool IsAuthenticated => !String.IsNullOrEmpty(dataToken);// && DateTime.UtcNow.Subtract(dataTokenLastUse).TotalMinutes <= 15;

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
        /// FM Data Constructor. Injects a new plain old <see ref="HttpClient"> instance to the class.
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

            var authResponse = RefreshTokenAsync(_userName, _password);
            authResponse.Wait();
            if (authResponse.Result.Messages.Any(r => r.Message == "OK"))
            {
                dataToken = authResponse.Result.Response.Token;
            }
        }
        #endregion

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
        public string FindEndpoint(string layout) => $"{_baseEndPoint}/layouts/{layout}/_find";
        
        /// <summary>
        /// Generate the appropriate Create endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <returns>The FileMaker Data API Endpoint for Create requests.</returns>
        public string CreateEndpoint(string layout) => $"{_baseEndPoint}/layouts/{layout}/records";
        
        /// <summary>
        /// Generate the appropriate Get Records endpoint.
        /// </summary>
        /// <param name="layout">The layout to use as the context for the response.</param>
        /// <param name="limit">The number of records to return.</param>
        /// <param name="offset">The offset number of records to skip before starting to return records.</param>
        /// <returns>The FileMaker Data API Endpoint for Get Records reqeusts.</returns>
        public string GetRecordsEndpoint(string layout, int limit, int offset) => $"{_baseEndPoint}/layouts/{layout}/records?_limit={limit}&_offset={offset}";
        
        /// <summary>
        /// Generate the appropriate Edit/Update endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <param name="recordid">The record ID of the record to edit.</param>
        /// <returns>The FileMaker Data API Endpoint for Update/Edit requests.</returns>
        public string UpdateEndpoint(string layout, object recordid) => $"{_baseEndPoint}/layouts/{layout}/records/{recordid}";
        
        /// <summary>
        /// Generate the appropriate Delete endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <param name="recordid">The record ID of the record to edit.</param>
        /// <returns>The FileMaker Data API Endpoint for Delete requests.</returns>
        public string DeleteEndpoint(string layout, object recordid) => $"{_baseEndPoint}/layouts/{layout}/records/{recordid}";
        #endregion

        #region FM Data Token Management

        /// <summary>
        /// <see cref="IFileMakerApiClient.RefreshTokenAsync(string, string)"/>
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
            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<AuthResponse>(responseJson);
                this.dataToken = responseObject.Response.Token;
                
                return responseObject;
            }

            // something bad happened. TODO: improve non-OK response handling
            throw new Exception("Could not authenticate.");
        }

        /// <summary>
        /// <see cref="IFileMakerApiClient.LogoutAsync"/>
        /// </summary>
        public async Task<IResponse> LogoutAsync()
        {
            // add a default request header of our data token to nuke
            _client.DefaultRequestHeaders.Add("FM-Data-token", this.dataToken);
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

        #region Data Minipulation Functions

        #region Relay Methods //TODO: Possibly Integrate into base class
        /// <summary>
        /// Create a record in the database utilizing the TableAttribute to target the layout.
        /// </summary>
        /// <typeparam name="T">The type parameter to be created.</typeparam>
        /// <param name="layout">Explicitly define the layout to use for this request.</param>
        /// <param name="input">Object containing the data to be on the newly created record.</param>
        /// <returns></returns>
        public override Task<IResponse> CreateAsync<T>(string layout, T input) => SendAsync(new CreateRequest<T>() { Data = input, Layout = layout });

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="layout">The name of the layout to run this request on.</param>
        /// <param name="input">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public override Task<IEnumerable<T>> FindAsync<T>(string layout, T input) => SendAsync(new FindRequest<T>() { Layout = layout, Query = new List<T>() { input } });


        /// <summary>
        /// Edit a record.
        /// </summary>
        /// <typeparam name="T">Type parameter for this edit.</typeparam>
        /// <param name="layout">Explicitly define the layout to use.</param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object with the updated values.</param>
        /// <returns></returns>
        public override Task<IResponse> EditAsync<T>(string layout, int recordId, T input) => SendAsync(new EditRequest<T>() { Data = input, Layout = layout, RecordId = recordId.ToString() });

        /// <summary>
        /// Edit a record.
        /// </summary>
        /// <param name="layout">Explicitly define the layout to use.</param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to be edited.</param>
        /// <param name="editValues">Object with the updated values.</param>
        /// <returns></returns>
        public override Task<IResponse> EditAsync(int recordId, string layout, Dictionary<string, string> editValues) => SendAsync(new EditRequest<Dictionary<string, string>>() { Data = editValues, Layout = layout, RecordId = recordId.ToString() });


        /// <summary>
        /// Delete a record by FileMaker RecordId and explicit layout.
        /// </summary>
        /// <param name="recId"></param>
        /// <param name="layout"></param>
        /// <returns></returns>
        public override Task<IResponse> DeleteAsync(int recId, string layout) => SendAsync(new DeleteRequest { Layout = layout, RecordId = recId.ToString() });
        #endregion

        #region Special Implementations
        /// <summary>
        /// General purpose Find Request method. Supports additional syntaxes like the { "omit" : "true" } operation.
        /// This method returns a strongly typed <see cref="IEnumerable{T}"/> but accepts a the more flexible <see cref="Dictionary{string,string}"/> request parameters.
        /// </summary>
        /// <typeparam name="T">the type of response objects to return.</typeparam>
        /// <param name="req">The find request dictionary.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        /// <remarks>Can't be a relay method, since we have to process the data specially to get our output</remarks>
        public override async Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req)
        {
            var response = await GetFindHttpResponseAsync(new FindRequest<Dictionary<string, string>> { Layout = layout, Query = new List<Dictionary<string, string>> { req } });

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<FindResponse<T>>(responseJson);
                return responseObject.Response.Data.Select(d => d.FieldData);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // on 404 return empty set instead of throwing an exception
                // since this is an expected case
                return new List<T>();
            }

            throw new Exception("Find request Error");
        }

        /// <summary>
        /// General purpose Find Request method. Supports additional syntaxes like the { "omit" : "true" } operation.
        /// </summary>
        /// <param name="req">The find request field/value dictionary to pass into FileMaker server.</param>
        /// <returns>A <see cref="Dictionary{String,String}"/> wrapped in a FindResponse containing both record data and portal data.</returns>
        public override async Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req)
        {
            var response = await GetFindHttpResponseAsync(req);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<FindResponse<Dictionary<string, string>>>(responseJson);
                return responseObject;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // on 404 return empty set instead of throwing an exception
                // since this is an expected case
                return new FindResponse<Dictionary<string, string>>();
            }

            throw new Exception("Find Request Error");
        } 
        #endregion


        /// <summary>
        /// Create a record in the database using the CreateRequest object.
        /// </summary>
        /// <typeparam name="T">The underlying type of record being created.</typeparam>
        /// <param name="req">The request object containing the data to be sent.</param>
        /// <returns></returns>
        public override async Task<IResponse> SendAsync<T>(ICreateRequest<T> req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");

            var str = req.SerializeRequest();
            var httpContent = new StringContent(str, Encoding.UTF8, "application/json");
            httpContent.Headers.Add("FM-Data-token", this.dataToken);

            // run the post action
            var response = await _client.PostAsync(CreateEndpoint(req.Layout), httpContent);

            // process the response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BaseResponse>(responseJson);
                return responseObject;
            }
            // something bad happened. TODO: improve non-OK response handling
            throw new Exception("Could not Create new record.");
        }

        /// <summary>
        /// Edit a record utilizing a generic parameter type to house the fields to be edited.
        /// </summary>
        /// <typeparam name="T">Type parameter for this edit.</typeparam>
        /// <param name="req">The edit request object.</param>
        /// <returns></returns>
        public override async Task<IResponse> SendAsync<T>(IEditRequest<T> req)
        {
            HttpResponseMessage response = await GetEditHttpResponse(req);

            // process the response
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BaseResponse>(responseJson);
                return responseObject;
            }
            // something bad happened. TODO: improve non-OK response handling
            throw new Exception("Could not edit existing record.");
        }

        /// <summary>
        /// Delete a record.
        /// </summary>
        /// <param name="req">The delete record request.</param>
        /// <returns></returns>
        public override async Task<IResponse> SendAsync(IDeleteRequest req)
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
                var responseObject = JsonConvert.DeserializeObject<BaseResponse>(responseJson);
                return responseObject;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new BaseResponse("404", "Error");
            }

            throw new Exception("Could not delete record.");
        }

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="req">The find request parameters.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public override async Task<IEnumerable<T>> SendAsync<T>(IFindRequest<T> req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the request.");

            var response = await GetFindHttpResponseAsync(req);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();

                JObject joResponse = JObject.Parse(responseJson);

                // get JSON result objects into a list
                IList<JToken> results = joResponse["response"]["data"].Children()["fieldData"].ToList();

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

            // not found, so return empty list
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<T>();
            }

            // other error TODO: Improve handling
            throw new Exception("Find request error");
        }

        #endregion

        #region Private Helpers and utility methods

        /// <summary>
        /// Utility method to handle processing of find requests.
        /// </summary>
        /// <typeparam name="T">The type parameter of the data request.</typeparam>
        /// <param name="req">The request object to send.</param>
        /// <returns>The task that will return the http response from this</returns>
        private Task<HttpResponseMessage> GetFindHttpResponseAsync<T>(IFindRequest<T> req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the find request.");

            var authHeader = new AuthenticationHeaderValue("Bearer", this.dataToken);

            if (req.Query == null || req.Query.Count() == 0)
            {
                // normally required, but internally we can route to the regular record request apis
                var uriEndpoint = GetRecordsEndpoint(req.Layout, req.Limit, req.Offset);
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, uriEndpoint);
                requestMessage.Headers.Authorization = authHeader;
                return _client.SendAsync(requestMessage);
            }

            var json = req.SerializeRequest();
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            var requestMessage2 = new HttpRequestMessage(HttpMethod.Post, FindEndpoint(req.Layout))
            {
                Content = httpContent
            };
            requestMessage2.Headers.Authorization = authHeader;

            var response = _client.SendAsync(requestMessage2);

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

            var str = JsonConvert.SerializeObject(req);
            var httpContent = new StringContent(str, Encoding.UTF8, "application/json");
            httpContent.Headers.Add("FM-Data-token", this.dataToken);

            // run the post action
            var response = await _client.PutAsync(UpdateEndpoint(req.Layout, req.RecordId), httpContent);
            return response;
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
                // end our token
                LogoutAsync().Wait();

                // dispose our injected http client
                _client.Dispose();
            }
        }
        #endregion
    }
}