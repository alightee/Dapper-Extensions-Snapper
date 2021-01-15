using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.Snapper.AbstractModels
{
    public interface IDatabaseModel<TKey>
    {
        TKey Id { get; set; }
    }
}
