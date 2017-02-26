using System.Data.Entity;
using SQLite.CodeFirst;

namespace Kontur.GameStats.Server.Database
{
    internal class DatabaseInitializer : SqliteCreateDatabaseIfNotExists<ServerDatabase>
    {
        public DatabaseInitializer(DbModelBuilder modelBuilder)
            : base(modelBuilder) { }

        protected override void Seed(ServerDatabase context)
        {
            // EF не умеет делать LIKE и COLLATE nocase, а LOWER() сильно замедляет работу
            const string query = "create index \"IX_PlayerName_Lower\" ON \"PlayerScores\" (lower(Name))";
            context.Database.ExecuteSqlCommand(query);
        }
    }
}
