using Newtonsoft.Json;
using System.Collections.Generic;

namespace FMData.Requests
{
    public class EditRequest : RequestBase
    {
        [JsonProperty("data")]
        public Dictionary<string, string> Data { get; set; }
        public string RecordId { get; set; }
        public string ModId { get; set; }
    }
}