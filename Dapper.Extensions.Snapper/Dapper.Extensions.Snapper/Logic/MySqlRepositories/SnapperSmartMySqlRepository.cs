using Dapper.Extensions.Snapper.AbstractModels;
using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.Snapper.Logic
{
    internal class SnapperSmartMySqlRepository<TModel> : SnapperSmartRepository<MySqlConnection, TModel>
    where TModel : SnapperDatabaseTableModel, new()
    {
        public SnapperSmartMySqlRepository(IDatabaseConnectionFactory<MySqlConnection> connectionManager, ICache cache) : base(connectionManager, cache)
        {
        }
    }
}
