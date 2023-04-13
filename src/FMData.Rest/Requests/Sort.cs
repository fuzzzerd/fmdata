using Newtonsoft.Json;

namespace FMData.Rest.Requests
{
    /// <summary>
    /// Data Class For Sort
    /// </summary>
    public partial class Sort : ISort
    {
        /// <summary>
        /// The name of the sort field.
        /// </summary>
        [JsonProperty("fieldName")]
        public string FieldName { get; set; }

        /// <summary>
        /// Sort direction (ascend/descend).
        /// </summary>
        [JsonProperty("sortOrder")]
        public string SortOrder { get; set; }
    }
}
