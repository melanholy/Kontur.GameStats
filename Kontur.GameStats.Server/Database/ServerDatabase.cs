using System.Data.Entity;
using System.Data.SQLite;
using Kontur.GameStats.Server.Models;

namespace Kontur.GameStats.Server.Database
{
    public class ServerDatabase : DbContext
    {
        public DbSet<GameServer> GameServers { get; set; }
        public DbSet<GameMode> GameModes { get; set; }
        public DbSet<GameMatch> GameMatches { get; set; }
        public DbSet<PlayerScore> PlayerScores { get; set; }
        
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            if (!(Database.Connection is SQLiteConnection))
                return;

            var sqliteConnectionInitializer = new DatabaseInitializer(modelBuilder);
            System.Data.Entity.Database.SetInitializer(sqliteConnectionInitializer);
        }
    }
}
