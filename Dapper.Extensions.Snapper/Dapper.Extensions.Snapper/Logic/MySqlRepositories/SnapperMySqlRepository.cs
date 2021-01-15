using Dapper.Extensions.Snapper.AbstractModels;
using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.Snapper.Logic
{
    internal class SnapperMySqlRepository<TModel, TKey>: SnapperRepository<MySqlConnection, TModel, TKey>
        where TModel : class, IDatabaseModel<TKey>, new()
    {
        public SnapperMySqlRepository(IDatabaseConnectionFactory<MySqlConnection> connectionManager, ICache cache) : base(connectionManager, cache)
        {
        }
    }
}
