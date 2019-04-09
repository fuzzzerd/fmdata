using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FMData
{
    /// <summary>
    /// Find Extensions
    /// </summary>
    public static class FindExtensions
    {
        /// <summary>
        /// Find a record with utilizing a class instance to define the find request field values.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="request">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public static Task<IEnumerable<T>> FindAsync<T>(
            this IFileMakerApiClient client,
            T request) where T : class, new()
        {
            var req = client.GenerateFindRequest(request);
            return client.SendAsync(req);
        }

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <param name="take">Number of records to return.</param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> FindAsync<T>(
            this IFileMakerApiClient client,
            T request,
            int skip,
            int take) where T : class, new()
        {
            var req = client.GenerateFindRequest(request).SetLimit(take).SetOffset(skip);
            return client.SendAsync(req);
        }

        /// <summary>
        /// Find a record with utilizing a class instance to define the find request field values.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="request">The object with properties to map to the find request.</param>
        /// <param name="fmid">Function to map a the FileMaker RecordId to each instance T.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public static Task<IEnumerable<T>> FindAsync<T>(
            this IFileMakerApiClient client,
            T request,
            Func<T, int, object> fmid) where T : class, new()
        {
            var req = client.GenerateFindRequest(request);
            return client.SendAsync(req, fmid);
        }

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="fmid">Function to map the FileMaker RecordId to each instance T.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <param name="take">Number of records to return.</param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> FindAsync<T>(
            this IFileMakerApiClient client,
            T request,
            int skip,
            int take,
            Func<T, int, object> fmid) where T : class, new()
        {
            var req = client.GenerateFindRequest(request).SetLimit(take).SetOffset(skip);
            return client.SendAsync(req, fmid);
        }


        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="script">Script to run after the request is completed.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="fmid">Function to map the FileMaker RecordId to each instance T.</param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> FindAsync<T>(
             this IFileMakerApiClient client,
             T request,
             string script,
             string scriptParameter,
             Func<T, int, object> fmid) where T : class, new()
        {
            var req = client.GenerateFindRequest(request)
                .SetLimit(100)
                .SetOffset(0);
            req.Script = script;
            req.ScriptParameter = scriptParameter;
            return client.SendAsync(req, fmid);
        }

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <param name="take">Number of records to return.</param>
        /// <param name="script">Script to run after the request is completed.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="fmid">Function to map the FileMaker RecordId to each instance T.</param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> FindAsync<T>(
            this IFileMakerApiClient client, 
            T request,
            int skip,
            int take,
            string script,
            string scriptParameter,
            Func<T, int, object> fmid) where T : class, new()
        {
            var req = client.GenerateFindRequest(request)
                .SetLimit(take)
                .SetOffset(skip);
            req.Script = script;
            req.ScriptParameter = scriptParameter;
            return client.SendAsync(req, fmid);
        }

        /// <summary>
        /// Finds a record or records matching the properties of the input request object.
        /// </summary>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="request">The object to utilize for the find request parameters.</param>
        /// <param name="skip">Number of records to skip.</param>
        /// <param name="take">Number of records to return.</param>
        /// <param name="script">Script to run after the request is completed.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <param name="fmid">Function to map the FileMaker RecordId to each instance T.</param>
        /// <param name="modid">Function to map hte FileMaker ModId to each instance of T.</param>
        /// <returns></returns>
        public static Task<IEnumerable<T>> FindAsync<T>(
            this IFileMakerApiClient client,
            T request,
            int skip,
            int take,
            string script,
            string scriptParameter,
            Func<T, int, object> fmid,
            Func<T, int, object> modid) where T : class, new()
        {
            var req = client.GenerateFindRequest(request)
                .SetLimit(take)
                .SetOffset(skip);
            req.Script = script;
            req.ScriptParameter = scriptParameter;
            return client.SendAsync(req, fmid, modid);
        }

        /// <summary>
        /// Strongly typed find request.
        /// </summary>
        /// <typeparam name="T">The type of response objects to return.</typeparam>
        /// <param name="client">FileMaker API client instance.</param>
        /// <param name="layout">The name of the layout to run this request on.</param>
        /// <param name="request">The object with properties to map to the find request.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> matching the request parameters.</returns>
        public static Task<IEnumerable<T>> FindAsync<T>(
            this IFileMakerApiClient client,
            string layout, 
            T request) where T : class, new()
        {
            var req = client.GenerateFindRequest<T>();
            req.Layout = layout;
            req.AddQuery(request, false);
            return client.SendAsync(req);
        }
    }
}