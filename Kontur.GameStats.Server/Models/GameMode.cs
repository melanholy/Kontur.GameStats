using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Kontur.GameStats.Server.Models
{
    public class GameMode
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string Name { get; set; }

        [JsonIgnore]
        public virtual GameServer Server { get; set; }
        
        public override bool Equals(object obj)
        {
            var mode = obj as GameMode;
            if (mode == null)
                return false;

            return Name == mode.Name;
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
    }
}