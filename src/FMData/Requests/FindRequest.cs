using System;
using System.Collections.Generic;
using System.Net;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FMData.Requests
{
    /// <summary>
    /// Ensure that numbers are written out to the json object as strings for FileMaker Data API compatibility.
    /// </summary>
    /// <remarks>Source: https://stackoverflow.com/a/39526179/86860</remarks>
    sealed class FormatNumbersAsTextConverter : JsonConverter
    {
        public override bool CanRead => false;
        public override bool CanWrite => true;
        public override bool CanConvert(Type type) => type == typeof(int);

        public override void WriteJson(
            JsonWriter writer, object value, JsonSerializer serializer)
        {
            int number = (int)value;
            writer.WriteValue(number.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// The object that contains the parameters to serialize for a find request.
    /// </summary>
    public partial class FindRequest<TRequestType> : RequestBase
    {
        /// <summary>
        /// The find request dictionary.
        /// </summary>
        [JsonProperty("query")]
        public IEnumerable<TRequestType> Query { get; set; }

        /// <summary>
        /// Maximum number of records to return for this request.
        /// </summary>
        [JsonProperty("range")]
        public int Range { get; set; } = 100; // default 

        /// <summary>
        /// The number of records to skip before returning records for this request.
        /// </summary>
        [JsonProperty("offset")]
        public int Offset { get; set; } = 0; // default

        /// <summary>
        /// The sort fields and directions for this request.
        /// </summary>
        [JsonProperty("sort")]
        public IEnumerable<Sort> Sort { get; set; }

        /// <summary>
        /// Create a find request from Json
        /// </summary>
        /// <param name="json">The incomming Json data to deserialize.</param>
        /// <returns>An instance of the FindRequest object from the provided Json string.</returns>
        public static FindRequest<T> FromJson<T>(string json) => JsonConvert.DeserializeObject<FindRequest<T>>(json);

        /// <summary>
        /// Convert the this instance to Json.
        /// </summary>
        /// <returns>Json serialization of this instance.</returns>
        public string ToJson() => JsonConvert.SerializeObject(
            this,
            Newtonsoft.Json.Formatting.None,
            new JsonSerializerSettings 
            { 
                NullValueHandling = NullValueHandling.Ignore, 
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = { new FormatNumbersAsTextConverter() }
            });
    }

    public partial class Sort
    {
        [JsonProperty("fieldName")]
        public string FieldName { get; set; }
        [JsonProperty("sortOrder")]
        public string SortOrder { get; set; }
    }
}