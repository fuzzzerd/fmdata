using FMData.Requests;
using FMData.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        #region Constructors
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
        #endregion

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

        #region Data Minipulation Functions

        /// <summary>
        /// Create a record in the database utilizing the TableAttribute to target the layout.
        /// </summary>
        /// <typeparam name="T">The type parameter to be created.</typeparam>
        /// <param name="input">Object containing the data to be on the newly created record.</param>
        /// <returns></returns>
        public Task<BaseDataResponse> CreateAsync<T>(T input) => CreateAsync(GetTableName(input), input);

        /// <summary>
        /// Create a record in the database utilizing the TableAttribute to target the layout.
        /// </summary>
        /// <typeparam name="T">The type parameter to be created.</typeparam>
        /// <param name="layout">Explicitly define the layout to use for this request.</param>
        /// <param name="input">Object containing the data to be on the newly created record.</param>
        /// <returns></returns>
        public Task<BaseDataResponse> CreateAsync<T>(string layout, T input) => CreateAsync(new CreateRequest<T>() { Data = input, Layout = layout });

        /// <summary>
        /// Create a record in the database using the CreateRequest object.
        /// </summary>
        /// <typeparam name="T">The underlying type of record being created.</typeparam>
        /// <param name="req">The request object containing the data to be sent.</param>
        /// <returns></returns>
        public async Task<BaseDataResponse> CreateAsync<T>(CreateRequest<T> req)
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

        /// <summary>
        /// Edit a record.
        /// </summary>
        /// <typeparam name="T">Type parameter for this edit.</typeparam>
        /// <param name="recordId">The internal FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object with the updated values.</param>
        /// <returns></returns>
        public Task<BaseDataResponse> EditAsync<T>(int recordId, T input) => EditAsync(GetTableName(input), recordId, input);

        /// <summary>
        /// Edit a record.
        /// </summary>
        /// <typeparam name="T">Type parameter for this edit.</typeparam>
        /// <param name="layout">Explicitly define the layout to use.</param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object with the updated values.</param>
        /// <returns></returns>
        public Task<BaseDataResponse> EditAsync<T>(string layout, int recordId, T input) => EditAsync(new EditRequest<T>() { Data = input, Layout = layout, RecordId = recordId.ToString() });

        /// <summary>
        /// Edit a record utilizing a dictionary of key/values for the data field.
        /// </summary>
        /// <param name="req">The edit request object.</param>
        /// <returns></returns>
        public async Task<BaseDataResponse> EditAsync(EditRequest<Dictionary<string, string>> req)
        {
            HttpResponseMessage response = await GetEditHttpResponse(req);

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

        /// <summary>
        /// Edit a record utilizing a generic parameter type to house the fields to be edited.
        /// </summary>
        /// <typeparam name="T">Type parameter for this edit.</typeparam>
        /// <param name="req">The edit request object.</param>
        /// <returns></returns>
        public async Task<BaseDataResponse> EditAsync<T>(EditRequest<T> req)
        {
            HttpResponseMessage response = await GetEditHttpResponse(req);

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

        public async Task<BaseDataResponse> DeleteAsync(DeleteRequest req)
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

        /// <summary>
        /// General purpose Find Request method. Supports additional syntaxes like the { "omit" : "true" } operation.
        /// </summary>
        /// <param name="req">The find request field/value dictionary to pass into FileMaker server.</param>
        /// <returns>A <see cref="Dictionary{String,String}"/> wrapped in a FindResponse containing both record data and portal data.</returns>
        public async Task<FindResponse<Dictionary<string, string>>> FindAsync(FindRequest<Dictionary<string, string>> req)
        {
            var response = await GetFindHttpResponseAsync(req);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<FindResponse<Dictionary<string, string>>>(responseJson);
                return responseObject;
            }

            throw new Exception("Find Rquest Error");
        }

        /// <summary>
        /// General purpose Find Request method. Supports additional syntaxes like the { "omit" : "true" } operation.
        /// This method returns a strongly typed <see cref="IEnumerable{T}"/> but accepts a the more flexible <see cref="Dictionary{string,string}"/> request parameters.
        /// </summary>
        /// <typeparam name="T">the type of response objects to return.</typeparam>
        /// <param name="req">The find request dictionary.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public async Task<IEnumerable<T>> FindAsync<T>(FindRequest<Dictionary<string, string>> req)
        {
            var response = await GetFindHttpResponseAsync(req);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<FindResponse<T>>(responseJson);
                return responseObject.Data.Select(d => d.FieldData);
            }

            throw new Exception("Find Rquest Error");
        }

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="req">The find request parameters.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public async Task<IEnumerable<T>> FindAsync<T>(FindRequest<T> req)
        {
            var response = await GetFindHttpResponseAsync(req);

            if (response.StatusCode == HttpStatusCode.OK)
            {
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

            // not found, so return empty list
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<T>();
            }

            // other error TODO: Improve handling
            throw new Exception("Find request error");
        }

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="input">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public Task<IEnumerable<T>> FindAsync<T>(T input) => FindAsync(GetTableName(input), input);

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="layout">The name of the layout to run this request on.</param>
        /// <param name="input">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public Task<IEnumerable<T>> FindAsync<T>(string layout, T input) => FindAsync(new FindRequest<T>() { Layout = layout, Query = new List<T>() { input } });

        #endregion

        #region Private Helpers and utility methods

        /// <summary>
        /// Utility method to handle processing of find requests.
        /// </summary>
        /// <typeparam name="T">The type parameter of the data request.</typeparam>
        /// <param name="req">The request object to send.</param>
        /// <returns>The task that will return the http response from this</returns>
        private Task<HttpResponseMessage> GetFindHttpResponseAsync<T>(FindRequest<T> req)
        {
            if (string.IsNullOrEmpty(req.Layout)) throw new ArgumentException("Layout is required on the find request.");
            if (req.Query == null || req.Query.Count() == 0) throw new ArgumentException("Query parameters are required on the find request.");

            var json = req.ToJson();
            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
            httpContent.Headers.Add("FM-Data-token", this.dataToken);
            var response = _client.PostAsync(FindEndpoint(req.Layout), httpContent);

            return response;
        }

        /// <summary>
        /// Utility method to handle processing of edit requests.
        /// </summary>
        /// <typeparam name="T">The type parameter of the data request.</typeparam>
        /// <param name="req">The request object to send.</param>
        /// <returns></returns>
        private async Task<HttpResponseMessage> GetEditHttpResponse<T>(EditRequest<T> req)
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

        /// <summary>
        /// Utility method to get the TableAttribute name to be used for the layout option in the request.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>The specified in the Table Attribute</returns>
        private string GetTableName<T>(T instance)
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
            return lay;
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Dispose resources opened for this instance of the data client.
        /// </summary>
        public void Dispose()
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