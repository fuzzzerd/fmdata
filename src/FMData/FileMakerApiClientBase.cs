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
        public abstract Task<IResponse> CreateAsync<T>(ICreateRequest<T> req) where T : class, new();

        public virtual Task<IResponse> DeleteAsync<T>(int recId, T delete) => DeleteAsync(recId, GetTableName(delete));
        public abstract Task<IResponse> DeleteAsync(int recId, string layout);
        public abstract Task<IResponse> DeleteAsync(IDeleteRequest req);
        public abstract void Dispose();
        public virtual Task<IResponse> EditAsync<T>(int recordId, T input) where T : class, new() => EditAsync(GetTableName(input), recordId, input);
        public abstract Task<IResponse> EditAsync<T>(string layout, int recordId, T input) where T : class, new();
        public abstract Task<IResponse> EditAsync(IEditRequest<Dictionary<string, string>> req);
        public abstract Task<IResponse> EditAsync<T>(IEditRequest<T> req) where T : class, new();
        

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="input">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public virtual Task<IEnumerable<T>> FindAsync<T>(T input) where T : class, new() => FindAsync(GetTableName(input), input);
        public abstract Task<IEnumerable<T>> FindAsync<T>(string layout, T request) where T : class, new();
        public abstract Task<IFindResponse<Dictionary<string, string>>> FindAsync(IFindRequest<Dictionary<string, string>> req);
        public abstract Task<IEnumerable<T>> FindAsync<T>(IFindRequest<Dictionary<string, string>> req) where T : class, new();
        public abstract Task<IEnumerable<T>> FindAsync<T>(IFindRequest<T> req) where T : class, new();

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
    }
}