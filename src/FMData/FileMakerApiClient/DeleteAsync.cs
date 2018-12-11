using System;
using System.Threading.Tasks;

namespace FMData
{
    public abstract partial class FileMakerApiClientBase
    {
        /// <summary>
        /// Delete a record utilizing a generic type with the [Table] attribute specifying the layout and the FileMaker RecordId.
        /// </summary>
        /// <typeparam name="T">Class with the [Table] attribute specifying the layout to use.</typeparam>
        /// <param name="recId">The FileMaker RecordId of the record to delete.</param>
        /// <returns></returns>
        public virtual Task<IResponse> DeleteAsync<T>(int recId) where T : class, new() => DeleteAsync(recId, GetLayoutName(new T()));
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
    }
}