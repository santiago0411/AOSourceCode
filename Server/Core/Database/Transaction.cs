using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using AO.Core.Logging;
using SqlKata;
using SqlKata.Execution;

namespace AO.Core.Database
{
    public sealed class Transaction
    {
        private static readonly LoggerAdapter log = new(typeof(Transaction));
        
        public DbTransaction DbTransaction { get; private set; }
        
        private DbConnection connection;
        private bool disposed;
        private readonly DatabaseBase database;
        
        private Transaction(DatabaseBase db, DbTransaction tr)
        {
            database = db;
            DbTransaction = tr;
            connection = tr.Connection;
        }

        public static async Task<Transaction> BeginTransactionAsync(DatabaseBase db, CancellationToken token)
        {
            var conn = db.GetConnection();
            await conn.OpenAsync(token);
            return new Transaction(db, await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted, token));
        }

        public Query BeginQuery()
        {
#if AO_LOG_QUERIES
            return new XQuery(transaction.Connection, database.GetCompiler())
            {
                Logger = compiledSql => log.Debug(compiledSql.ToString())
            };
#else
            return new XQuery(DbTransaction.Connection, database.GetCompiler());
#endif
        }

        public async Task<bool> CommitTransactionAsync()
        {
            try
            {
                await DbTransaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Failed to commit transaction. {0}\n{1}", ex.Message, ex.StackTrace);
                return false;
            }
            finally
            {
                await DisposeAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                await DbTransaction.RollbackAsync();
            }
            catch (Exception ex)
            {
                // I don't think this can ever happen but you never know :)
                log.Error("Failed to rollback transaction. {0}\n{1}", ex.Message, ex.StackTrace);
            }
            finally
            {
                await DisposeAsync();
            }
        }

        ~Transaction()
        {
            Dispose(false);
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private async void Dispose(bool disposing)
        {
            if (disposing)
                await DisposeAsync();
        }

        private async ValueTask DisposeAsync()
        {
            if (!disposed)
            {
                await DbTransaction.DisposeAsync();
                await connection.DisposeAsync();
                DbTransaction = null;
                connection = null;
                disposed = true;
            }
        }
    }
}