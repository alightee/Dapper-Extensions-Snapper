using System.Data;
using System.Data.Common;

namespace Dapper.Extensions.Snapper.DatabaseConnection
{
    public abstract class DatabaseConnectionManager<TDbConnection> : IDatabaseConnectionFactory<TDbConnection>, ITransactionManager<TDbConnection> where TDbConnection : DbConnection
    {
        private UnitOfWork<TDbConnection> UnitOfWork { get; set; }
        public string ConnectionString { get; private set; }
        public int TransactionCount { get; private set; } = 0;

        public DatabaseConnectionManager(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected abstract TDbConnection NewConnection();

        public IDatabaseConnection<TDbConnection> Connect()
        {
            return new SnapperDatabaseConnection<TDbConnection>(CreateUnitOfWork());
        }

        /// <summary>
        /// Not thread safe
        /// </summary>
        /// <returns></treturns>
        public ISnapperDatabaseTransaction<TDbConnection> StartTransaction()
        {
            if (UnitOfWork == null || UnitOfWork.IsDisposed)
            {
                UnitOfWork = CreateUnitOfWork();
            }

            UnitOfWork.StartTransaction();
            return UnitOfWork;
        }

        public IDbTransaction GetCurrentTransaction()
        {
            if (UnitOfWork == null || UnitOfWork.IsDisposed)
            {
                return null;
            }
            return UnitOfWork.Transaction;
        }

        private UnitOfWork<TDbConnection> CreateUnitOfWork()
        {
            if (UnitOfWork == null || UnitOfWork.IsDisposed)
            {
                return new UnitOfWork<TDbConnection>(NewConnection());
            }

            return UnitOfWork;
        }
    }
}
