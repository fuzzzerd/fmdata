using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FMData
{
    public abstract partial class FileMakerApiClientBase
    {
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
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="input">The object with properties to map to the find request.</param>
        /// <param name="fmid">Function to map a the FileMaker RecordId to each instance T.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public virtual Task<IEnumerable<T>> FindAsync<T>(T input, Func<T, int, object> fmid) where T : class, new()
        {
            return FindAsync(input, null, null, fmid);
        }

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="input">The object to utilize for the find request parameters.</param>
        /// <param name="script">Script to run after the request is completed.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="fmid">Function to map the FileMaker RecordId to each instance T.</param>
        /// <returns></returns>
        public virtual Task<IEnumerable<T>> FindAsync<T>(T input, string script, string scriptParameter, Func<T, int, object> fmid) where T : class, new()
        {
            var req = _findFactory<T>();

            if (!string.IsNullOrEmpty(script))
            {
                req.Script = script;
                req.ScriptParameter = scriptParameter;
            }

            req.Layout = GetTableName(input);
            req.Query = new List<T>() { input };

            return SendAsync(req, fmid);
        }

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="layout">The name of the layout to run this request on.</param>
        /// <param name="request">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public virtual Task<IEnumerable<T>> FindAsync<T>(string layout, T request) where T : class, new()
        {
            var req = _findFactory<T>();
            req.Layout = layout;
            req.Query = new List<T>() { request };
            return SendAsync(req);
        }

        /// <summary>
        /// Find a record with utilizing a class instance to define the find request field values.
        /// </summary>
        /// <typeparam name="T">The response type to extract and return.</typeparam>
        /// <param name="layout">The layout to perform the request on.</param>
        /// <param name="req">The dictionary of key/value pairs to find against.</param>
        /// <returns></returns>
        public abstract Task<IEnumerable<T>> FindAsync<T>(string layout, Dictionary<string, string> req);
    }
}