using Newtonsoft.Json;

namespace FMData.Rest.Requests
{
    public class DeleteRequest : RequestBase, IDeleteRequest
    {
        [JsonProperty("recordId")]
        public string RecordId { get; set; }
    }
}