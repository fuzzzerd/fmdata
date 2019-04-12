using FMData.Rest.Responses;
using System.Net.Http;
using System.Threading.Tasks;

namespace FMData.Rest
{
    /// <summary>
    /// FileMaker REST Client methods. Mostly for internal use.
    /// </summary>
    public interface IFileMakerRestClient : IFileMakerApiClient
    {
        /// <summary>
        /// Utility method for exposing the raw request data from FMS.
        /// </summary>
        /// <param name="method">The http method to execute for this request (GET, POST, PATCH, DELETE).</param>
        /// <param name="requestUri">The Endpoint to execute the request at.</param>
        /// <param name="req">The request object to send to FMS.</param>
        /// <returns>An HttpResponseMessage from executing the request.</returns>
        Task<HttpResponseMessage> ExecuteRequestAsync(
            HttpMethod method,
            string requestUri,
            IFileMakerRequest req);

        /// <summary>
        /// /// Helper For Getting Raw Responses from Data API.
        /// </summary>
        Task<HttpResponseMessage> ExecuteRequestAsync<T>(ICreateRequest<T> req);
        
        /// <summary>
        /// Helper For Getting Raw Responses from Data API.
        /// </summary>
        Task<HttpResponseMessage> ExecuteRequestAsync<T>(IEditRequest<T> req);
        
        /// <summary>
        /// Helper For Getting Raw Responses from Data API.
        /// </summary>
        Task<HttpResponseMessage> ExecuteRequestAsync<T>(IFindRequest<T> req);

        /// <summary>
        /// Helper For Getting Raw Responses from Data API.
        /// </summary>
        Task<HttpResponseMessage> ExecuteRequestAsync(IDeleteRequest req);
    }
}