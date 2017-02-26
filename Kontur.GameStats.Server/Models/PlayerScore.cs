using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Models
{
    public class PlayerScore
    {
        [Key]
        [JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("frags")]
        public int Frags { get; set; }

        [JsonProperty("kills")]
        public int Kills { get; set; }
        
        [JsonProperty("deaths")]
        public int Deaths { get; set; }

        [JsonIgnore]
        public int Place { get; set; }

        [JsonIgnore]
        public virtual GameMatch Match { get; set; }
        
        public override bool Equals(object obj)
        {
            var score = obj as PlayerScore;
            if (score == null)
                return false;

            return Name == score.Name &&
                   Frags == score.Frags &&
                   Kills == score.Kills &&
                   Deaths == score.Deaths &&
                   Place == score.Place;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Frags;
                hashCode = (hashCode * 397) ^ Kills;
                hashCode = (hashCode * 397) ^ Deaths;
                hashCode = (hashCode * 397) ^ Place;
                hashCode = (hashCode * 397) ^ (Match?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}