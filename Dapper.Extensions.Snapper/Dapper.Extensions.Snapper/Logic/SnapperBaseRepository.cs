using Dapper.Contrib.Extensions;
using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using System;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;

namespace Dapper.Extensions.Snapper.Logic
{
    public abstract class SnapperBaseRepository<TDbConnection>
        where TDbConnection : DbConnection
    {
        protected ICache Cache { get; }
        protected int DefaultCacheExpirationInSeconds { get; }
        protected abstract string CacheKey { get; }
        protected IDatabaseConnectionFactory<TDbConnection> ConnectionManager { get; set; }

        public SnapperBaseRepository(IDatabaseConnectionFactory<TDbConnection> connectionManager, ICache cache)
        {
            Cache = cache;
            ConnectionManager = connectionManager;
            DefaultCacheExpirationInSeconds = 600;
        }

        protected string GetColumnName<T>(Expression<Func<T, object>> selector)
        {
            var propertySelectorExpression = selector.Body;
            if (propertySelectorExpression is UnaryExpression && (propertySelectorExpression as UnaryExpression).NodeType == ExpressionType.Convert)
            {
                propertySelectorExpression = (propertySelectorExpression as UnaryExpression).Operand;
            }

            if (propertySelectorExpression is MemberExpression && (propertySelectorExpression as MemberExpression).Member is PropertyInfo)
            {
                var propertyInfo = (PropertyInfo)((MemberExpression)propertySelectorExpression).Member;
                var propertyName = propertyInfo.Name;

                var cacheKey = $"COLUMNNAME_{typeof(T).FullName}_{propertyName}";
                var cachedValue = Cache.Get<string>(cacheKey);
                if (cachedValue != default(string))
                    return cachedValue;

                var propertyAttributes = propertyInfo.GetCustomAttributes(true);
                foreach (var attr in propertyAttributes)
                {
                    var attrType = attr.GetType();
                    if (attrType.FullName != "Dapper.Contrib.Extensions.WriteAttribute")
                        continue;

                    var attrProperty = attrType.GetProperty("Write");
                    if (attrProperty != null)
                    {
                        var attrPropertyValue = attrProperty.GetValue(attr);
                        if (attrPropertyValue.GetType() == typeof(bool) && !(bool)attrPropertyValue)
                        {
                            throw new NotSupportedException("Must select a property that is mapped to one of the table columns.");
                        }
                    }
                }

                Cache.Add(cacheKey, propertyName);
                return propertyInfo.Name;
            }
            throw new NotSupportedException("Must select a property of the model");
        }

        /// <summary>
        /// https://dapper-tutorial.net/knowledge-base/43032797/get-table-name-from-tableattribute-dapper-contrib
        /// </summary>
        /// <param name="modelType">Defaults to <see cref="TModel"/> when not specified</param>
        /// <returns></returns>
        protected string GetTableName<T>()
        {
            return Cache.CacheExecution(() =>
            {
                if (SqlMapperExtensions.TableNameMapper != null)
                    return SqlMapperExtensions.TableNameMapper(typeof(T));

                string getTableName = "GetTableName";
                MethodInfo getTableNameMethod = typeof(SqlMapperExtensions).GetMethod(getTableName, BindingFlags.NonPublic | BindingFlags.Static);

                if (getTableNameMethod == null)
                    throw new ArgumentOutOfRangeException($"Method '{getTableName}' is not found in '{nameof(SqlMapperExtensions)}' class.");

                return getTableNameMethod.Invoke(null, new object[] { typeof(T) }) as string;
            }, $"TABLENAME_{typeof(T).FullName}");
        }

        protected string DefineQuery(string cacheKeySuffix, string query)
        {
            var key = $"QUERY_{CacheKey}_{cacheKeySuffix}";

            return Cache.CacheExecution(() => query, key);
        }
    }
}
