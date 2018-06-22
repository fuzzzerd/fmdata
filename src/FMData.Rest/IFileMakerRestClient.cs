using FMData.Rest.Responses;
using System.Threading.Tasks;

namespace FMData.Rest
{
    /// <summary>
    /// FileMaker REST Client methods. Mostly for internal use.
    /// </summary>
    public interface IFileMakerRestClient : IFileMakerApiClient
    {
        #region Auth/Data Token Management
        /// <summary>
        /// Refreshes the internally stored authentication token from filemaker server.
        /// </summary>
        /// <param name="username">Username of the account to authenticate.</param>
        /// <param name="password">Password of the account to authenticate.</param>
        /// <returns>An AuthResponse from deserialized from FileMaker Server json response.</returns>
        Task<AuthResponse> RefreshTokenAsync(
            string username,
            string password);

        /// <summary>
        /// Logs the user out and nullifies the token.
        /// </summary>
        /// <returns>FileMaker Response</returns>
        Task<IResponse> LogoutAsync();
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
        /// Generate the appropriate Get Records endpoint.
        /// </summary>
        /// <param name="layout">The layout to use as the context for the response.</param>
        /// <param name="recordId">The FileMaker record Id for this request.</param>
        /// <returns>The FileMaker Data API Endpoint for Get Records reqeusts.</returns>
        string GetRecordEndpoint(string layout, int recordId);

        /// <summary>
        /// Generate the appropriate Get Records endpoint.
        /// </summary>
        /// <param name="layout">The layout to use as the context for the response.</param>
        /// <param name="range">The number of records to return.</param>
        /// <param name="offset">The offset number of records to skip before starting to return records.</param>
        /// <returns>The FileMaker Data API Endpoint for Get Records reqeusts.</returns>
        string GetRecordsEndpoint(string layout, int range, int offset);

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

        /// <summary>
        /// Generate the appropriate Container field endpoint.
        /// </summary>
        /// <param name="layout">The layout to use.</param>
        /// <param name="recordid">the record ID of the record to edit.</param>
        /// <param name="fieldName">The name of the container field.</param>
        /// <param name="repetitionNumber">Field repetition number; default value is 1 (one).</param>
        /// <returns></returns>
        string ContainerEndpoint(string layout, object recordid, string fieldName, int repetitionNumber = 1);
        #endregion

        /// <summary>
        /// Indicates if the client is authenticated or not.
        /// </summary>
        bool IsAuthenticated { get; }
    }
}