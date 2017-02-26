using System;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.ApiDatatypes
{
    public class PopularServerReport
    {
        [JsonProperty("endpoint")]
        public string ServerAddress { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("averageMatchesPerDay")]
        public double AverageMatchesPerDay { get; set; }

        public override bool Equals(object obj)
        {
            var report = obj as PopularServerReport;
            if (report == null)
                return false;

            return ServerAddress == report.ServerAddress &&
                   Name == report.Name &&
                   Math.Abs(AverageMatchesPerDay - report.AverageMatchesPerDay) < 0.001;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ServerAddress?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Name?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ AverageMatchesPerDay.GetHashCode();
                return hashCode;
            }
        }
    }
}
