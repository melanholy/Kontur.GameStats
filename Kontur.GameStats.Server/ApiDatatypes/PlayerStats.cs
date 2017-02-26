using System;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.ApiDatatypes
{
    public class PlayerStats
    {
        [JsonProperty("totalMatchesPlayed")]
        public int TotalMatchesPlayed;

        [JsonProperty("totalMatchesWon")]
        public int TotalMatchesWon;

        [JsonProperty("favoriteServer")]
        public string FavoriteServer;

        [JsonProperty("uniqueServers")]
        public int UniqueServers;

        [JsonProperty("favoriteGameMode")]
        public string FavoriteGameMode;

        [JsonProperty("averageScoreboardPercent")]
        public double AverageScoreboardPercent;

        [JsonProperty("maximumMatchesPerDay")]
        public int MaximumMatchesPerDay;

        [JsonProperty("averageMatchesPerDay")]
        public double AverageMatchesPerDay;

        [JsonProperty("lastMatchPlayed")]
        public DateTime LastMatchPlayed;

        [JsonProperty("killToDeathRatio")]
        public double KillToDeathRatio;
        
        public override bool Equals(object obj)
        {
            var stats = obj as PlayerStats;
            if (stats == null)
                return false;
            
            return TotalMatchesPlayed == stats.TotalMatchesPlayed &&
                   TotalMatchesWon == stats.TotalMatchesWon &&
                   FavoriteServer == stats.FavoriteServer &&
                   UniqueServers == stats.UniqueServers &&
                   FavoriteGameMode == stats.FavoriteGameMode &&
                   Math.Abs(AverageScoreboardPercent - stats.AverageScoreboardPercent) < 0.001 &&
                   MaximumMatchesPerDay == stats.MaximumMatchesPerDay &&
                   Math.Abs(AverageMatchesPerDay - stats.AverageMatchesPerDay) < 0.001 &&
                   LastMatchPlayed == stats.LastMatchPlayed &&
                   Math.Abs(KillToDeathRatio - stats.KillToDeathRatio) < 0.001;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TotalMatchesPlayed;
                hashCode = (hashCode*397) ^ TotalMatchesWon;
                hashCode = (hashCode*397) ^ (FavoriteServer?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ UniqueServers;
                hashCode = (hashCode*397) ^ (FavoriteGameMode?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ AverageScoreboardPercent.GetHashCode();
                hashCode = (hashCode*397) ^ MaximumMatchesPerDay;
                hashCode = (hashCode*397) ^ AverageMatchesPerDay.GetHashCode();
                hashCode = (hashCode*397) ^ LastMatchPlayed.GetHashCode();
                hashCode = (hashCode*397) ^ KillToDeathRatio.GetHashCode();
                return hashCode;
            }
        }
    }
}
