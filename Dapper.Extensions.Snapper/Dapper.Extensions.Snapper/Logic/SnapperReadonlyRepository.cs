using Dapper;
using Dapper.Extensions.Snapper.AbstractModels;
using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using Dapper.Extensions.Snapper.Helpers.SqlCompiler;
using Dapper.Extensions.Snapper.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Dapper.Contrib.Extensions;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace Dapper.Extensions.Snapper.Logic
{
    public abstract class SnapperReadonlyRepository<TDbConnection, TModel, TKey> : SnapperBaseRepository<TDbConnection>, IReadonlyRepository<TModel, TKey>
        where TModel : class, IDatabaseModel<TKey>
        where TDbConnection : DbConnection
    {
        protected override string CacheKey { get; }
        protected string TableName { get; }
        public SnapperReadonlyRepository(IDatabaseConnectionFactory<TDbConnection> connectionManager, ICache cache) : base(connectionManager, cache)
        {
            CacheKey = typeof(TModel).FullName;
            TableName = GetTableName<TModel>();
        }

        public List<TModel> GetAll(QueryExecutionOptions options = null)
        {
            return Cache.CacheExecution(() =>
            {
                using (var unitOfWork = ConnectionManager.Connect())
                {
                    return unitOfWork.Connection.GetAll<TModel>(ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds).ToList();
                }
            },
            $"{CacheKey}_All",
            options?.UseCache ?? false,
            DefaultCacheExpirationInSeconds);
        }

        public async Task<List<TModel>> GetAllAsync(QueryExecutionOptions options = null)
        {
            return await Cache.CacheAsyncExecution(async () =>
            {
                using (var unitOfWork = ConnectionManager.Connect())
                {
                    return (await unitOfWork.Connection.GetAllAsync<TModel>(ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds)).ToList();
                }
            },
            $"{CacheKey}_All",
            options?.UseCache ?? false,
            DefaultCacheExpirationInSeconds);
        }

        public TModel GetById(TKey id, QueryExecutionOptions options = null)
        {
            return Cache.CacheExecution(() =>
            {
                using (var unitOfWork = ConnectionManager.Connect())
                {
                    return unitOfWork.Connection.Get<TModel>(id, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);
                }
            },
            $"{CacheKey}_{id}",
            options?.UseCache ?? false,
            DefaultCacheExpirationInSeconds);
        }

        public Task<TModel> GetByIdAsync(TKey id, QueryExecutionOptions options = null)
        {
            return Cache.CacheAsyncExecution(() =>
            {
                using (var unitOfWork = ConnectionManager.Connect())
                {
                    return unitOfWork.Connection.GetAsync<TModel>(id, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);
                }
            },
            $"{CacheKey}_{id}",
            options?.UseCache ?? false,
            DefaultCacheExpirationInSeconds);
        }

        public List<TModel> GetWhere(Expression<Func<TModel, bool>> where, QueryExecutionOptions options = null)
        {
            var limit = options?.ResultLimit;
            if (limit.HasValue && limit.Value <= 0)
                return new List<TModel>();
            var columnName = options?.ColumnName;
            var cacheKey = $"{CacheKey}_WHERE{(limit.HasValue ? $"_LIMIT_{limit}" : "")}__{options?.CacheKeySuffix}";

            return Cache.CacheExecution(() =>
            {
                var language = GetCurrentLanguage();
                var (whereClauseSql, whereClauseParameters) = WhereClauseCompiler.ToSql(where, language: language);
                var parameters = new DynamicParameters(whereClauseParameters);

                var limitSql = limit.HasValue 
                    ? (language == WhereClauseCompiler.Language.MYSQL) ? $" LIMIT {limit}"  : $" TOP {limit}"
                    : "";
                var orderByColumnSql = String.IsNullOrEmpty(columnName) ? "" : $"ORDER BY {columnName}";

                using (var unitOfWork = ConnectionManager.Connect())
                {
                    if(language.Equals(WhereClauseCompiler.Language.MYSQL))
                        return unitOfWork.Connection.Query<TModel>($"SELECT * FROM `{TableName}` WHERE {whereClauseSql}{orderByColumnSql}{limitSql} ", parameters, ConnectionManager.GetCurrentTransaction(), commandTimeout: options?.TimeoutInSeconds).ToList();
                    else
                        return unitOfWork.Connection.Query<TModel>($"SELECT {limitSql} * FROM {TableName} WHERE {whereClauseSql}{orderByColumnSql} ", parameters, ConnectionManager.GetCurrentTransaction(), commandTimeout: options?.TimeoutInSeconds).ToList();
                }
            },
            cacheKey,
            options?.UseCache ?? false,
            DefaultCacheExpirationInSeconds);
        }

        private WhereClauseCompiler.Language GetCurrentLanguage()
        {
            if (typeof(TDbConnection).IsAssignableFrom(typeof(MySqlConnection)))
            {
                return WhereClauseCompiler.Language.MYSQL;
            }
            else
            {
                return WhereClauseCompiler.Language.TSQL;
            }
        }

        public async Task<List<TModel>> GetWhereAsync(Expression<Func<TModel, bool>> where, QueryExecutionOptions options = null)
        {
            var limit = options?.ResultLimit;
            if (limit.HasValue && limit.Value <= 0)
                return new List<TModel>();
            var columnName = options?.ColumnName;
            var cacheKey = $"{CacheKey}_WHERE{(limit.HasValue ? $"_LIMIT_{limit}" : "")}__{options?.CacheKeySuffix}";

            return await Cache.CacheAsyncExecution(async () =>
            {
                var language = GetCurrentLanguage();
                var (whereClauseSql, whereClauseParameters) = WhereClauseCompiler.ToSql(where, language: language);
                var parameters = new DynamicParameters(whereClauseParameters);

                var limitSql = limit.HasValue
                 ? (language == WhereClauseCompiler.Language.MYSQL) ? $" LIMIT {limit}" : $" TOP {limit}"
                 : "";
                var orderByColumnSql = String.IsNullOrEmpty(columnName) ? "" : $"ORDER BY {columnName}";

                using (var unitOfWork = ConnectionManager.Connect())
                {
                    Task<IEnumerable<TModel>> queryTask;
                    if (language.Equals(WhereClauseCompiler.Language.MYSQL))
                        queryTask = unitOfWork.Connection.QueryAsync<TModel>($"SELECT * FROM `{TableName}` WHERE {whereClauseSql}{orderByColumnSql}{limitSql} ", parameters, ConnectionManager.GetCurrentTransaction(), commandTimeout: options?.TimeoutInSeconds);
                    else
                        queryTask = unitOfWork.Connection.QueryAsync<TModel>($"SELECT {limitSql} * FROM {TableName} WHERE {whereClauseSql}{orderByColumnSql} ", parameters, ConnectionManager.GetCurrentTransaction(), commandTimeout: options?.TimeoutInSeconds);
                    
                    return (await queryTask).ToList();
                }
            },
            cacheKey,
            options?.UseCache ?? false,
            DefaultCacheExpirationInSeconds);
        }

        protected string GetColumnName(Expression<Func<TModel, object>> propertySelector)
        {
            return GetColumnName<TModel>(propertySelector);
        }
    }

}
