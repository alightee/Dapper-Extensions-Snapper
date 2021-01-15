using Dapper.Extensions.Snapper.Helpers.SqlCompiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dapper.Extensions.Snapper.Tests.SqlCompiler
{
    [TestClass]
    public class WhereClauseCompilerTests_MultipleClauses
    {
        public class OtherThing
        {
            public OtherThing2 ChildData { get; set; }
        }

        public class OtherThing2
        {
            public OtherThing3 ChildData { get; set; }
        }

        public class OtherThing3
        {
            public DateTime? Date { get; set; }
            public byte[] Guid { get; set; }
        }

        public class Model
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
        }

        private OtherThing CreateTestDataDto(DateTime? createdOn = null, byte[] guid = null)
        {
            return new OtherThing()
            {
                ChildData = new OtherThing2
                {
                    ChildData = new OtherThing3
                    {
                        Date = createdOn,
                        Guid = guid
                    }
                }
            };
        }

        [TestMethod]
        public void MultipleClauses_PreserveOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Long_Nullable == null && x.Flag_Nullable == null || x.Date_Nullable == null && x.StringData == null);

            //ASSERT
            Assert.AreEqual("(([Id_Long_Nullable] IS NULL) AND ([Flag_Nullable] IS NULL) OR ([Date_Nullable] IS NULL) AND ([StringData] IS NULL))", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
        }

        [TestMethod]
        public void MultipleClauses_PreserveOperationHierarchy_NoParentheses()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Long_Nullable == null || x.Flag_Nullable == null || x.Date_Nullable == null && x.StringData == null || x.Guid_Nullable == null);

            //ASSERT
            Assert.AreEqual("(([Id_Long_Nullable] IS NULL) OR ([Flag_Nullable] IS NULL) OR ([Date_Nullable] IS NULL) AND ([StringData] IS NULL) OR ([Guid_Nullable] IS NULL))", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
        }

        [TestMethod]
        public void MultipleClauses_PreserveOperationHierarchy_NoParentheses2()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Long_Nullable == null && x.Flag_Nullable == null && x.Date_Nullable == null);

            //ASSERT
            Assert.AreEqual("(([Id_Long_Nullable] IS NULL) AND ([Flag_Nullable] IS NULL) AND ([Date_Nullable] IS NULL))", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
        }

        [TestMethod]
        public void MultipleClauses_PreserveOperationHierarchy_WithParentheses()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => (x.Id_Long_Nullable == null || x.Flag_Nullable == null || x.Date_Nullable == null) && (x.StringData == null || x.Guid_Nullable == null));

            //ASSERT
            Assert.AreEqual("((([Id_Long_Nullable] IS NULL) OR ([Flag_Nullable] IS NULL) OR ([Date_Nullable] IS NULL)) AND (([StringData] IS NULL) OR ([Guid_Nullable] IS NULL)))", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
        }
    }
}
