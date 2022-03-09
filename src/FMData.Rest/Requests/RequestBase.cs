using System.IO;
using System.Text;
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
        [JsonProperty("layout.response", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ResponseLayout { get; set; }

        /// <summary>
        /// Request Script. Occurs post request, and post sort.
        /// </summary>
        [JsonProperty("script", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Script { get; set; }

        /// <summary>
        /// Request Script Parameter.
        /// </summary>
        [JsonProperty("script.param", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ScriptParameter { get; set; }

        /// <summary>
        /// Pre-request script. Runs after going to the layout in the request, but before the API request takes place.
        /// </summary>
        [JsonProperty("script.prerequest", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PreRequestScript { get; set; }

        /// <summary>
        /// Pre-request script parameter.
        /// </summary>
        [JsonProperty("script.prerequest.param", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PreRequestScriptParameter { get; set; }

        /// <summary>
        /// Pre-sort request. Occurs after the pre-request and the api request but before the sort has occurred.
        /// </summary>
        [JsonProperty("script.presort", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PreSortScript { get; set; }

        /// <summary>
        /// Pre-sort script parameter.
        /// </summary>
        [JsonProperty("script.presort.param", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
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
        public virtual string SerializeRequest()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var njw = new NullJsonWriter(sw))
            {
                // use our custom NullJsonWriter (that writes empty string for null values)
                // while still respecting the above configured Null and Default value handle flags.
                var ser = new JsonSerializer
                {
                    DefaultValueHandling = IncludeDefaultValuesInSerializedOutput ? DefaultValueHandling.Include : DefaultValueHandling.Ignore,
                    NullValueHandling = IncludeNullValuesInSerializedOutput ? NullValueHandling.Include : NullValueHandling.Ignore,
                    Formatting = Formatting.None,
                };
                // serialize this instance
                ser.Serialize(njw, this);
            }
            // return the string representation
            return sb.ToString();
        }
    }
}
