using Newtonsoft.Json;

namespace FMData.Rest.Requests
{
    /// <summary>
    /// Create Request Wrapper
    /// </summary>
    /// <typeparam name="T">The type to create.</typeparam>
    public class CreateRequest<T> : RequestBase, ICreateRequest<T>
    {
        /// <summary>
        /// The field data for the create request.
        /// </summary>
        [JsonProperty("fieldData")]
        public T Data { get; set; }
    }
}