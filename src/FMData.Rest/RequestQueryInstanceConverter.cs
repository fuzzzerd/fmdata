using Newtonsoft.Json;
using FMData;
using System;
using Newtonsoft.Json.Linq;

namespace FMData.Rest
{
    /// <summary>
    /// JSON Convert Class that is used to combine the query object with the omit attribute that FileMaker Server expects to see.
    /// </summary>
    /// <typeparam name="T">Generic Type Instance of the Request to be converted.</typeparam>
    class RequestQueryInstanceConverter<T> : JsonConverter<RequestQueryInstance<T>>
    {
        public override void WriteJson(JsonWriter writer, RequestQueryInstance<T> value, JsonSerializer serializer)
        {
            JObject jo = JObject.FromObject(value.QueryInstance);
            if (value.Omit)
            {
                jo.Add("omit", value.Omit);
            }
            string json = jo.ToString();
            writer.WriteRawValue(json);
        }

        public override RequestQueryInstance<T> ReadJson(JsonReader reader, Type objectType, RequestQueryInstance<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}