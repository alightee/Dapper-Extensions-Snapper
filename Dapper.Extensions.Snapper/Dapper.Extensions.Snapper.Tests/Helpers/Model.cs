using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.Snapper.Tests.Helpers
{
    internal class Model
    {
        public int Id_Int { get; set; }
        public int? Id_Int_Nullable { get; set; }
        public long Id_Long { get; set; }
        public long? Id_Long_Nullable { get; set; }
        public string StringData { get; set; }
        public bool Flag { get; set; }
        public bool? Flag_Nullable { get; set; }
        public DateTime Date { get; set; }
        public DateTime? Date_Nullable { get; set; }
        public Guid Guid { get; set; }
        public Guid? Guid_Nullable { get; set; }

        [Write(false)]
        public object NotAMappedProperty { get; set; }

        [Write(true)]
        [AutoMapper.Configuration.Annotations.Ignore]
        public object AMappedPropertyWithMultipleAttributes { get; set; }

    }
}
