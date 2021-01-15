using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.Snapper.DatabaseConnection
{
    public interface ITransactionRollBack
    {
        void RollbackTransaction();
    }
}
