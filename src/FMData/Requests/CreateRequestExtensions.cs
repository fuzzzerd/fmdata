namespace FMData
{
    /// <summary>
    /// Extension (utility and helper) methods for ICreateRequest.
    /// </summary>
    public static class CreateRequestExtensions
    {
        /// <summary>
        /// Set the data for this request.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="data">Object containing the data for this find request record.</param>
        /// <typeparam name="T">The type used for the create request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static ICreateRequest<T> SetData<T>(this ICreateRequest<T> request, T data)
        {
            request.Data = data;
            return request;
        }

        /// <summary>
        /// Specify a layout to use for this request.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="layout">Name of the layout to use</param>
        /// <typeparam name="T">The type used for the create request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static ICreateRequest<T> UseLayout<T>(this ICreateRequest<T> request, string layout)
        {
            request.Layout = layout;
            return request;
        }

        /// <summary>
        /// Specify a layout to use for this request.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="instance">Object to pull the layout from using its DataContract attribute.</param>
        /// <typeparam name="T">The type used for the create request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static ICreateRequest<T> UseLayout<T>(this ICreateRequest<T> request, T instance)
        {
            request.Layout = FileMakerApiClientBase.GetLayoutName(instance);
            return request;
        }

        /// <summary>
        /// Adds a pre request script to the request, with a parameter.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="scriptName">Name of the script to be called.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static ICreateRequest<T> SetPreRequestScript<T>(
            this ICreateRequest<T> request,
            string scriptName,
            string scriptParameter = null)
        {
            request.PreRequestScript = scriptName;
            if (!string.IsNullOrEmpty(scriptParameter)) request.PreRequestScriptParameter = scriptParameter;
            return request;
        }

        /// <summary>
        /// Adds a pre sort script to the request, with a parameter.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="scriptName">Name of the script to be called.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static ICreateRequest<T> SetPreSortScript<T>(
            this ICreateRequest<T> request,
            string scriptName,
            string scriptParameter = null)
        {
            request.PreSortScript = scriptName;
            if (!string.IsNullOrEmpty(scriptParameter)) request.PreSortScriptParameter = scriptParameter;
            return request;
        }

        /// <summary>
        /// Adds a script to the request, with a parameter.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="scriptName">Name of the script to be called.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static ICreateRequest<T> SetScript<T>(
            this ICreateRequest<T> request,
            string scriptName,
            string scriptParameter = null)
        {
            request.Script = scriptName;
            if (!string.IsNullOrEmpty(scriptParameter)) request.ScriptParameter = scriptParameter;
            return request;
        }
    }
}