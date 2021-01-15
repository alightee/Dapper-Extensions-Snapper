using Dapper.Extensions.Snapper.AbstractModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions.Snapper.Logic.Interfaces
{
    public interface IRepository<T, TKey> : IReadonlyRepository<T, TKey> where T : IDatabaseModel<TKey>
    {
        /// <summary>
        /// This does not set the id value automatically to the id property of the model because it supports composite keys.
        /// <para>If your primary key is not composite then use the <see cref="ISmartDbTableRepository{T}"/></para>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        TKey Add(T model, QueryExecutionOptions options = null);

        /// <summary>
        /// This does not set the id value automatically to the id property of the model because it supports composite keys.
        /// <para>If your primary key is not composite then use the <see cref="ISmartDbTableRepository{T}"/></para>
        /// </summary>
        /// <param name="model"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        Task<TKey> AddAsync(T model, QueryExecutionOptions options = null);

        bool Add(IEnumerable<T> models, QueryExecutionOptions options = null);
        Task<bool> AddAsync(IEnumerable<T> models, QueryExecutionOptions options = null);

        bool Update(T model, QueryExecutionOptions options = null);
        Task<bool> UpdateAsync(T model, QueryExecutionOptions options = null);

        bool Update(IEnumerable<T> models, QueryExecutionOptions options = null);
        Task<bool> UpdateAsync(IEnumerable<T> models, QueryExecutionOptions options = null);

        bool Delete(TKey id, QueryExecutionOptions options = null);
        Task<bool> DeleteAsync(TKey id, QueryExecutionOptions options = null);

        bool Delete(IEnumerable<TKey> ids, QueryExecutionOptions options = null);
        Task<bool> DeleteAsync(IEnumerable<TKey> ids, QueryExecutionOptions options = null);

        bool DeleteWhere(Expression<Func<T, bool>> where, QueryExecutionOptions options = null);
        Task<bool> DeleteWhereAsync(Expression<Func<T, bool>> where, QueryExecutionOptions options = null);
    }
}
