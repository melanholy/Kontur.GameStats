using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Models
{
    public class GameMatch
    {
        [Key]
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [JsonIgnore]
        public virtual GameServer Server { get; set; }

        [JsonIgnore]
        public DateTime Timestamp { get; set; }

        [JsonProperty("map")]
        public string Map { get; set; }

        [JsonProperty("gameMode")]
        public string GameMode { get; set; }

        [JsonProperty("fragLimit")]
        public int FragLimit { get; set; }

        [JsonProperty("timeLimit")]
        public int TimeLimit { get; set; }

        [JsonProperty("timeElapsed")]
        public double TimeElapsed { get; set; }

        [JsonIgnore]
        public int TotalPlayers { get; set; }

        [JsonProperty("scoreboard")]
        public virtual ICollection<PlayerScore> Scoreboard { get; set; }
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Server?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode*397) ^ (Map?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (GameMode?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ FragLimit;
                hashCode = (hashCode*397) ^ TimeLimit;
                hashCode = (hashCode*397) ^ TimeElapsed.GetHashCode();
                hashCode = (hashCode*397) ^ (Scoreboard?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            var match = obj as GameMatch;
            if (match == null)
                return false;

            return Scoreboard.Equals(match.Scoreboard, score => score.Place) &&
                   Map == match.Map &&
                   match.GameMode == GameMode &&
                   match.Timestamp == Timestamp &&
                   match.FragLimit == FragLimit &&
                   Math.Abs(match.TimeElapsed - TimeElapsed) < 0.001 &&
                   match.TimeLimit == TimeLimit &&
                   match.Server.Equals(Server);
        }
    }
}
