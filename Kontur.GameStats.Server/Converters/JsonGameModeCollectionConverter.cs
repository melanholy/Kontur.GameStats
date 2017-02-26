using System;
using System.Collections.Generic;
using System.Linq;
using Kontur.GameStats.Server.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kontur.GameStats.Server.Converters
{
    public class JsonGameModeCollectionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ICollection<GameMode>).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            var array = JArray.Load(reader);
            var result = new GameMode[array.Count];
            for (var i = 0; i < array.Count; i++)
                result[i] = new GameMode { Name = array[i].Value<string>() };

            return result;
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            var modes = (ICollection<GameMode>) value;
            var array = JArray.FromObject(modes.Select(mode => mode.Name));
            array.WriteTo(writer);
        }
    }
}
