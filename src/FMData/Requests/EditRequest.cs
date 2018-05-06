using Newtonsoft.Json;
using System.Collections.Generic;

namespace FMData.Requests
{
    public class EditRequest<T> : RequestBase
    {
        [JsonProperty("data")]
        public T Data { get; set; }

        [JsonIgnore]
        public string RecordId { get; set; }

        [JsonProperty("modId")]
        public string ModId { get; set; }
    }
}