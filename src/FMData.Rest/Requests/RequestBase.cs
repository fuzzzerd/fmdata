using Newtonsoft.Json;

namespace FMData
{
    public class RequestBase
    {
        /// <summary>
        /// Name of the layout to run the request on
        /// </summary>
        [JsonIgnore] // don't serialize to output, this is internal to for our use
        public string Layout { get; set; }

        /// <summary>
        /// The layout the response should take place on (useful for projecting different data than the request).
        /// </summary>
        [JsonProperty("layout.response")]
        public string ResponseLayout { get; set; }

        /// <summary>
        /// JSON Convert the current object to a string for passing out to the API.
        /// </summary>
        /// <returns></returns>
        public string SerializeRequest()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}