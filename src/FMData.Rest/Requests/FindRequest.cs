using Newtonsoft.Json;
using System.Collections.Generic;

namespace FMData.Rest.Requests
{
    /// <summary>
    /// The object that contains the parameters to serialize for a find request.
    /// </summary>
    public partial class FindRequest<TRequestType> : RequestBase, IFindRequest<TRequestType>
    {
        /// <summary>
        /// The find request dictionary.
        /// </summary>
        [JsonProperty("query")]
        public IEnumerable<TRequestType> Query { get; set; }

        /// <summary>
        /// Maximum number of records to return for this request.
        /// </summary>
        [JsonProperty("limit")]
        public int Limit { get; set; } = 100; // default 

        /// <summary>
        /// The number of records to skip before returning records for this request.
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; set; } = 1; // default

        /// <summary>
        /// The sort fields and directions for this request.
        /// </summary>
        [JsonProperty("sort")]
        public IEnumerable<ISort> Sort { get; set; }

        /// <summary>
        /// Create a find request from Json
        /// </summary>
        /// <param name="json">The incomming Json data to deserialize.</param>
        /// <returns>An instance of the FindRequest object from the provided Json string.</returns>
        public static FindRequest<T> FromJson<T>(string json) => JsonConvert.DeserializeObject<FindRequest<T>>(json);
    }

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