using System.Data.Common;
using MySql.Data.MySqlClient;
using SqlKata.Compilers;

namespace AO.Core.Database
{
    public sealed class MySqlDatabase : DatabaseBase
    {
        private readonly string connectionString;
        private readonly MySqlCompiler compiler = new();

        public MySqlDatabase(string username, string password, string server, uint port, string database)
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                UserID = username,
                Password = password,
                Server = server,
                Port = port,
                Database = database,
                SslMode = MySqlSslMode.None,
                ConnectionTimeout = 15,
                Pooling = true,
                MinimumPoolSize = 3,
                MaximumPoolSize = 10
            };

            connectionString = connectionStringBuilder.ToString();
        }

        public override DbConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public override Compiler GetCompiler()
        {
            return compiler;
        }
    }
}