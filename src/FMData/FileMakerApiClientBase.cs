using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;

namespace FMData
{
    /// <summary>
    /// Base for implementations to inherit from.
    /// </summary>
    public abstract class FileMakerApiClientBase : IFileMakerApiClient, IDisposable
    {
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

        /// <summary>
        /// Factory for generating requests.
        /// </summary>
        /// <param name="createFactory">The Create Request Factory</param>
        ///protected FileMakerApiClientBase(Func<IFileMakerRequest> createFactory) => _createFactory = createFactory;

        /// <summary>
        /// Create a record in the database utilizing the TableAttribute to target the layout.
        /// </summary>
        /// <typeparam name="T">The type parameter to be created.</typeparam>
        /// <param name="input">Object containing the data to be on the newly created record.</param>
        /// <returns></returns>
        public virtual Task<IResponse> CreateAsync<T>(T input) where T : class, new() => CreateAsync(GetTableName(input), input);
        /// <summary>
        /// Create a record in the file, attempt to use the [TableAttribute] to determine the layout and perform a script with parameter.
        /// </summary>
        /// <typeparam name="T">The type to create</typeparam>
        /// <param name="input">The input record to create.</param>
        /// <param name="script">The name of a FileMaker script to run.</param>
        /// <param name="scriptParameter">The parameter to pass to the script.</param>
        /// <returns></returns>
        public virtual Task<IResponse> CreateAsync<T>(T input, string script, string scriptParameter) where T : class, new()
        {
            var request = _createFactory<T>();
            request.Layout = GetTableName(input);
            request.Data = input;
            request.Script = script;
            request.ScriptParameter = scriptParameter;
            return SendAsync(request);
        }
        /// <summary>
        /// Create a record in the database.
        /// </summary>
        /// <typeparam name="T">The type parameter to be created.</typeparam>
        /// <param name="layout">Layout to use (overrides any [Table] parms on the class.)</param>
        /// <param name="input">The input object containing the values for the record.</param>
        /// <returns></returns>
        public virtual Task<IResponse> CreateAsync<T>(string layout, T input) where T : class, new()
        {
            var request = _createFactory<T>();
            request.Layout = layout;
            request.Data = input;
            return SendAsync(request);
        }


        /// <summary>
        /// Find a record with utilizing a class instance to define the find request field values.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="input">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public virtual Task<IEnumerable<T>> FindAsync<T>(T input) where T : class, new() => FindAsync(GetTableName(input), input);
        /// <summary>
        /// Find a record with utilizing a class instance to define the find request field values.
        /// </summary>
        /// <typeparam name="T">The type parameter to be created.</typeparam>
        /// <param name="layout">The layout to use for the find request.</param>
        /// <param name="request">The object with the parameters to find against.</param>
        /// <returns></returns>
        public abstract Task<IEnumerable<T>> FindAsync<T>(string layout, T request) where T : class, new();
        /// <summary>
        /// Find a record with utilizing a class instance to define the find request field values.
        /// </summary>
        /// <typeparam name="T">The response type to extract and return.</typeparam>
        /// <param name="layout">The layout to perform the request on.</param>
        /// <param name="req">The dictionary of key/value pairs to find against.</param>
        /// <returns></returns>
        public abstract Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req);


        /// <summary>
        /// Edit a record by FileMaker RecordId.
        /// </summary>
        /// <typeparam name="T">The type to pull the [Table] attribute from for context layout.</typeparam>
        /// <param name="recordId">The FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object containing the values the record should reflect after the edit.</param>
        /// <returns></returns>
        public virtual Task<IResponse> EditAsync<T>(int recordId, T input) where T : class, new() => EditAsync(GetTableName(input), recordId, input);
        /// <summary>
        /// Edit a record by FileMaker RecordId.
        /// </summary>
        public abstract Task<IResponse> EditAsync<T>(string layout, int recordId, T input) where T : class, new();
        /// <summary>
        /// Edit a record by FileMaker RecordId.
        /// </summary>
        public abstract Task<IResponse> EditAsync(int recordId, string layout, Dictionary<string, string> editValues);


        /// <summary>
        /// Delete a record utilizing a generic type with the [Table] attribute specifying the layout and the FileMaker RecordId.
        /// </summary>
        /// <typeparam name="T">Class with the [Table] attribute specifying the layout to use.</typeparam>
        /// <param name="recId">The FileMaker RecordId of the record to delete.</param>
        /// <returns></returns>
        public virtual Task<IResponse> DeleteAsync<T>(int recId) where T : class, new() => DeleteAsync(recId, GetTableName(new T()));
        /// <summary>
        /// Delete a record by id and layout.
        /// </summary>
        public virtual Task<IResponse> DeleteAsync(int recId, string layout)
        {
            var request = _deleteFactory();
            request.RecordId = recId;
            request.Layout = layout;
            return SendAsync(request);
        }

        /// <summary>
        /// Send a Create Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IResponse> SendAsync<T>(ICreateRequest<T> req) where T : class, new();
        /// <summary>
        /// Send a Delete Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IResponse> SendAsync(IDeleteRequest req);
        /// <summary>
        /// Send an Edit Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IResponse> SendAsync<T>(IEditRequest<T> req) where T : class, new();
        /// <summary>
        /// Send a Find Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req);
        /// <summary>
        /// Send a Find Record request to the FileMaker API.
        /// </summary>
        public abstract Task<IEnumerable<T>> SendAsync<T>(IFindRequest<T> req) where T : class, new();

        #region Utility Methods
        /// <summary>
        /// Utility method to get the TableAttribute name to be used for the layout option in the request.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>The specified in the Table Attribute</returns>
        protected string GetTableName<T>(T instance)
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