using Newtonsoft.Json;

namespace FMData.Rest.Requests
{
    public class EditRequest<T> : IEditRequest<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonIgnore]
        public string RecordId { get; set; }

        [JsonProperty("modId")]
        public string ModId { get; set; }

        [JsonIgnore]
        public string Layout { get; set; }
    }
}