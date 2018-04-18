using Newtonsoft.Json;

namespace FMData.Responses
{
    public class BaseDataResponse
    {
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }
        [JsonProperty("result")]
        public string Result { get; set; }
    }
}