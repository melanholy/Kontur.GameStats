using System;
using Kontur.GameStats.Server.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kontur.GameStats.Server.Converters
{
    public class JsonPlayerScoreConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(PlayerScore).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return obj.ToObject<PlayerScore>();
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
