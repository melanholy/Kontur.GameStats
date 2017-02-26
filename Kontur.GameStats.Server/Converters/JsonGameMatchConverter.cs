using System;
using Kontur.GameStats.Server.Models;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Converters
{
    public class JsonGameMatchConverter : JsonConverter
    {
        private static readonly JsonDoubleConverter DoubleConverter = new JsonDoubleConverter();
        private static readonly JsonDatetimeConverter DatetimeConverter = new JsonDatetimeConverter();

        public override bool CanConvert(Type objectType)
        {
            return typeof(GameMatch).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            var match = (GameMatch)value;

            writer.WriteRawValue(JsonConvert.SerializeObject(new
            {
                server = match.Server.Endpoint,
                timestamp = match.Timestamp,
                results = match
            }, DatetimeConverter, DoubleConverter));
        }
    }
}
