using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Dapper.Extensions.Snapper.DatabaseConnection
{
    public sealed class UnitOfWork<TDbConnection> : ISnapperDatabaseTransaction<TDbConnection>, IDatabaseConnection<TDbConnection> where TDbConnection : DbConnection
    {
        internal DbTransaction Transaction { get; private set; }
        public TDbConnection Connection { get; private set; }
        public bool IsDisposed { get; private set; } = false;
        public bool TransactionFinished { get; private set; } = true;
        public int TransactionCount { get; private set; } = 0;

        public UnitOfWork(TDbConnection connection)
        {
            Connection = connection;
        }

        internal void StartTransaction()
        {
            if (Transaction == null || TransactionFinished)
            {
                Transaction = Connection.BeginTransaction();
                TransactionFinished = false;
            }
            TransactionCount++;
        }

        public void RollbackTransaction()
        {
            if (TransactionFinished)
                return;

            Transaction.Rollback();
            Transaction.Dispose();
            TransactionFinished = true;
        }

        public void CommitTransaction(bool rollbackOnError = true)
        {
            if (TransactionFinished)
                return;

            if (TransactionCount == 1)
            {
                try
                {
                    Transaction.Commit();
                    TransactionFinished = true;
                }
                catch
                {
                    if (rollbackOnError)
                        RollbackTransaction();
                    throw;
                }
            }
            else
                TransactionCount--;
        }

        public void CloseConnection()
        {
            if (TransactionCount <= 0)
            {
                Connection.Close();
                Connection.Dispose();
                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            CommitTransaction();
            CloseConnection();
        }
    }
}
