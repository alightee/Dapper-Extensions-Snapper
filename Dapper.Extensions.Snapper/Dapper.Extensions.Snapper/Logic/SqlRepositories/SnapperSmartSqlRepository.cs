using Dapper.Extensions.Snapper.AbstractModels;
using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Dapper.Extensions.Snapper.Logic
{

    internal class SnapperSmartSqlRepository<TModel> : SnapperSmartRepository<SqlConnection, TModel>
    where TModel : SnapperDatabaseTableModel, new()
    {
        public SnapperSmartSqlRepository(IDatabaseConnectionFactory<SqlConnection> connectionManager, ICache cache) : base(connectionManager, cache)
        {
        }
    }
}
