using Newtonsoft.Json;
using System.Collections.Generic;

namespace FMData.Requests
{
    public class EditRequest : RequestBase
    {
        [JsonProperty("data")]
        public Dictionary<string, string> Data { get; set; }

        [JsonIgnore]
        public string RecordId { get; set; }

        [JsonProperty("modId")]
        public string ModId { get; set; }
    }
}