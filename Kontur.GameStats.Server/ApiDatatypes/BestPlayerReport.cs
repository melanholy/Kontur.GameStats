using System;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.ApiDatatypes
{
    public class BestPlayerReport
    {
        [JsonProperty("killToDeathRatio")]
        public double KillToDeathRatio { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        public override bool Equals(object obj)
        {
            var report = obj as BestPlayerReport;
            if (report == null)
                return false;

            return Name == report.Name && 
                   Math.Abs(KillToDeathRatio - report.KillToDeathRatio) < 0.001;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (KillToDeathRatio.GetHashCode()*397) ^ (Name?.GetHashCode() ?? 0);
            }
        }
    }
}
