using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.Snapper.AbstractModels
{
    public class QueryExecutionOptions
    {
        public bool UseCache { get; set; } = false;
        public int? TimeoutInSeconds { get; set; } = null;
        public string CacheKeySuffix { get; set; } = "";
        public int? ResultLimit { get; set; } = null;
        public string ColumnName { get; set; } = "";
    }
}
