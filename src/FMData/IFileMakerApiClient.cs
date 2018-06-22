using System;
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
        Task<ICreateResponse> CreateAsync<T>(T input) where T : class, new();

        /// <summary>
        /// <see cref="FileMakerApiClientBase.CreateAsync{T}(T, string, string)"/>
        /// </summary>
        Task<ICreateResponse> CreateAsync<T>(T input, string script, string scriptParameter) where T : class, new();

        /// <summary>
        /// <see cref="FileMakerApiClientBase.CreateAsync{T}(T, string, string, string, string, string, string)"/>
        /// </summary>
        Task<ICreateResponse> CreateAsync<T>(T input, string script, string scriptParameter, string preRequestScript, string preRequestScriptParam, string preSortScript, string preSortScriptParameter) where T : class, new();

        /// <summary>
        /// Create a record in the file via explicit layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="layout">The layout to use for the context of the request.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns>The newly created RecordId and/or an error response code.</returns>
        Task<ICreateResponse> CreateAsync<T>(string layout, T input) where T : class, new();



        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(T request) where T : class, new();

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="fmid">Function to map the FileMaker RecordId to each instance T.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(T request, Func<T,int,object> fmid) where T : class, new();

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="script">Script to run after the request is completed.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="fmid">Function to map the FileMaker RecordId to each instance T.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(T request, string script, string scriptParameter, Func<T, int, object> fmid) where T : class, new();

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="layout">Overrides the TableAttribute for the name of the layout to run this request on.</param>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(string layout, T request) where T : class, new();

        /// <summary>
        /// Finds a record or records matching the properties of the input request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="layout"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req);


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
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="script">script to run after the request.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        Task<IResponse> EditAsync<T>(int recordId, string script, string scriptParameter, T input) where T : class, new();


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
        /// Edit a record based on RecordId, layout, and a collection of key/value pairs for the fields to be updated.
        /// </summary>
        /// <param name="recordId">FileMaker RecordId</param>
        /// <param name="layout">The layout to use for context.</param>
        /// <param name="editValues">The field and value pairs to send for edit.</param>
        /// <returns></returns>
        Task<IResponse> EditAsync(int recordId, string layout, Dictionary<string, string> editValues);


        /// <summary>
        /// Delete a record by FileMaker RecordId. 
        /// </summary>
        /// <param name="recId">The filemaker RecordId to delete.</param>
        /// <typeparam name="T">Used to pull the [TableAttribute] value to determine the layout to use.</typeparam>
        /// <returns></returns>
        /// <remarks>Use the other delete overload if the class does not use the [Table] attribute.</remarks>
        Task<IResponse> DeleteAsync<T>(int recId) where T : class, new();

        /// <summary>
        /// Delete a record.
        /// </summary>
        /// <param name="recId">The FileMaker RecordId to delete.</param>
        /// <param name="layout">The layout to use for the delete.</param>
        /// <returns></returns>
        Task<IResponse> DeleteAsync(int recId, string layout);



        #region Set Globals
        /// <summary>
        /// Set the value of global fields.
        /// // https://fmhelp.filemaker.com/docs/17/en/dataapi/#set-global-fields
        /// </summary>
        /// <param name="baseTable">The base table on which this global field is defined.</param>
        /// <param name="fieldName">The name of the global field to set.</param>
        /// <param name="targetValue">The target value for this global field.</param>
        /// <returns>FileMaker Response</returns>
        Task<IResponse> SetGlobalFieldAsync(string baseTable, string fieldName, string targetValue);
        #endregion

        #region Set Containers
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
        Task<IResponse> UpdateContainer(
            string layout,
            int recordId,
            string fieldName,
            string fileName,
            int repetition,
            byte[] content);

        /// <summary>
        /// Puts the contents of the byte array into the specified container field.
        /// </summary>
        /// <param name="layout">The layout to perform this operation on.</param>
        /// <param name="recordId">The FileMaker RecordID of the record we want to update the container on.</param>
        /// <param name="fieldName">Name of the Container Field.</param>
        /// <param name="fileName">The name of the file being inserted into the container field.</param>
        /// <param name="content">The content to be inserted into the container field.</param>
        /// <returns>The FileMaker Server Response from this operation.</returns>
        Task<IResponse> UpdateContainerAsync(
            string layout,
            int recordId,
            string fieldName,
            string fileName,
            byte[] content);
        #endregion

        #region Send Request Methods
        /// <summary>
        /// Creates a new record.
        /// </summary>
        /// <param name="req">New record request.</param>
        Task<ICreateResponse> SendAsync<T>(ICreateRequest<T> req) where T : class, new();

        /// <summary>
        /// Find a record or records matching the request.
        /// </summary>
        /// <param name="req">Find request.</param>
        Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req);

        /// <summary>
        /// Find a record or records matching the request.
        /// </summary>
        /// <param name="req">Find request.</param>
        /// <param name="fmId">Function to map the FileMaker Id to the model.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> SendAsync<T>(IFindRequest<T> req, Func<T, int, object> fmId = null) where T : class, new();

        /// <summary>
        /// Edit record.
        /// </summary>
        /// <param name="req">Edit record request.</param>
        Task<IResponse> SendAsync<T>(IEditRequest<T> req) where T : class, new();
        
        /// <summary>
        /// Delete record
        /// </summary>
        /// <param name="req">Delete record request.</param>
        Task<IResponse> SendAsync(IDeleteRequest req); 
        #endregion
    }
}