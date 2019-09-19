using Newtonsoft.Json;
using System;
using Newtonsoft.Json.Linq;

namespace FMData.Rest
{
    /// <summary>
    /// JSON Convert Class that is used to combine the query object with the omit attribute that FileMaker Server expects to see.
    /// </summary>
    /// <typeparam name="T">Generic Type Instance of the Request to be converted.</typeparam>
    sealed class RequestQueryInstanceConverter<T> : JsonConverter<RequestQueryInstance<T>>
    {
        private readonly IFileMakerRequest _parentRequest;
        public RequestQueryInstanceConverter(IFileMakerRequest parentRequest)
        {
            _parentRequest = parentRequest;
        }

        public override void WriteJson(JsonWriter writer, RequestQueryInstance<T> value, JsonSerializer serializer)
        {
            var jsonConverted = JsonConvert.SerializeObject(value.QueryInstance, new JsonSerializerSettings
            {
                NullValueHandling = _parentRequest.IncludeNullValuesInSerializedOutput ? NullValueHandling.Include : NullValueHandling.Ignore,
                DefaultValueHandling = _parentRequest.IncludeDefaultValuesInSerializedOutput ? DefaultValueHandling.Include : DefaultValueHandling.Ignore,
                Converters =
                {
                    new FormatNumbersAsTextConverter()
                }
            });

            // only parse a second time if we have an omit to insert.
            if (value.Omit)
            {
                var jo = JObject.Parse(jsonConverted);
                // as always, filemaker is stringly typed...
                jo.Add("omit", "true");
                // overwrite original since we have an omit situation
                jsonConverted = jo.ToString();
            }

            writer.WriteRawValue(jsonConverted);
        }

        public override RequestQueryInstance<T> ReadJson(JsonReader reader, Type objectType, RequestQueryInstance<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}