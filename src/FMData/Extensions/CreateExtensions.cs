using System;
using System.Threading.Tasks;

namespace FMData
{
    /// <summary>
    /// Extension methods for IFileMakerApiClient.
    /// </summary>
    public static class CreateExtensions
    {
        /// <summary>
        /// Create a record in the database utilizing the TableAttribute to target the layout.
        /// </summary>
        /// <typeparam name="T">The type parameter to be created.</typeparam>
        /// <param name="client">The client that is the this object in the static method</param>
        /// <param name="input">Object containing the data to be on the newly created record.</param>
        /// <returns></returns>
        public static Task<ICreateResponse> CreateAsync<T>(
                this IFileMakerApiClient client,
                T input)
            where T : class, new()
        {
            var request = client.GenerateCreateRequest(input);
            return client.SendAsync(request);
        }

        /// <summary>
        /// Create a record in the file, attempt to use the [TableAttribute] to determine the layout and perform a script with parameter.
        /// </summary>
        /// <typeparam name="T">The type to create</typeparam>
        /// <param name="client">The client that is the this object in the static method</param>
        /// <param name="input">The input record to create.</param>
        /// <param name="script">The name of a FileMaker script to run.</param>
        /// <param name="scriptParameter">The parameter to pass to the script.</param>
        /// <returns></returns>
        public static Task<ICreateResponse> CreateAsync<T>(
            this IFileMakerApiClient client,
            T input,
            string script,
            string scriptParameter) where T : class, new()
        {
            var request = client.GenerateCreateRequest(input);
            request.Script = script;
            request.ScriptParameter = scriptParameter;
            return client.SendAsync(request);
        }

        /// <summary>
        /// Creates a record matching the input data. All possible scripts available.
        /// Empty script names will be ignored.
        /// </summary>
        /// <typeparam name="T">The type of record to be created.</typeparam>
        /// /// <param name="client">The client that is the this object in the static method</param>
        /// <param name="input">The data to put in the record.</param>
        /// <param name="script">Name of the script to run at request completion.</param>
        /// <param name="scriptParameter">Parameter for script.</param>
        /// <param name="preRequestScript">Script to run before the request. See FMS documentation for more details.</param>
        /// <param name="preRequestScriptParameter">Parameter for script.</param>
        /// <param name="preSortScript">Script to run after the request, but before the sort. See FMS documentation for more details.</param>
        /// <param name="preSortScriptParameter">Parameter for script.</param>
        /// <returns>A response indicating the results of the call to the FileMaker Server Data API.</returns>
        public static Task<ICreateResponse> CreateAsync<T>(
            this IFileMakerApiClient client,
            T input,
            string script,
            string scriptParameter,
            string preRequestScript,
            string preRequestScriptParameter,
            string preSortScript,
            string preSortScriptParameter) where T : class, new()
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var request = client.GenerateCreateRequest(input);

            if (!string.IsNullOrEmpty(script))
            {
                request.Script = script;
                request.ScriptParameter = scriptParameter;
            }
            if (!string.IsNullOrEmpty(preRequestScript))
            {
                request.PreRequestScript = preRequestScript;
                request.PreRequestScriptParameter = preRequestScriptParameter;
            }

            if (!string.IsNullOrEmpty(preSortScript))
            {
                request.PreSortScript = preSortScript;
                request.PreSortScriptParameter = preSortScriptParameter;
            }

            return client.SendAsync(request);
        }
    }
}