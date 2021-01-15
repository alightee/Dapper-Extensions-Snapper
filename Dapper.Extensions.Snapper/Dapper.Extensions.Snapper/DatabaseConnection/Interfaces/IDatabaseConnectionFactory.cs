using System.Data;
using System.Data.Common;

namespace Dapper.Extensions.Snapper.DatabaseConnection
{
    public interface IDatabaseConnectionFactory<TDbConnection> where TDbConnection : DbConnection
    {
        IDatabaseConnection<TDbConnection> Connect();
        IDbTransaction GetCurrentTransaction();
    }
}
