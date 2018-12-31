namespace FMData
{
    /// <summary>
    /// Extension (utility and helper) methods for IEditRequest.
    /// </summary>
    public static class EditRequestExtensions
    {
        /// <summary>
        /// Generates a new edit request for the input object.
        /// </summary>
        /// <param name="client">The FileMaker API client instance.</param>
        /// <param name="data">The initial edit data request.</param>
        /// <typeparam name="T">The type used for the edit request.</typeparam>
        /// <returns>An IFindRequest{T} instance setup per the initial query paramater.</returns>
        public static IEditRequest<T> GenerateEditRequest<T>(this IFileMakerApiClient client, T data)
        {
            return client.GenerateEditRequest<T>()
                .SetData(data)
                .UseLayout(data);
        }

        /// <summary>
        /// Set the data for this request.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="data">Object containing the data for this find request record.</param>
        /// <typeparam name="T">The type used for the edit request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IEditRequest<T> SetData<T>(this IEditRequest<T> request, T data)
        {
            request.Data = data;
            return request;
        }

        /// <summary>
        /// Specify a layout to use for this request.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="layout">Name of the layout to use</param>
        /// <typeparam name="T">The type used for the edit request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IEditRequest<T> UseLayout<T>(this IEditRequest<T> request, string layout)
        {
            request.Layout = layout;
            return request;
        }

        /// <summary>
        /// Specify a layout to use for this request.
        /// </summary>
        /// <param name="request">The request. This is the 'this' parameter.</param>
        /// <param name="instance">Object to pull the layout from using its DataContract attribute.</param>
        /// <typeparam name="T">The type used for the edit request/response.</typeparam>
        /// <returns>The request instanced that was implicitly passed in which is useful for method chaining.</returns>
        public static IEditRequest<T> UseLayout<T>(this IEditRequest<T> request, T instance)
        {
            request.Layout = FileMakerApiClientBase.GetLayoutName(instance);
            return request;
        }
    }
}