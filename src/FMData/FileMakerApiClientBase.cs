using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Threading.Tasks;

namespace FMData
{
    public abstract class FileMakerApiClientBase : IFileMakerApiClient, IDisposable
    {
        /// <summary>
        /// Create a record in the database utilizing the TableAttribute to target the layout.
        /// </summary>
        /// <typeparam name="T">The type parameter to be created.</typeparam>
        /// <param name="input">Object containing the data to be on the newly created record.</param>
        /// <returns></returns>
        public virtual Task<IResponse> CreateAsync<T>(T input) where T : class, new() => CreateAsync(GetTableName(input), input);
        public abstract Task<IResponse> CreateAsync<T>(string layout, T input) where T : class, new();


        /// <summary>
        /// Find a record with utilizing a class instance define the find request field values.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="input">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public virtual Task<IEnumerable<T>> FindAsync<T>(T input) where T : class, new() => FindAsync(GetTableName(input), input);
        public abstract Task<IEnumerable<T>> FindAsync<T>(string layout, T request) where T : class, new();
        public abstract Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req);


        /// <summary>
        /// Edit a record by FileMaker RecordId.
        /// </summary>
        /// <typeparam name="T">The type to pull the [Table] attribute from for context layout.</typeparam>
        /// <param name="recordId">The FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object containing the values the record should reflect after the edit.</param>
        /// <returns></returns>
        public virtual Task<IResponse> EditAsync<T>(int recordId, T input) where T : class, new() => EditAsync(GetTableName(input), recordId, input);
        public abstract Task<IResponse> EditAsync<T>(string layout, int recordId, T input) where T : class, new();
        public abstract Task<IResponse> EditAsync(int recordId, string layout, Dictionary<string, string> editValues);


        /// <summary>
        /// Delete a record utilizing a generic type with the [Table] attribute specifying the layout and the FileMaker RecordId.
        /// </summary>
        /// <typeparam name="T">Class with the [Table] attribute specifying the layout to use.</typeparam>
        /// <param name="recId">The FileMaker RecordId of the record to delete.</param>
        /// <returns></returns>
        public virtual Task<IResponse> DeleteAsync<T>(int recId) where T : class, new() => DeleteAsync(recId, GetTableName(new T()));
        public abstract Task<IResponse> DeleteAsync(int recId, string layout);


        public abstract Task<IResponse> SendAsync<T>(ICreateRequest<T> req) where T : class, new();
        public abstract Task<IResponse> SendAsync(IDeleteRequest req);
        public abstract Task<IResponse> SendAsync<T>(IEditRequest<T> req) where T : class, new();
        public abstract Task<IFindResponse<Dictionary<string, string>>> SendAsync(IFindRequest<Dictionary<string, string>> req);
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

        public abstract void Dispose();
    }
}