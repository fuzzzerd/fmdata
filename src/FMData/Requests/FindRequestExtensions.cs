namespace FMData
{
    /// <summary>
    /// Extension (utility and helper) methods for IFindRequest.
    /// </summary>
    public static class FindRequestExtensions
    {
        /// <summary>
        /// Adds a new find criteria to the find request.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="criteria">Object containing the data for this find request record.</param>
        /// <param name="omit">Use this criteria for omit(int matching records) or not.</param>
        /// <typeparam name="T">The type used for the find request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFindRequest<T> AddCriteria<T>(this IFindRequest<T> request, T criteria, bool omit)
        {
            request.AddQuery(criteria, omit);
            return request;
        }

        /// <summary>
        /// Adds a sort field and direction to the find request.
        /// </summary>
        /// <typeparam name="T">The type used for the find request/response.</typeparam>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="field">The field to sort on.</param>
        /// <param name="direction">The direction to sort.</param>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFindRequest<T> AddSortFieldDirection<T>(this IFindRequest<T> request, string field, string direction)
        {
            request.AddSort(field, direction);
            return request;
        }

        /// <summary>
        /// Specify the limit of records to be returned.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="limit">The maximum number of records to be returned as part of this request.</param>
        /// <typeparam name="T">The type used for the find request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFindRequest<T> SetLimit<T>(this IFindRequest<T> request, int limit)
        {
            request.Limit = limit;
            return request;
        }

        /// <summary>
        /// Specify the number to offset (skip) before returning records.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="offset">The offset to use before returning records.</param>
        /// <typeparam name="T">The type used for the find request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFindRequest<T> SetOffset<T>(this IFindRequest<T> request, int offset)
        {
            request.Offset = offset;
            return request;
        }

        /// <summary>
        /// Specify a layout to use for this request.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="layout">Name of the layout to use</param>
        /// <typeparam name="T">The type used for the find request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFindRequest<T> UseLayout<T>(this IFindRequest<T> request, string layout)
        {
            request.Layout = layout;
            return request;
        }

        /// <summary>
        /// Specify a layout to use for this request.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="instance">Object to pull the layout from using its DataContract attribute.</param>
        /// <typeparam name="T">The type used for the find request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFindRequest<T> UseLayout<T>(this IFindRequest<T> request, T instance)
        {
            request.Layout = FileMakerApiClientBase.GetLayoutName(instance);
            return request;
        }

        /// <summary>
        /// Indicate that container data should be processed.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <typeparam name="T">The type used for the find request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFindRequest<T> LoadContainers<T>(this IFindRequest<T> request)
        {
            request.LoadContainerData = true;
            return request;
        }

        /// <summary>
        /// Adds a pre request script to the request, with a parameter.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="scriptName">Name of the script to be called.</param>
        /// <param name="scriptParameter">Script parameter.</param>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IFindRequest<T> SetPreRequestScript<T>(
            this IFindRequest<T> request,
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
        public static IFindRequest<T> SetPreSortScript<T>(
            this IFindRequest<T> request,
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
        public static IFindRequest<T> SetScript<T>(
            this IFindRequest<T> request,
            string scriptName,
            string scriptParameter = null)
        {
            request.Script = scriptName;
            if (!string.IsNullOrEmpty(scriptParameter)) request.ScriptParameter = scriptParameter;
            return request;
        }
    }
}
