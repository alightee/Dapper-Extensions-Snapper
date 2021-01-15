using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Dapper.Extensions.Snapper.DatabaseConnection
{
    class SnapperDatabaseConnection<TDbConnection> : IDatabaseConnection<TDbConnection> where TDbConnection : DbConnection
    {
        private UnitOfWork<TDbConnection> UnitOfWork { get; }
        public TDbConnection Connection => UnitOfWork.Connection;

        public SnapperDatabaseConnection(UnitOfWork<TDbConnection> unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        public void Dispose()
        {
            UnitOfWork.CloseConnection();
        }
    }
}
