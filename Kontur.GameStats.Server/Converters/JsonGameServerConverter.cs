using System;
using Kontur.GameStats.Server.Models;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Converters
{
    public class JsonGameServerConverter : JsonConverter
    {
        private static readonly JsonGameModeCollectionConverter GameModeCollectionConverter = 
            new JsonGameModeCollectionConverter();

        public override bool CanConvert(Type objectType)
        {
            return typeof(GameServer).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType,
            object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value,
            JsonSerializer serializer)
        {
            var server = (GameServer) value;

            writer.WriteRawValue(JsonConvert.SerializeObject(new
            {
                endpoint = server.Endpoint,
                info = new
                {
                    name = server.Name,
                    gameModes = server.GameModes
                }
            }, GameModeCollectionConverter));
        }
    }
}