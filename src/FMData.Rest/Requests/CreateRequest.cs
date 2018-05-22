using Newtonsoft.Json;

namespace FMData.Rest.Requests
{
    public class CreateRequest<T> : RequestBase, ICreateRequest<T>
    {
        [JsonProperty("fieldData")]
        public T Data { get; set; }
    }
}