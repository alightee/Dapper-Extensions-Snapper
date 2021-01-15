using Dapper.Extensions.Snapper.AbstractModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions.Snapper.Logic.Interfaces
{
    public interface IReadonlyRepository<T, TKey> where T : IDatabaseModel<TKey>
    {
        List<T> GetAll(QueryExecutionOptions options = null);
        Task<List<T>> GetAllAsync(QueryExecutionOptions options = null);
        T GetById(TKey id, QueryExecutionOptions options = null);
        Task<T> GetByIdAsync(TKey id, QueryExecutionOptions options = null);
        List<T> GetWhere(Expression<Func<T, bool>> where, QueryExecutionOptions options = null);
        Task<List<T>> GetWhereAsync(Expression<Func<T, bool>> where, QueryExecutionOptions options = null);
    }
}
