using System;
using System.Collections.Generic;
using System.Text;
using Dapper.Extensions.Snapper.AbstractModels;
using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using MySql.Data.MySqlClient;

namespace Dapper.Extensions.Snapper.Logic
{
    internal abstract class SnapperReadonlyMySqlRepository<TModel, TKey> : SnapperReadonlyRepository<MySqlConnection, TModel, TKey>
       where TModel : class, IDatabaseModel<TKey>
    {
        public SnapperReadonlyMySqlRepository(IDatabaseConnectionFactory<MySqlConnection> connectionManager, ICache cache) : base(connectionManager, cache)
        {
        }
    }
}
