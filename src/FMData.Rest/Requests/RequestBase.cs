using Newtonsoft.Json;

namespace FMData.Rest
{
    /// <summary>
    /// Base Request Implementation
    /// </summary>
    public abstract class RequestBase : IFileMakerRequest
    {
        /// <summary>
        /// Name of the layout FileMaker should be on when processing this request.
        /// </summary>
        [JsonIgnore] // don't serialize to output, this is internal to for our use as this is part of the route not the payload
        public string Layout { get; set; }

        /// <summary>
        /// The layout the response should take place on (useful for projecting different data than the request).
        /// </summary>
        [JsonProperty("layout.response")]
        public string ResponseLayout { get; set; }

        /// <summary>
        /// Request Script. Occurs post request, and post sort.
        /// </summary>
        [JsonProperty("script")]
        public string Script { get; set; }

        /// <summary>
        /// Request Script Parameter.
        /// </summary>
        [JsonProperty("script.param")]
        public string ScriptParameter { get; set; }

        /// <summary>
        /// Pre-request script. Runs after going to the layout in the request, but before the API request takes place.
        /// </summary>
        [JsonProperty("script.prerequest")]
        public string PreRequestScript { get; set; }

        /// <summary>
        /// Pre-request script parameter.
        /// </summary>
        [JsonProperty("script.prerequest.param")]
        public string PreRequestScriptParameter { get; set; }

        /// <summary>
        /// Pre-sort request. Occurs after the pre-request and the api request but before the sort has occurred.
        /// </summary>
        [JsonProperty("script.presort")]
        public string PreSortScript { get; set; }

        /// <summary>
        /// Pre-sort script parameter.
        /// </summary>
        [JsonProperty("script.presort.param")]
        public string PreSortScriptParameter { get; set; }

        /// <summary>
        /// When set to true, serialization will include null values.
        /// </summary>
        [JsonIgnore] // don't serialize to output, this is internal to for our use as this is part of the route not the payload
        public bool IncludeNullValuesInSerializedOutput { get; set; }

        /// <summary>
        /// When set to true, serialization will include null values.
        /// </summary>
        [JsonIgnore] // don't serialize to output, this is internal to for our use as this is part of the route not the payload
        public bool IncludeDefaultValuesInSerializedOutput { get; set; }

        /// <summary>
        /// JSON Convert the current object to a string for passing out to the API.
        /// </summary>
        /// <returns></returns>
        public virtual string SerializeRequest() => JsonConvert.SerializeObject(this,
            Formatting.None,
            new JsonSerializerSettings
            {
                NullValueHandling = IncludeNullValuesInSerializedOutput ? NullValueHandling.Include : NullValueHandling.Ignore,
                DefaultValueHandling = IncludeDefaultValuesInSerializedOutput ? DefaultValueHandling.Include : DefaultValueHandling.Ignore,
                Converters = { new FormatNumbersAsTextConverter() }
            });
    }
}