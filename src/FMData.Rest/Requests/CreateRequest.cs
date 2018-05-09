using Newtonsoft.Json;

namespace FMData.Rest.Requests
{
    public class CreateRequest<T> : RequestBase, ICreateRequest<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}