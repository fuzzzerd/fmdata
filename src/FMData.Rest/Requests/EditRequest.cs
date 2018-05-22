using Newtonsoft.Json;

namespace FMData.Rest.Requests
{
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
        public string RecordId { get; set; }

        /// <summary>
        /// Optional -- FileMaker Modification ID to ensure no update was made from the time the data was read to updated.
        /// </summary>
        [JsonProperty("modId")]
        public string ModId { get; set; }
    }
}