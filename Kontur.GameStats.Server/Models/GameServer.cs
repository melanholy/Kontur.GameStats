using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Models
{
    public class GameServer
    {
        [Key]
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public string Endpoint { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("gameModes")]
        public virtual ICollection<GameMode> GameModes { get; set; }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Endpoint?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (GameModes?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            var server = obj as GameServer;
            if (server == null)
                return false;

            return server.Name == Name &&
                   server.Endpoint == Endpoint &&
                   GameModes.Equals(server.GameModes, mode => mode.Name);
        }
    }
}
