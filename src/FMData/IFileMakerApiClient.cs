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
        #region Request Factory Abstracts
        /// <summary>
        /// Get a new Create Request of the correct type.
        /// </summary>
        ICreateRequest<T> GenerateCreateRequest<T>();
        /// <summary>
        /// Get a new Edit Request of the correct type.
        /// </summary>
        IEditRequest<T> GenerateEditRequest<T>();
        /// <summary>
        /// Get a new Find Request of the correct type.
        /// </summary>
        IFindRequest<T> GenerateFindRequest<T>();
        /// <summary>
        /// Get a new Delete Request of the correct type.
        /// </summary>
        IDeleteRequest GenerateDeleteRequest();
        #endregion

        #region Get
        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        Task<T> GetByFileMakerIdAsync<T>(int fileMakerId, Func<T, int, object> fmId = null) where T : class, new();

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <param name="fmMod">The funtion to use to map the FileMaker ModId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        Task<T> GetByFileMakerIdAsync<T>(int fileMakerId, Func<T, int, object> fmId = null, Func<T, int, object> fmMod = null) where T : class, new();

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="layout">The layout to execute the request against.</param>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <param name="fmMod">The funtion to use to map the FileMaker ModId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        Task<T> GetByFileMakerIdAsync<T>(string layout, int fileMakerId, Func<T, int, object> fmId = null, Func<T, int, object> fmMod = null) where T : class, new();

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="layout">The layout to execute the request against.</param>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        Task<T> GetByFileMakerIdAsync<T>(string layout, int fileMakerId, Func<T, int, object> fmId = null) where T : class, new();
        #endregion

        #region Find
        /// <summary>
        /// Finds a record or records matching the properties of the input request.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="layout"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req);
        #endregion

        #region Edit
        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        Task<IEditResponse> EditAsync<T>(int recordId, T input) where T : class, new();

        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="script">script to run after the request.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        Task<IEditResponse> EditAsync<T>(int recordId, string script, string scriptParameter, T input) where T : class, new();

        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="layout"></param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        Task<IEditResponse> EditAsync<T>(string layout, int recordId, T input) where T : class, new();

        /// <summary>
        /// Edit a record based on RecordId, layout, and a collection of key/value pairs for the fields to be updated.
        /// </summary>
        /// <param name="recordId">FileMaker RecordId</param>
        /// <param name="layout">The layout to use for context.</param>
        /// <param name="editValues">The field and value pairs to send for edit.</param>
        /// <returns></returns>
        Task<IEditResponse> EditAsync(int recordId, string layout, Dictionary<string, string> editValues);
        #endregion

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
        Task<IEditResponse> UpdateContainerAsync(
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
        Task<IEditResponse> UpdateContainerAsync(
            string layout,
            int recordId,
            string fieldName,
            string fileName,
            byte[] content);
        #endregion

        /// <summary>
        /// Load the contents of the container data into the attributed property of the model.
        /// </summary>
        /// <typeparam name="T">The type of object to populate.</typeparam>
        /// <param name="instance">Instance of the object that has container data with the ContainerDataForAttribute.</param>
        Task ProcessContainer<T>(T instance);

        /// <summary>
        /// Load the contents of the container data into the attributed property of the models.
        /// </summary>
        /// <typeparam name="T">The type of object to populate.</typeparam>
        /// <param name="instances">Collection of objects that have container data with the ContainerDataForAttribute.</param>
        Task ProcessContainers<T>(IEnumerable<T> instances);

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
        /// <param name="modId">Function to map the FileMaker Mod Id to the modle.</param>
        /// <returns></returns>
        Task<IEnumerable<T>> SendAsync<T>(
            IFindRequest<T> req, 
            Func<T, int, object> fmId = null, 
            Func<T, int, object> modId = null) where T : class, new();

        /// <summary>
        /// Edit record.
        /// </summary>
        /// <param name="req">Edit record request.</param>
        Task<IEditResponse> SendAsync<T>(IEditRequest<T> req) where T : class, new();

        /// <summary>
        /// Delete record
        /// </summary>
        /// <param name="req">Delete record request.</param>
        Task<IResponse> SendAsync(IDeleteRequest req); 
        #endregion
    }
}