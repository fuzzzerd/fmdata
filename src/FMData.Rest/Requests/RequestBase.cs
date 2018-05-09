using Newtonsoft.Json;

namespace FMData
{
    public class RequestBase
    {
        /// <summary>
        /// Name of the layout to run the request on
        /// </summary>
        /// <returns></returns>
        [JsonIgnore] // don't serialize to output, this is internal to for our use
        public string Layout { get; set; }
    }
}