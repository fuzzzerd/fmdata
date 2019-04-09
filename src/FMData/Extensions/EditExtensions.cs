using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FMData
{
    /// <summary>
    /// Edit Helper Extensions
    /// </summary>
    public static class EditExtensions
    {
        /// <summary>
        /// Edit a record by FileMaker RecordId.
        /// </summary>
        /// <typeparam name="T">The type to pull the [Table] attribute from for context layout.</typeparam>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="recordId">The FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object containing the values the record should reflect after the edit.</param>
        /// <returns></returns>
        public static Task<IEditResponse> EditAsync<T>(
            this IFileMakerApiClient client,
            int recordId,
            T input) where T : class, new()
        {
            var request = client.GenerateEditRequest(input);
            request.RecordId = recordId;
            return client.SendAsync(request);
        }

        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="script">script to run after the request.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        public static Task<IEditResponse> EditAsync<T>(
            this IFileMakerApiClient client,
            int recordId,
            string script,
            string scriptParameter,
            T input) where T : class, new()
        {
            var request = client.GenerateEditRequest(input);
            request.RecordId = recordId;

            if (!string.IsNullOrEmpty(script))
            {
                request.Script = script;
                request.ScriptParameter = scriptParameter;
            }

            return client.SendAsync(request);
        }

        /// <summary>
        /// Edit a record.
        /// </summary>
        /// <typeparam name="T">Type parameter for this edit.</typeparam>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="layout">Explicitly define the layout to use.</param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object with the updated values.</param>
        /// <returns></returns>
        public static Task<IEditResponse> EditAsync<T>(
            this IFileMakerApiClient client,
            string layout,
            int recordId,
            T input) where T : class, new()
        {
            var request = client.GenerateEditRequest<T>();
            request.Data = input;
            request.Layout = layout;
            request.RecordId = recordId;
            return client.SendAsync(request);
        }

        /// <summary>
        /// Edit a record.
        /// </summary>
        /// <param name="layout">Explicitly define the layout to use.</param>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to be edited.</param>
        /// <param name="editValues">Object with the updated values.</param>
        /// <returns></returns>
        public static Task<IEditResponse> EditAsync(
            this IFileMakerApiClient client, 
            int recordId, 
            string layout, 
            Dictionary<string, string> editValues)
        {
            var request = client.GenerateEditRequest<Dictionary<string, string>>();
            request.Data = editValues;
            request.Layout = layout;
            request.RecordId = recordId;
            return client.SendAsync(request);
        }
    }
}