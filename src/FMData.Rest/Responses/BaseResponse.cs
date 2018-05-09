using Newtonsoft.Json;

namespace FMData.Rest.Responses
{
    public class BaseResponse : IResponse
    {
        [JsonProperty("errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }
    }
}