using System;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Converters
{
    internal class JsonDoubleConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(double);
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            var number = (double) value;
            writer.WriteRawValue(number.ToString("F6"));
        }
    }
}
