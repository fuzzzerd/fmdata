using Newtonsoft.Json;
using FMData;
using System;
using Newtonsoft.Json.Linq;

namespace FMData.Rest
{
    public class RequestQueryInstanceConverter<T> : JsonConverter<RequestQueryInstance<T>>
    {
         public override void WriteJson(JsonWriter writer, RequestQueryInstance<T> value, JsonSerializer serializer)
        {
            JObject jo = JObject.FromObject(value.QueryInstance);
            if(value.Omit)
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