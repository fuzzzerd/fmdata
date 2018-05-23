using Newtonsoft.Json;

namespace FMData.Rest.Requests
{
    /// <summary>
    /// Delete Record Request.
    /// </summary>
    public class DeleteRequest : RequestBase, IDeleteRequest
    {
        /// <summary>
        /// The FileMaker record id to delete.
        /// </summary>
        [JsonProperty("recordId")]
        public int RecordId { get; set; }
    }
}