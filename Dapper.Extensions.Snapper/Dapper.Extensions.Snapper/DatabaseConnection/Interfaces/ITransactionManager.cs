using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Dapper.Extensions.Snapper.DatabaseConnection
{
    public interface ITransactionManager<TDbConnection> where TDbConnection : DbConnection
    {
        /// <summary>
        /// Not thread safe
        /// </summary>
        /// <returns></treturns>
        ISnapperDatabaseTransaction<TDbConnection> StartTransaction();
    }
}
