using System;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Converters
{
    internal class JsonDatetimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            return DateTime.Parse(reader.ReadAsString()).ToUniversalTime();
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            var date = (DateTime) value;
            writer.WriteValue(date.ToString("yyyy-MM-ddTHH:mm:ssZ"));
        }
    }
}
