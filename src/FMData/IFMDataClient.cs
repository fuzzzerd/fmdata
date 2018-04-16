using System.Collections.Generic;
using System.Threading.Tasks;
using FMData.Requests;
using FMData.Responses;

namespace FMData
{
    /// <summary>
    /// FileMaker Data API Client
    /// </summary>
    interface IFMDataClient
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
        /// Create a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns>The newly created RecordId and/or an error response code.</returns>
        Task<BaseDataResponse> Create<T>(T input);

        /// <summary>
        /// Create a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="layout"></param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns>The newly created RecordId and/or an error response code.</returns>
        Task<BaseDataResponse> Create<T>(string layout, T input);


        /// <summary>
        /// Creates a new record.
        /// </summary>
        /// <param name="req">New record request.</param>
        Task<BaseDataResponse> ExecuteCreate<T>(CreateRequest<T> req);

        /// <summary>
        /// Find a records.
        /// </summary>
        /// <param name="req">Find request.</param
        Task<FindResponse> ExecuteFind(FindRequest req);

        /// <summary>
        /// Edit record.
        /// </summary>
        /// <param name="req">Edit record request.</param>
        Task<BaseDataResponse> ExecuteEdit(EditRequest req);

        /// <summary>
        /// Delete record
        /// </summary>
        /// <param name="req">Delete record request.</param>
        Task<BaseDataResponse> ExecuteDelete(DeleteRequest req);
    }
}