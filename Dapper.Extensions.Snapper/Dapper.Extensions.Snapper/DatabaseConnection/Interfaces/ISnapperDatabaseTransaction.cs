using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Dapper.Extensions.Snapper.DatabaseConnection
{

    public interface ISnapperDatabaseTransaction<TDbConnection> : ITransactionRollBack, IDisposable where TDbConnection : DbConnection
    {
        void CommitTransaction(bool revertOnException = true);
    }
}
