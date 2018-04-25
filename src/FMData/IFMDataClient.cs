using System.Collections.Generic;
using System.Threading.Tasks;
using FMData.Requests;
using FMData.Responses;

namespace FMData
{
    /// <summary>
    /// FileMaker Data API Client
    /// </summary>
    public interface IFMDataClient
    {
        #region Auth/Data Token Management
        /// <summary>
        /// Refreshes the internally stored authentication token from filemaker server.
        /// </summary>
        /// <param name="username">Username of the account to authenticate.</param>
        /// <param name="password">Password of the account to authenticate.</param>
        /// <param name="layout">The layout to authenticate against.</param>
        /// <returns>An AuthResponse from deserialized from FileMaker Server json response.</returns>
        Task<AuthResponse> RefreshTokenAsync(
            string username,
            string password,
            string layout);

        /// <summary>
        /// Logs the user out and nullifies the token.
        /// </summary>
        /// <returns>FileMaker Response</returns>
        Task<BaseDataResponse> LogoutAsync();
        #endregion

        #region API Endpoint Functions
        /// <summary>
        /// Generate the appropriate Authentication endpoint uri for this instance of the data client.
        /// </summary>
        /// <returns>The FileMaker Data API Endpoint for Authentication Requests.</returns>
        string AuthEndpoint();

        /// <summary>
        /// Generate the appropriate Find endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <returns>The FileMaker Data API Endpoint for Find requests.</returns>
        string FindEndpoint(string layout);

        /// <summary>
        /// Generate the appropriate Create endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <returns>The FileMaker Data API Endpoint for Create requests.</returns>
        string CreateEndpoint(string layout);

        /// <summary>
        /// Generate the appropriate Edit/Update endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <param name="recordid">The record ID of the record to edit.</param>
        /// <returns>The FileMaker Data API Endpoint for Update/Edit requests.</returns>
        string UpdateEndpoint(string layout, object recordid);

        /// <summary>
        /// Generate the appropriate Delete endpoint uri for this instance of the data client.
        /// </summary>
        /// <param name="layout">The name of the layout to use as the context for creating the record.</param>
        /// <param name="recordid">The record ID of the record to edit.</param>
        /// <returns>The FileMaker Data API Endpoint for Delete requests.</returns>
        string DeleteEndpoint(string layout, object recordid);
        #endregion

        /// <summary>
        /// Indicates if the client is authenticated or not.
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// Create a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns>The newly created RecordId and/or an error response code.</returns>
        Task<BaseDataResponse> CreateAsync<T>(T input);

        /// <summary>
        /// Create a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="layout"></param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns>The newly created RecordId and/or an error response code.</returns>
        Task<BaseDataResponse> CreateAsync<T>(string layout, T input);

        /// <summary>
        /// Creates a new record.
        /// </summary>
        /// <param name="req">New record request.</param>
        Task<BaseDataResponse> CreateAsync<T>(CreateRequest<T> req);

        /// <summary>
        /// Find a record or records matching the request.
        /// </summary>
        /// <param name="req">Find request.</param>
        Task<FindResponse<Dictionary<string,string>>> FindAsync(FindRequest<Dictionary<string,string>> req);

        /// <summary>
        /// Find a record with a custom dictionary of request parameters.
        /// </summary>
        /// <typeparam name="T">The type of object to project and return.</typeparam>
        /// <param name="req">The request parameters to send to FileMaker Server.</param>
        /// <returns>An IEnumerable of type T.</returns>
        Task<IEnumerable<T>> FindAsync<T>(FindRequest<Dictionary<string, string>> req);

        /// <summary>
        /// Find a record or records matching the request.
        /// </summary>
        /// <param name="req">Find request</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(FindRequest<T> req);

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(T request);
        
        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="layout">Overrides the TableAttribute for the name of the layout to run this request on.</param>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(string layout, T request);

        /// <summary>
        /// Edit record.
        /// </summary>
        /// <param name="req">Edit record request.</param>
        Task<BaseDataResponse> EditAsync(EditRequest<Dictionary<string, string>> req);

        /// <summary>
        /// Edit record.
        /// </summary>
        /// <param name="req">Edit record request.</param>
        Task<BaseDataResponse> EditAsync<T>(EditRequest<T> req);

        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        Task<BaseDataResponse> EditAsync<T>(int recordId, T input);

        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="layout"></param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        Task<BaseDataResponse> EditAsync<T>(string layout, int recordId, T input);

        /// <summary>
        /// Delete record
        /// </summary>
        /// <param name="req">Delete record request.</param>
        Task<BaseDataResponse> DeleteAsync(DeleteRequest req);
    }
}