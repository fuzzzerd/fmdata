namespace FMData
{
    /// <summary>
    /// Methods and Properties all requests must implement.
    /// </summary>
    public interface IFileMakerRequest
    {
        /// <summary>
        /// The layout to run the request against.
        /// </summary>
        string Layout { get; set; }

        /// <summary>
        /// The layout to respond with. 
        /// </summary>
        string ResponseLayout { get; set; }

        /// <summary>
        /// Name of the script to run after the request has completed.
        /// </summary>
        string Script { get; set; }
        /// <summary>
        /// Request Script Parameter.
        /// </summary>
        string ScriptParameter { get; set; }

        /// <summary>
        /// Pre-request script. Runs after going to the layout in the request, but before the API request takes place.
        /// </summary>
        string PreRequestScript { get; set; }
        /// <summary>
        /// Pre-request script parameter.
        /// </summary>
        string PreRequestScriptParameter { get; set; }

        /// <summary>
        /// Pre-sort request. Occurs after the pre-request and the api request but before the sort has occurred.
        /// </summary>
        string PreSortScript { get; set; }
        /// <summary>
        /// Pre-sort script parameter.
        /// </summary>
        string PreSortScriptParameter { get; set; }

        /// <summary>
        /// When set to true, serialization will include null values.
        /// </summary>
        bool IncludeNullValuesInSerializedOutput { get; set; }

        /// <summary>
        /// When set to true, serialization will include default values.
        /// </summary>
        bool IncludeDefaultValuesInSerializedOutput { get; set; }

        /// <summary>
        /// Serializes the request to the required format.
        /// </summary>
        /// <returns>A serialized string represneting the current request.</returns>
        string SerializeRequest();
    }
}