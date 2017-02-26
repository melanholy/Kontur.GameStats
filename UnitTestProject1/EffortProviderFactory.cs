using System.Data.Common;
using System.Data.Entity.Infrastructure;

namespace Kontur.GameStats.Tests
{
    public class EffortProviderFactory : IDbConnectionFactory
    {
        private static DbConnection connection;
        private static readonly object Lock = new object();

        public static void ResetDb()
        {
            lock (Lock)
                connection = null;
        }

        public DbConnection CreateConnection(string nameOrConnectionString)
        {
            lock (Lock)
                return connection ?? (connection = Effort.DbConnectionFactory.CreateTransient());
        }
    }
}
