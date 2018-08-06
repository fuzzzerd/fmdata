using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;

namespace FMData
{
    /// <summary>
    /// Base for implementations to inherit from.
    /// Provides some wrapper and passthrough functionality to expand the API surface without requiring each fo those methods be re-implemented.
    /// </summary>
    public abstract partial class FileMakerApiClientBase : IFileMakerApiClient, IDisposable
    {
        #region Request Factory Abstracts
        /// <summary>
        /// Factory to get a new Create Request of the correct type.
        /// </summary>
        protected abstract ICreateRequest<T> _createFactory<T>();
        /// <summary>
        /// Factory to get a new Edit Request of the correct type.
        /// </summary>
        protected abstract IEditRequest<T> _editFactory<T>();
        /// <summary>
        /// Factory to get a new Find Request of the correct type.
        /// </summary>
        protected abstract IFindRequest<T> _findFactory<T>();
        /// <summary>
        /// Factory to get a new Delete Request of the correct type.
        /// </summary>
        protected abstract IDeleteRequest _deleteFactory(); 
        #endregion


        /// <summary>
        /// Send a Create Record request to the FileMaker API.
        /// </summary>
        public abstract Task<ICreateResponse> SendAsync<T>(ICreateRequest<T> req) where T : class, new();

        /// <summary>
        /// Send a Delete Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IResponse> SendAsync(IDeleteRequest req);

        /// <summary>
        /// Send an Edit Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IEditResponse> SendAsync<T>(IEditRequest<T> req) where T : class, new();

        /// <summary>
        /// Send a Find Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req);

        /// <summary>
        /// Send a Find Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IEnumerable<T>> SendAsync<T>(
            IFindRequest<T> req,
            Func<T, int, object> fmId = null,
            Func<T, int, object> modId = null) where T : class, new();



        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        public virtual Task<T> GetByFileMakerIdAsync<T>(int fileMakerId, Func<T, int, object> fmId = null) where T : class, new()
        {
            var layout = GetTableName(new T()); // probably a better way
            return GetByFileMakerIdAsync(layout, fileMakerId, fmId);
        }

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <param name="fmMod">The funtion to use to map the FileMaker ModId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        public virtual Task<T> GetByFileMakerIdAsync<T>(int fileMakerId, Func<T, int, object> fmId = null, Func<T, int, object> fmMod = null) where T : class, new()
        {
            var layout = GetTableName(new T()); // probably a beter way
            return GetByFileMakerIdAsync(layout, fileMakerId, fmId, fmMod);
        }

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="layout">the layout to execute the request on.</param>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        public virtual Task<T> GetByFileMakerIdAsync<T>(string layout, int fileMakerId, Func<T, int, object> fmId = null) where T : class, new()
        {
            return GetByFileMakerIdAsync(layout, fileMakerId, fmId, null);
        }

        /// <summary>
        /// Get a single record by FileMaker RecordId
        /// </summary>
        /// <typeparam name="T">The type to load the data into.</typeparam>
        /// <param name="layout">The layout to execute the request against.</param>
        /// <param name="fileMakerId">The FileMaker RecordId of the record to load.</param>
        /// <param name="fmId">The function to use to map the FileMakerId to the return object.</param>
        /// <returns>A single record matching the FileMaker Record Id.</returns>
        public abstract Task<T> GetByFileMakerIdAsync<T>(string layout, int fileMakerId, Func<T, int, object> fmId = null, Func<T, int, object> fmMod = null) where T : class, new();

        /// <summary>
        /// Set the value of global fields.
        /// // https://fmhelp.filemaker.com/docs/17/en/dataapi/#set-global-fields
        /// </summary>
        /// <param name="baseTable">The base table on which this global field is defined.</param>
        /// <param name="fieldName">The name of the global field to set.</param>
        /// <param name="targetValue">The target value for this global field.</param>
        /// <returns>FileMaker Response</returns>
        public abstract Task<IResponse> SetGlobalFieldAsync(string baseTable, string fieldName, string targetValue);


        #region Container Uploads
        /// <summary>
        /// Puts the contents of the byte array into the specified container field.
        /// </summary>
        /// <param name="layout">The layout to perform this operation on.</param>
        /// <param name="recordId">The FileMaker RecordID of the record we want to update the container on.</param>
        /// <param name="fieldName">Name of the Container Field.</param>
        /// <param name="fileName">The name of the file being inserted into the container field.</param>
        /// <param name="content">The content to be inserted into the container field.</param>
        /// <returns>The FileMaker Server Response from this operation.</returns>
        public virtual Task<IEditResponse> UpdateContainerAsync(
            string layout,
            int recordId,
            string fieldName,
            string fileName,
            byte[] content) => UpdateContainerAsync(layout, recordId, fieldName, fileName, 1, content); // default repetition of 1.

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
        public abstract Task<IEditResponse> UpdateContainerAsync(
            string layout,
            int recordId,
            string fieldName,
            string fileName,
            int repetition,
            byte[] content); 
        #endregion

        #region Utility Methods
        /// <summary>
        /// Utility method to get the TableAttribute name to be used for the layout option in the request.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>The specified in the Table Attribute</returns>
        public static string GetTableName<T>(T instance)
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

        /// <summary>
        /// IDisposable
        /// </summary>
        public abstract void Dispose();
    }
}