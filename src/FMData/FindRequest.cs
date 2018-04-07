using System;
using System.Collections.Generic;
using System.Net;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FMData
{
    public partial class FindRequest
    {
        /// <summary>
        /// Name of the layout to run the request on
        /// </summary>
        /// <returns></returns>
        public string Layout { get; set; }
        
        /// <summary>
        /// The find request dictionary.
        /// </summary>
        [JsonProperty("query")]
        public IEnumerable<Dictionary<string, string>> Query { get; set; }

        /// <summary>
        /// Maximum number of records to return for this request.
        /// </summary>
        [JsonProperty("range")]
        public int Range { get; set; }

        /// <summary>
        /// The number of records to skip before returning records for this request.
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// The sort fields and directions for this request.
        /// </summary>
        [JsonProperty("sort")]
        public Sort[] Sort { get; set; }

        /// <summary>
        /// Create a find request from Json
        /// </summary>
        /// <param name="json">The incomming Json data to deserialize.</param>
        /// <returns>An instance of the FindRequest object from the provided Json string.</returns>
        public static FindRequest FromJson(string json) => JsonConvert.DeserializeObject<FindRequest>(json);

        /// <summary>
        /// Convert the this instance to Json.
        /// </summary>
        /// <returns>Json serialization of this instance.</returns>
        public string ToJson() => JsonConvert.SerializeObject(this);
    }

    public partial class Sort
    {
        [JsonProperty("fieldName")]
        public string FieldName { get; set; }
        [JsonProperty("sortOrder")]
        public string SortOrder { get; set; }
    }
}