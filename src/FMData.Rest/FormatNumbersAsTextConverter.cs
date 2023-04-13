using System;
using System.Globalization;
using Newtonsoft.Json;

namespace FMData.Rest
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
            var number = (int)value;
            writer.WriteValue(number.ToString(CultureInfo.InvariantCulture));
        }

        public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
