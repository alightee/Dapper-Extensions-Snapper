using Dapper.Extensions.Snapper.AbstractModels;
using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using Dapper.Extensions.Snapper.Logic;
using Dapper.Extensions.Snapper.Logic.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace Dapper.Extensions.Snapper.ServicesRegistration
//namespace Microsoft.Extensions.DependencyInjection
{
    public static class SnapperServiceCollection
    {
        public static IServiceCollection RegisterSnapperServicesForSql(this IServiceCollection services)
        {
            services.AddScoped<ICache, SnapperMemoryCache>();
            services.AddScoped(typeof(IRepository<,>), typeof(SnapperSqlRepository<,>));
            services.AddScoped(typeof(IReadonlyRepository<,>), typeof(SnapperSqlRepository<,>));
            services.AddScoped(typeof(ISmartRepository<>), typeof(SnapperSmartSqlRepository<>));

            return services;
        }



        public static IServiceCollection RegisterSnapperServicesForMySql(this IServiceCollection services)
        {
            services.AddScoped<ICache, SnapperMemoryCache>();
            services.AddScoped(typeof(IRepository<,>), typeof(SnapperMySqlRepository<,>));
            services.AddScoped(typeof(IReadonlyRepository<,>), typeof(SnapperMySqlRepository<,>));
            services.AddScoped(typeof(ISmartRepository<>), typeof(SnapperSmartMySqlRepository<>));
            return services;
        }

        public static IServiceCollection RegisterMySqlConnectionManager<T>(this IServiceCollection services)
            where T: DatabaseConnectionManager<MySqlConnection>
        {
            services.AddScoped(typeof(IDatabaseConnectionFactory<MySqlConnection>), typeof(T));
            services.AddScoped(typeof(ITransactionManager<MySqlConnection>), typeof(T));
            return services;
        }

        public static IServiceCollection RegisterSqlConnectionManager<T>(this IServiceCollection services)
            where T : DatabaseConnectionManager<SqlConnection>
        {
            services.AddScoped(typeof(IDatabaseConnectionFactory<SqlConnection>), typeof(T));
            services.AddScoped(typeof(ITransactionManager<SqlConnection>), typeof(T));
            return services;
        }
    }
}
