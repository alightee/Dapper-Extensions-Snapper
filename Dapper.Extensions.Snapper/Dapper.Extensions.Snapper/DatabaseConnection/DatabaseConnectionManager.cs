using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Dapper.Extensions.Snapper.DatabaseConnection
{
    public abstract class DatabaseConnectionManager<TDbConnection> : IDatabaseConnectionFactory<TDbConnection>, ITransactionManager<TDbConnection> where TDbConnection : DbConnection
    {
        private UnitOfWork<TDbConnection> UnitOfWork { get; set; }
        private IHttpContextAccessor HttpContextAccessor { get; }
        public string ConnectionString { get; private set; }
        public int TransactionCount { get; private set; } = 0;

        public DatabaseConnectionManager(IHttpContextAccessor httpContextAccessor, string connectionString)
        {
            HttpContextAccessor = httpContextAccessor;
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

                // in global exception handling, we will check if this item exists in the context and then
                //  using Instractucture.Interfaces.ITransactionRollBack we will roll it back

                //TODO: change with params - Midlleware handling exception
                //var httpContextItems = HttpContextAccessor.HttpContext.Items;
                //if (httpContextItems.ContainsKey(ApplicationConstants.HttpContextItemKey_CurrentTransaction))
                //    httpContextItems.Remove(ApplicationConstants.HttpContextItemKey_CurrentTransaction);

                //httpContextItems.Add(ApplicationConstants.HttpContextItemKey_CurrentTransaction, UnitOfWork);
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
