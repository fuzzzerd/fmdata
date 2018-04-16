using Newtonsoft.Json;

namespace FMData.Requests
{
    public class CreateRequest<T> : RequestBase
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}