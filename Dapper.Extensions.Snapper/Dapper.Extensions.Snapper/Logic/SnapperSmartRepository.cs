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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions.Snapper.Logic
{

    public class SnapperSmartRepository<TDbConnection, TModel> : SnapperRepository<TDbConnection, TModel, long>, ISmartRepository<TModel> 
        where TModel : SnapperDatabaseTableModel, new()
        where TDbConnection : DbConnection
    {
        public SnapperSmartRepository(IDatabaseConnectionFactory<TDbConnection> connectionManager, ICache cache) : base(connectionManager, cache)
        {
        }

        public virtual bool AddUpdateDelete(IEnumerable<TModel> modelsToAddOrUpdate, IEnumerable<long> modelIdsToDelete, QueryExecutionOptions options = null)
        {
            modelsToAddOrUpdate = modelsToAddOrUpdate.ToList();

            var result = true;

            result = result && Add(modelsToAddOrUpdate.Where(x => x.Id <= 0), options);
            result = result && Update(modelsToAddOrUpdate.Where(x => x.Id > 0), options);
            result = result && Delete(modelIdsToDelete, options);

            return result;
        }

        public virtual async Task<bool> AddUpdateDeleteAsync(IEnumerable<TModel> modelsToAddOrUpdate, IEnumerable<long> modelIdsToDelete, QueryExecutionOptions options = null)
        {
            modelsToAddOrUpdate = modelsToAddOrUpdate.ToList();

            var results = await Task.WhenAll(
                AddAsync(modelsToAddOrUpdate.Where(x => x.Id <= 0), options),
                UpdateAsync(modelsToAddOrUpdate.Where(x => x.Id > 0), options),
                DeleteAsync(modelIdsToDelete, options));

            return results.All(x => x);
        }

        public bool AddUpdateDeleteWhere(Expression<Func<TModel, bool>> where, IEnumerable<TModel> models, QueryExecutionOptions options = null)
        {
            models = models.ToList();

            var modelsInsideDb = GetWhere(where, options);
            var idsOfModelsInideDb = modelsInsideDb.Select(x => x.Id).ToList();
            var idsOfModelsToSync = models.Where(x => x.Id > 0).Select(x => x.Id).ToList();

            var addOrUpdate = models.Where(x => x.Id <= 0 || idsOfModelsInideDb.Contains(x.Id));
            var delete = idsOfModelsInideDb.Where(x => !idsOfModelsToSync.Contains(x));

            return AddUpdateDelete(addOrUpdate, delete, options);
        }

        public Task<bool> AddUpdateDeleteWhereAsync(Expression<Func<TModel, bool>> where, IEnumerable<TModel> models, QueryExecutionOptions options = null)
        {
            models = models.ToList();

            var modelsInsideDb = GetWhere(where, options);
            var idsOfModelsInideDb = modelsInsideDb.Select(x => x.Id).ToList();
            var idsOfModelsToSync = models.Where(x => x.Id > 0).Select(x => x.Id).ToList();

            var addOrUpdate = models.Where(x => x.Id <= 0 || idsOfModelsInideDb.Contains(x.Id));
            var delete = idsOfModelsInideDb.Where(x => !idsOfModelsToSync.Contains(x));

            return AddUpdateDeleteAsync(addOrUpdate, delete, options);
        }

        #region Optimizations

        public override sealed long Add(TModel model, QueryExecutionOptions options = null)
        {
            using (var unitOfWork = ConnectionManager.Connect())
            {
                model.Id = unitOfWork.Connection.Insert(model, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);
                return model.Id;
            }
        }

        public override sealed async Task<long> AddAsync(TModel model, QueryExecutionOptions options = null)
        {
            using (var unitOfWork = ConnectionManager.Connect())
            {
                model.Id = await unitOfWork.Connection.InsertAsync(model, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);
                return model.Id;
            }
        }

        public override sealed bool Delete(IEnumerable<long> ids, QueryExecutionOptions options = null)
        {
            var (whereClauseSql, whereClauseParameters) = WhereClauseCompiler.ToSql<TModel>(x => ids.Contains(x.Id), language: WhereClauseCompiler.Language.MYSQL);
            var parameters = new DynamicParameters(whereClauseParameters);

            using (var unitOfWork = ConnectionManager.Connect())
            {
                var deletedRows = unitOfWork.Connection.Execute($"DELETE FROM {TableName} WHERE {whereClauseSql}", parameters, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);

                if (deletedRows > 0)
                    Cache.InvalidateByPrefix(CacheKey);

                return deletedRows >= 0;
            }
        }

        public override sealed async Task<bool> DeleteAsync(IEnumerable<long> ids, QueryExecutionOptions options = null)
        {
            var (whereClauseSql, whereClauseParameters) = WhereClauseCompiler.ToSql<TModel>(x => ids.Contains(x.Id), language: WhereClauseCompiler.Language.MYSQL);
            var parameters = new DynamicParameters(whereClauseParameters);

            using (var unitOfWork = ConnectionManager.Connect())
            {
                var deletedRows = await unitOfWork.Connection.ExecuteAsync($"DELETE FROM {TableName} WHERE {whereClauseSql}", parameters, ConnectionManager.GetCurrentTransaction(), options?.TimeoutInSeconds);

                if (deletedRows > 0)
                    Cache.InvalidateByPrefix(CacheKey);

                return deletedRows >= 0;
            }
        }

        #endregion
    }
}
