using System;
using System.Threading.Tasks;

namespace FMData
{
    /// <summary>
    /// Delete Helper Extensions
    /// </summary>
    public static class DeleteExtensions
    {
        /// <summary>
        /// Delete a record utilizing a generic type with the [Table] attribute specifying the layout and the FileMaker RecordId.
        /// </summary>
        /// <typeparam name="T">Class with the [Table] attribute specifying the layout to use.</typeparam>
        /// <param name="client">The FileMaker API client instance.</param>
        /// <param name="recId">The FileMaker RecordId of the record to delete.</param>
        /// <returns></returns>
        public static Task<IResponse> DeleteAsync<T>(this IFileMakerApiClient client, int recId) where T : class, new()
        {
            var request = client.GenerateDeleteRequest();
            request.Layout = FileMakerApiClientBase.GetLayoutName(new T());
            request.RecordId = recId;
            return client.SendAsync(request);
        }

        /// <summary>
        /// Delete a record by id and layout.
        /// </summary>
        public static Task<IResponse> DeleteAsync(
            this IFileMakerApiClient client,
            int recId,
            string layout)
        {
            var request = client.GenerateDeleteRequest();
            request.Layout = layout;
            request.RecordId = recId;
            return client.SendAsync(request);
        }
    }
}