using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using Dapper.Extensions.Snapper.Logic;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.Extensions.Snapper.Tests.Helpers
{ 
    internal class SnapperDatabaseRepositoryHelper<TDbConnection> : SnapperBaseRepository<TDbConnection>
        where TDbConnection : DbConnection
    {
        protected override string CacheKey { get; }

        public SnapperDatabaseRepositoryHelper(IDatabaseConnectionFactory<TDbConnection> connectionManager, ICache cache) : base(connectionManager, cache)
        {
            CacheKey = "Model" + new Random().Next();
        }

        public string ExposeGetTableNameMethod<T>()
        {
            return GetTableName<T>();
        }

        public string ExposeGetColumnName<T>(Expression<Func<T, object>> selector)
        {
            return GetColumnName<T>(selector);
        }

        public string ExposeDefineQuery(string cacheKeySuffix, string query)
        {
            return DefineQuery(cacheKeySuffix, query);
        }
    }
}
