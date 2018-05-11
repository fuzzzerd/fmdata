using System.Collections.Generic;
using System.Threading.Tasks;

namespace FMData
{
    /// <summary>
    /// FileMaker API Client
    /// </summary>
    public interface IFileMakerApiClient
    {
        /// <summary>
        /// Create a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns>The newly created RecordId and/or an error response code.</returns>
        Task<IResponse> CreateAsync<T>(T input) where T : class, new();

        /// <summary>
        /// Create a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="layout"></param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns>The newly created RecordId and/or an error response code.</returns>
        Task<IResponse> CreateAsync<T>(string layout, T input) where T : class, new();

        /// <summary>
        /// Creates a new record.
        /// </summary>
        /// <param name="req">New record request.</param>
        Task<IResponse> CreateAsync<T>(ICreateRequest<T> req) where T : class, new();

        /// <summary>
        /// Find a record or records matching the request.
        /// </summary>
        /// <param name="req">Find request.</param>
        Task<IFindResponse<Dictionary<string,string>>> FindAsync(IFindRequest<Dictionary<string,string>> req);

        /// <summary>
        /// Find a record with a custom dictionary of request parameters.
        /// </summary>
        /// <typeparam name="T">The type of object to project and return.</typeparam>
        /// <param name="req">The request parameters to send to FileMaker Server.</param>
        /// <returns>An IEnumerable of type T.</returns>
        Task<IEnumerable<T>> FindAsync<T>(IFindRequest<Dictionary<string, string>> req) where T : class, new();

        /// <summary>
        /// Find a record or records matching the request.
        /// </summary>
        /// <param name="req">Find request</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(IFindRequest<T> req) where T : class, new();

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(T request) where T : class, new();
        
        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="layout">Overrides the TableAttribute for the name of the layout to run this request on.</param>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(string layout, T request) where T : class, new();

        /// <summary>
        /// Edit record.
        /// </summary>
        /// <param name="req">Edit record request.</param>
        Task<IResponse> EditAsync(IEditRequest<Dictionary<string, string>> req);

        /// <summary>
        /// Edit record.
        /// </summary>
        /// <param name="req">Edit record request.</param>
        Task<IResponse> EditAsync<T>(IEditRequest<T> req) where T : class, new();

        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        Task<IResponse> EditAsync<T>(int recordId, T input) where T : class, new();

        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="layout"></param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        Task<IResponse> EditAsync<T>(string layout, int recordId, T input) where T : class, new();

        /// <summary>
        /// Delete record
        /// </summary>
        /// <param name="req">Delete record request.</param>
        Task<IResponse> DeleteAsync(IDeleteRequest req);

        /// <summary>
        /// Delete a record
        /// </summary>
        /// <param name="recId">The filemaker RecordId to delete.</param>
        /// <param name="delete">Used to pull the [TableAttribute] value to determine the layout to use.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IResponse> DeleteAsync<T>(int recId, T delete);

        /// <summary>
        /// Delete a record.
        /// </summary>
        /// <param name="recId">The FileMaker RecordId to delete.</param>
        /// <param name="layout">The layout to use for the delete.</param>
        /// <returns></returns>
        Task<IResponse> DeleteAsync(int recId, string layout);
        
    }
}