using Dapper.Extensions.Snapper.AbstractModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.Extensions.Snapper.Logic.Interfaces
{
    public interface ISmartRepository<T> : IRepository<T, long> where T : SnapperDatabaseTableModel
    {
        bool AddUpdateDelete(IEnumerable<T> modelsToAddOrUpdate, IEnumerable<long> modelIdsToDelete, QueryExecutionOptions options = null);
        Task<bool> AddUpdateDeleteAsync(IEnumerable<T> modelsToAddOrUpdate, IEnumerable<long> modelIdsToDelete, QueryExecutionOptions options = null);
        bool AddUpdateDeleteWhere(Expression<Func<T, bool>> where, IEnumerable<T> models, QueryExecutionOptions options = null);
        Task<bool> AddUpdateDeleteWhereAsync(Expression<Func<T, bool>> where, IEnumerable<T> models, QueryExecutionOptions options = null);
    }
}
