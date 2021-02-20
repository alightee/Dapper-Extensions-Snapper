using Dapper;
using Dapper.Contrib.Extensions;
using Dapper.Extensions.Snapper.AbstractModels;
using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using Dapper.Extensions.Snapper.Helpers.SqlCompiler;
using Dapper.Extensions.Snapper.Logic.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.Extensions.Snapper.Logic
{
    public class SnapperRepository<TDbConnection, TModel, TKey> : SnapperReadonlyRepository<TDbConnection, TModel, TKey>, IRepository<TModel, TKey>
        where TModel : class, IDatabaseModel<TKey>, new()
        where TDbConnection : DbConnection
    {
        public SnapperRepository(IDatabaseConnectionFactory<TDbConnection> connectionManager, ICache cache) : base(connectionManager, cache)
        {
        }

        public virtual TKey Add(TModel model, QueryExecutionOptions options = null)
        {
            using (var unitOfWork = ConnectionManager.Connect())
            {
                unitOfWork.Connection.Insert(model, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);

                return model.Id;
            }
        }

        public virtual async Task<TKey> AddAsync(TModel model, QueryExecutionOptions options = null)
        {
            using (var unitOfWork = ConnectionManager.Connect())
            {
                await unitOfWork.Connection.InsertAsync(model, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);

                return model.Id;
            }
        }

        public virtual bool Add(IEnumerable<TModel> models, QueryExecutionOptions options = null)
        {
            foreach (var model in models)
            {
                Add(model, options);
            }
            return true;
        }

        public virtual async Task<bool> AddAsync(IEnumerable<TModel> models, QueryExecutionOptions options = null)
        {
            foreach (var model in models)
            {
                await AddAsync(model, options);
            }
            return true;
        }

        public virtual bool Delete(TKey id, QueryExecutionOptions options = null)
        {
            using (var unitOfWork = ConnectionManager.Connect())
            {
                if (unitOfWork.Connection.Delete(new TModel { Id = id }, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual async Task<bool> DeleteAsync(TKey id, QueryExecutionOptions options = null)
        {
            using (var unitOfWork = ConnectionManager.Connect())
            {
                if (await unitOfWork.Connection.DeleteAsync(new TModel { Id = id }, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual bool Delete(IEnumerable<TKey> ids, QueryExecutionOptions options = null)
        {
            foreach (var id in ids)
            {
                Delete(id, options);
            }
            return true;
        }

        public virtual async Task<bool> DeleteAsync(IEnumerable<TKey> ids, QueryExecutionOptions options = null)
        {
            foreach (var id in ids)
            {
                await DeleteAsync(id, options);
            }
            return true;
        }

        public bool DeleteWhere(Expression<Func<TModel, bool>> where, QueryExecutionOptions options = null)
        {
            var (whereClauseSql, whereClauseParameters) = WhereClauseCompiler.ToSql(where, language: WhereClauseCompiler.Language.MYSQL);
            var parameters = new DynamicParameters(whereClauseParameters);

            using (var unitOfWork = ConnectionManager.Connect())
            {
                var rowsDeleted = unitOfWork.Connection.Execute($"DELETE FROM {TableName} WHERE {whereClauseSql}", parameters, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);

                return rowsDeleted >= 0;
            }
        }

        public async Task<bool> DeleteWhereAsync(Expression<Func<TModel, bool>> where, QueryExecutionOptions options = null)
        {
            var (whereClauseSql, whereClauseParameters) = WhereClauseCompiler.ToSql(where, language: WhereClauseCompiler.Language.MYSQL);
            var parameters = new DynamicParameters(whereClauseParameters);

            using (var unitOfWork = ConnectionManager.Connect())
            {
                var rowsDeleted = await unitOfWork.Connection.ExecuteAsync($"DELETE FROM {TableName} WHERE {whereClauseSql}", parameters, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);

                return rowsDeleted >= 0;
            }
        }

        public virtual bool Update(TModel model, QueryExecutionOptions options = null)
        {
            using (var unitOfWork = ConnectionManager.Connect())
            {
                return unitOfWork.Connection.Update(model, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);
            }
        }

        public virtual async Task<bool> UpdateAsync(TModel model, QueryExecutionOptions options = null)
        {
            using (var unitOfWork = ConnectionManager.Connect())
            {
                return await unitOfWork.Connection.UpdateAsync(model, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);
            }
        }

        public virtual bool Update(IEnumerable<TModel> models, QueryExecutionOptions options = null)
        {
            bool result = true;
            foreach (var model in models)
            {
                if (result)
                    result = result && Update(model, options);
            }
            return result;
        }

        public virtual async Task<bool> UpdateAsync(IEnumerable<TModel> models, QueryExecutionOptions options = null)
        {
            bool result = true;
            foreach (var model in models)
            {
                if (result)
                    result = result && await UpdateAsync(model, options);
            }
            return result;
        }
    }
}
