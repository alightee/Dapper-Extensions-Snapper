using System;
using System.Data.Common;

namespace Dapper.Extensions.Snapper.DatabaseConnection
{
    public interface IDatabaseConnection<TDbConnection> : IDisposable where TDbConnection  : DbConnection
    {
        TDbConnection Connection { get; }
    }
}
