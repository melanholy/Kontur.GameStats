using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.ApiDatatypes
{
    public class MatchesStats
    {
        [JsonProperty("totalMatchesPlayed")]
        public int TotalMatchesPlayed;

        [JsonProperty("maximumMatchesPerDay")]
        public int MaximumMatchesPerDay;

        [JsonProperty("averageMatchesPerDay")]
        public double AverageMatchesPerDay;

        [JsonProperty("maximumPopulation")]
        public int MaximumPopulation;

        [JsonProperty("averagePopulation")]
        public double AveragePopulation;

        [JsonProperty("top5GameModes")]
        public IEnumerable<string> Top5GameModes;

        [JsonProperty("top5Maps")]
        public IEnumerable<string> Top5Maps;
        
        public override bool Equals(object obj)
        {
            var other = obj as MatchesStats;
            if (other == null)
                return false;

            return TotalMatchesPlayed == other.TotalMatchesPlayed &&
                   MaximumMatchesPerDay == other.MaximumMatchesPerDay &&
                   Math.Abs(AverageMatchesPerDay - other.AverageMatchesPerDay) < 0.001 &&
                   MaximumPopulation == other.MaximumPopulation &&
                   Math.Abs(AveragePopulation - other.AveragePopulation) < 0.001 &&
                   Top5GameModes.Equals(other.Top5GameModes, mode => mode) &&
                   Top5Maps.Equals(other.Top5Maps, map => map);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TotalMatchesPlayed;
                hashCode = (hashCode*397) ^ MaximumMatchesPerDay;
                hashCode = (hashCode*397) ^ AverageMatchesPerDay.GetHashCode();
                hashCode = (hashCode*397) ^ MaximumPopulation;
                hashCode = (hashCode*397) ^ AveragePopulation.GetHashCode();
                hashCode = (hashCode*397) ^ (Top5GameModes?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Top5Maps?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
