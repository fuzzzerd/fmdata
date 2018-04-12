using Newtonsoft.Json;

namespace FMData.Requests
{
    public class DeleteRequest : RequestBase
    {
        [JsonProperty("recordId")]
        public string RecordId { get; set; }
    }
}