using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FMData
{
    public abstract partial class FileMakerApiClientBase
    {
        /// <summary>
        /// Edit a record by FileMaker RecordId.
        /// </summary>
        /// <typeparam name="T">The type to pull the [Table] attribute from for context layout.</typeparam>
        /// <param name="recordId">The FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object containing the values the record should reflect after the edit.</param>
        /// <returns></returns>
        public virtual Task<IEditResponse> EditAsync<T>(int recordId, T input) where T : class, new() => EditAsync(GetTableName(input), recordId, input);

        /// <summary>
        /// Edit a record in the file, attempt to use the [TableAttribute] to determine the layout.
        /// </summary>
        /// <typeparam name="T">Properties of this generic type should match fields on target layout.</typeparam>
        /// <param name="recordId">The internal FileMaker RecordId of the record to edit.</param>
        /// <param name="script">script to run after the request.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="input">The object containing the data to be sent across the wire to FileMaker.</param>
        /// <returns></returns>
        public virtual Task<IEditResponse> EditAsync<T>(int recordId, string script, string scriptParameter, T input) where T : class, new()
        {
            var request = _editFactory<T>();

            if (!string.IsNullOrEmpty(script))
            {
                request.Script = script;
                request.ScriptParameter = scriptParameter;
            }

            request.Layout = GetTableName(input);
            request.RecordId = recordId.ToString();
            request.Data = input;
            return SendAsync(request);
        }

        /// <summary>
        /// Edit a record.
        /// </summary>
        /// <typeparam name="T">Type parameter for this edit.</typeparam>
        /// <param name="layout">Explicitly define the layout to use.</param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to be edited.</param>
        /// <param name="input">Object with the updated values.</param>
        /// <returns></returns>
        public virtual Task<IEditResponse> EditAsync<T>(string layout, int recordId, T input) where T : class, new()
        {
            var request = _editFactory<T>();
            request.Layout = layout;
            request.RecordId = recordId.ToString();
            request.Data = input;
            return SendAsync(request);
        }

        /// <summary>
        /// Edit a record.
        /// </summary>
        /// <param name="layout">Explicitly define the layout to use.</param>
        /// <param name="recordId">The internal FileMaker RecordId of the record to be edited.</param>
        /// <param name="editValues">Object with the updated values.</param>
        /// <returns></returns>
        public virtual Task<IEditResponse> EditAsync(int recordId, string layout, Dictionary<string, string> editValues)
        {
            var req = _editFactory<Dictionary<string, string>>();
            req.Data = editValues;
            req.Layout = layout;
            req.RecordId = recordId.ToString();
            return SendAsync(req);
        }
    }
}