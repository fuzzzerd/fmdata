namespace FMData
{
    /// <summary>
    /// Extension (utility and helper) methods for ICreateRequest.
    /// </summary>
    public static class CreateRequestExtensions
    {
        /// <summary>
        /// Generates a new create request for the input data.
        /// </summary>
        /// <param name="client">The FileMaker API client instance.</param>
        /// <param name="data">The initial find request data.</param>
        /// <typeparam name="T">The type used for the create request.</typeparam>
        /// <returns>An IFindRequest{T} instance setup per the initial query paramater.</returns>
        public static ICreateRequest<T> GenerateCreateRequest<T>(this IFileMakerApiClient client, T data)
        {
            return client.GenerateCreateRequest<T>()
                .SetData(data)
                .UseLayout(data);
        }

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
    }
}