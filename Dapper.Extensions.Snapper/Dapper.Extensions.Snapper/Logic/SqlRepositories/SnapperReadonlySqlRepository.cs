using Dapper.Extensions.Snapper.AbstractModels;
using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Dapper.Extensions.Snapper.Logic
{
    internal abstract class SnapperReadonlySqlRepository<TModel, TKey> : SnapperReadonlyRepository<SqlConnection, TModel, TKey>
       where TModel : class, IDatabaseModel<TKey>
    {
        public SnapperReadonlySqlRepository(IDatabaseConnectionFactory<SqlConnection> connectionManager, ICache cache) : base(connectionManager, cache)
        {
        }
    }
}
