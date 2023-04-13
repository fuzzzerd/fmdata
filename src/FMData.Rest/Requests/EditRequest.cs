using Newtonsoft.Json;

namespace FMData.Rest.Requests
{
    /// <summary>
    /// Edit Request Wrapper
    /// </summary>
    /// <typeparam name="T">The type to edit.</typeparam>
    public class EditRequest<T> : RequestBase, IEditRequest<T>
    {
        /// <summary>
        /// Edit request data. The values from this object are what will be passed to FileMaker's API.
        /// </summary>
        [JsonProperty("fieldData")]
        public T Data { get; set; }

        /// <summary>
        /// The FileMaker RecordID of the record to be edited.
        /// </summary>
        [JsonIgnore]
        public int RecordId { get; set; }

        /// <summary>
        /// Optional -- FileMaker Modification ID to ensure no update was made from the time the data was read to updated.
        /// </summary>
        [JsonProperty("modId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int ModId { get; set; }
    }
}
