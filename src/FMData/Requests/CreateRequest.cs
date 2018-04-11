using Newtonsoft.Json;
using System.Collections.Generic;

namespace FMData.Requests
{
    public class CreateRequest : RequestBase
    {
        [JsonProperty("data")]
        public Dictionary<string, string> Data { get; set; }
    }
}