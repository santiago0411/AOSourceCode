using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AO.Core.Logging;
using SqlKata;
using SqlKata.Execution;

namespace AO.Core.Database
{
    public static class DatabaseManager
    {
        public static bool DatabaseActive;
        
        private static DatabaseBase database;
    
        private static readonly LoggerAdapter log = new(typeof(DatabaseManager));

        public static async Task Init()
        {
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            
            database = new MySqlDatabase("root", "root", "localhost", 3306, "csantiag_ao_db");

            if (!await DatabaseOperations.TestConnection())
                GameManager.CloseApplication();
            
            DatabaseActive = true;
            log.Info("DatabaseManager start-up successful.");
        }

        public static DbConnection GetConnection()
        {
            return database.GetConnection();
        }

        public static Query BeginQuery()
        {
#if AO_LOG_QUERIES
            return new XQuery(database.GetConnection(), database.GetCompiler())
            {
                Logger = compiledSql => log.Debug(compiledSql.ToString())
            };
#else
            return new XQuery(database.GetConnection(), database.GetCompiler());
#endif
        }

        public static Query BeginQuery(DbConnection connection)
        {
#if AO_LOG_QUERIES
            return new XQuery(connection, database.GetCompiler())
            {
                Logger = compiledSql => log.Debug(compiledSql.ToString())
            };
#else
            return new XQuery(connection, database.GetCompiler());
#endif
        }

        public static async Task<Transaction> BeginTransactionAsync(CancellationToken token = default)
        {
            try
            {
                return await Transaction.BeginTransactionAsync(database, token);
            }
            catch (Exception ex)
            {
                log.Error("Failed to begin transaction. {0}\n{1}", ex.Message, ex.StackTrace);
                return null;
            }
        }

        public static void OnDatabaseOperationFailed()
        {
            DatabaseActive = false;
            // Send an email or something
        }

        public static void OnDatabaseOperationFailed(Query query)
        {
            // Recompile the query and save it to a file together with a dump of the data probably?
        }
    }
}