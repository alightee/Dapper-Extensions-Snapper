using System;
using System.Collections.Generic;
using System.Text;
using Dapper.Contrib.Extensions;


namespace Dapper.Extensions.Snapper.AbstractModels
{
    public abstract class SnapperDatabaseTableModel : IDatabaseModel<long>
    {
        [Key]
        public long Id { get; set; }
    }
}
