namespace FMData.Xml
{
    /// <summary>
    /// Base Request Implemenation
    /// </summary>
    public class RequestBase : IFileMakerRequest
    {
        /// <summary>
        /// Name of the layout to run the request on
        /// </summary>
        public string Layout { get; set; }

        /// <summary>
        /// The layout the response should take place on (useful for projecting different data than the request).
        /// </summary>
        //[JsonProperty("layout.response")]
        public string ResponseLayout { get; set; }

        /// <summary>
        /// Request Script. Occurs post request, and post sort.
        /// </summary>
        //[JsonProperty("script")]
        public string Script { get; set; }
        /// <summary>
        /// Request Script Parameter.
        /// </summary>
        //[JsonProperty("script.param")]
        public string ScriptParameter { get; set; }

        /// <summary>
        /// Pre-request script. Runs after going to the layout in the request, but before the API request takes place.
        /// </summary>
        //[JsonProperty("script.prerequest")]
        public string PreRequestScript { get; set; }
        /// <summary>
        /// Pre-request script parameter.
        /// </summary>
        //[JsonProperty("script.prerequest.param")]
        public string PreRequestScriptParameter { get; set; }

        /// <summary>
        /// Pre-sort request. Occurs after the pre-request and the api request but before the sort has occured.
        /// </summary>
        //[JsonProperty("script.presort")]
        public string PreSortScript { get; set; }
        /// <summary>
        /// Pre-sort script parameter.
        /// </summary>
        //[JsonProperty("script.presort.param")]
        public string PreSortScriptParameter { get; set; }

        /// <summary>
        /// JSON Convert the current object to a string for passing out to the API.
        /// </summary>
        /// <returns></returns>
        public virtual string SerializeRequest() => "";
    }
}