using Dapper.Extensions.Snapper.Helpers.SqlCompiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Dapper.Extensions.Snapper.Tests.SqlCompiler
{
    [TestClass]
    public class WhereClauseCompilerTests_SingleClause
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
            public int Id { get; set; }
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

        public class ModelWithBase : ModelBase { }
        public class OtherClassWithSameBase : ModelBase { }
        public class ModelBase
        {
            public int Base_Id { get; set; }
        }

        private OtherThing CreateTestDataDto(DateTime? createdOn = null)
        {
            return new OtherThing()
            {
                ChildData = new OtherThing2
                {
                    ChildData = new OtherThing3
                    {
                        Date = createdOn,
                        Id = 1
                    }
                }
            };
        }

        #region NULL

        [TestMethod]
        public void NullableColumnEqualsNullConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Long_Nullable == null);
            var clause2 = WhereClauseCompiler.ToSql<Model>(x => x.Flag_Nullable == null);
            var clause3 = WhereClauseCompiler.ToSql<Model>(x => x.Date_Nullable == null);
            var clause4 = WhereClauseCompiler.ToSql<Model>(x => x.StringData == null);

            //ASSERT
            Assert.AreEqual("([Id_Long_Nullable] IS NULL)", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
            Assert.AreEqual("([Flag_Nullable] IS NULL)", clause2.Sql);
            Assert.AreEqual(0, clause2.Parameters.Count);
            Assert.AreEqual("([Date_Nullable] IS NULL)", clause3.Sql);
            Assert.AreEqual(0, clause3.Parameters.Count);
            Assert.AreEqual("([StringData] IS NULL)", clause4.Sql);
            Assert.AreEqual(0, clause4.Parameters.Count);
        }

        [TestMethod]
        public void NullableColumnEqualsNullConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => null == x.Id_Long_Nullable);
            var clause2 = WhereClauseCompiler.ToSql<Model>(x => null == x.Flag_Nullable);
            var clause3 = WhereClauseCompiler.ToSql<Model>(x => null == x.Date_Nullable);
            var clause4 = WhereClauseCompiler.ToSql<Model>(x => null == x.StringData);

            //ASSERT
            Assert.AreEqual("([Id_Long_Nullable] IS NULL)", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
            Assert.AreEqual("([Flag_Nullable] IS NULL)", clause2.Sql);
            Assert.AreEqual(0, clause2.Parameters.Count);
            Assert.AreEqual("([Date_Nullable] IS NULL)", clause3.Sql);
            Assert.AreEqual(0, clause3.Parameters.Count);
            Assert.AreEqual("([StringData] IS NULL)", clause4.Sql);
            Assert.AreEqual(0, clause4.Parameters.Count);
        }

        [TestMethod]
        public void NullableDateTimeColumn_Equals_NullNullableDateTimeMember_NormalOrder()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto();

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Date_Nullable == clasWithDate.ChildData.ChildData.Date);

            //ASSERT
            Assert.AreEqual("([Date_Nullable] IS NULL)", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
        }

        [TestMethod]
        public void NullableDateTimeColumn_Equals_NullNullableDateTimeMember_ReversedOrder()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto();

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => clasWithDate.ChildData.ChildData.Date == x.Date_Nullable);

            //ASSERT
            Assert.AreEqual("([Date_Nullable] IS NULL)", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
        }

        #endregion

        #region Numbers

        [TestMethod]
        public void IntColumn_Equals_IntConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Int == 1);

            //ASSERT
            Assert.AreEqual("([Id_Int] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(1, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void IntColumn_Equals_IntConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => 1 == x.Id_Int);

            //ASSERT
            Assert.AreEqual("(@__P0 = [Id_Int])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(1, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void IntColumn_GreaterThan_IntConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Int > 1);

            //ASSERT
            Assert.AreEqual("([Id_Int] > @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(1, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void IntColumn_GreaterThan_IntConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => 1 > x.Id_Int);

            //ASSERT
            Assert.AreEqual("(@__P0 > [Id_Int])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(1, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void IntColumn_GreaterOrEqualThan_IntConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Int >= 1);

            //ASSERT
            Assert.AreEqual("([Id_Int] >= @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(1, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void IntColumn_GreaterOrEqualThan_IntConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => 1 >= x.Id_Int);

            //ASSERT
            Assert.AreEqual("(@__P0 >= [Id_Int])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(1, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void IntColumn_LessThan_IntConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Int < 1);

            //ASSERT
            Assert.AreEqual("([Id_Int] < @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(1, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void IntColumn_LessThan_IntConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => 1 < x.Id_Int);

            //ASSERT
            Assert.AreEqual("(@__P0 < [Id_Int])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(1, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void IntColumn_LessOrEqualThan_IntConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Int <= 1);

            //ASSERT
            Assert.AreEqual("([Id_Int] <= @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(1, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void IntColumn_LessOrEqualThan_IntConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => 1 <= x.Id_Int);

            //ASSERT
            Assert.AreEqual("(@__P0 <= [Id_Int])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(1, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void NullableIntColumn_Equals_IntConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Int_Nullable == 1);

            //ASSERT
            Assert.AreEqual("([Id_Int_Nullable] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(1, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableIntColumn_Equals_IntConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => 1 == x.Id_Int_Nullable);

            //ASSERT
            Assert.AreEqual("(@__P0 = [Id_Int_Nullable])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(1, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void LongColumn_Equals_IntConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Long == 1);

            //ASSERT
            Assert.AreEqual("([Id_Long] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(long), clause.Parameters["__P0"].GetType());
            Assert.AreEqual((long)1, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void LongColumn_Equals_IntConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => 1 == x.Id_Long);

            //ASSERT
            Assert.AreEqual("(@__P0 = [Id_Long])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(long), clause.Parameters["__P0"].GetType());
            Assert.AreEqual((long)1, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableLongColumn_Equals_IntConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Long_Nullable == 1);

            //ASSERT
            Assert.AreEqual("([Id_Long_Nullable] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(long), clause.Parameters["__P0"].GetType());
            Assert.AreEqual((long)1, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableLongColumn_Equals_IntConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => 1 == x.Id_Long_Nullable);

            //ASSERT
            Assert.AreEqual("(@__P0 = [Id_Long_Nullable])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(long), clause.Parameters["__P0"].GetType());
            Assert.AreEqual((long)1, clause.Parameters["__P0"]);
        }

        #endregion

        #region Booleans

        [TestMethod]
        public void BoolColumn_CheckTrue()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Flag);

            //ASSERT
            Assert.AreEqual("([Flag] = 1)", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
        }

        [TestMethod]
        public void BoolColumn_CheckFalse()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => !x.Flag);

            //ASSERT
            Assert.AreEqual("(NOT ([Flag] = 1))", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
        }

        [TestMethod]
        public void BoolColumn_Equals_BoolConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Flag == true);

            //ASSERT
            Assert.AreEqual("([Flag] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(true, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(bool), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void BoolColumn_Equals_BoolConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => true == x.Flag);

            //ASSERT
            Assert.AreEqual("(@__P0 = [Flag])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(true, clause.Parameters["__P0"]);
            Assert.AreEqual(typeof(bool), clause.Parameters["__P0"].GetType());
        }

        [TestMethod]
        public void NullableBoolColumn_Equals_BoolConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Flag_Nullable == true);

            //ASSERT
            Assert.AreEqual("([Flag_Nullable] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(bool), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(true, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableBoolColumn_Equals_BoolConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => true == x.Flag_Nullable);

            //ASSERT
            Assert.AreEqual("(@__P0 = [Flag_Nullable])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(bool), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(true, clause.Parameters["__P0"]);
        }

        #endregion

        #region Strings

        [TestMethod]
        public void StringColumn_Equals_StringConstant_NormalOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.StringData == "abc");

            //ASSERT
            Assert.AreEqual("([StringData] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(string), clause.Parameters["__P0"].GetType());
            Assert.AreEqual("abc", clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void StringColumn_Equals_StringConstant_ReversedOrder()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => "abc" == x.StringData);

            //ASSERT
            Assert.AreEqual("(@__P0 = [StringData])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(string), clause.Parameters["__P0"].GetType());
            Assert.AreEqual("abc", clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void StringColumn_Contains_StringConstant()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.StringData.Contains("abc"));

            //ASSERT
            Assert.AreEqual("([StringData] LIKE CONCAT('%',@__P0,'%'))", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(string), clause.Parameters["__P0"].GetType());
            Assert.AreEqual("abc", clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void StringConstant_Contains_StringColumn()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => "abc".Contains(x.StringData));

            //ASSERT
            Assert.AreEqual("(@__P0 LIKE CONCAT('%',[StringData],'%'))", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(string), clause.Parameters["__P0"].GetType());
            Assert.AreEqual("abc", clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void StringColumn_StartsWith_StringConstant()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.StringData.StartsWith("abc"));

            //ASSERT
            Assert.AreEqual("([StringData] LIKE CONCAT(@__P0,'%'))", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(string), clause.Parameters["__P0"].GetType());
            Assert.AreEqual("abc", clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void StringConstant_StartsWith_StringColumn()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => "abc".StartsWith(x.StringData));

            //ASSERT
            Assert.AreEqual("(@__P0 LIKE CONCAT([StringData],'%'))", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(string), clause.Parameters["__P0"].GetType());
            Assert.AreEqual("abc", clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void StringColumn_EndsWith_StringConstant()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.StringData.EndsWith("abc"));

            //ASSERT
            Assert.AreEqual("([StringData] LIKE CONCAT('%',@__P0))", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(string), clause.Parameters["__P0"].GetType());
            Assert.AreEqual("abc", clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void StringConstant_EndsWith_StringColumn()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => "abc".EndsWith(x.StringData));

            //ASSERT
            Assert.AreEqual("(@__P0 LIKE CONCAT('%',[StringData]))", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(string), clause.Parameters["__P0"].GetType());
            Assert.AreEqual("abc", clause.Parameters["__P0"]);
        }

        #endregion

        #region DateTime

        [TestMethod]
        public void DateTimeColumn_Equals_DateTimeVariable_NormalOrder()
        {
            //ARRANGE
            var date = DateTime.Now;

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Date == date);

            //ASSERT
            Assert.AreEqual("([Date] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void DateTimeColumn_Equals_DateTimeVariable_ReversedOrder()
        {
            //ARRANGE
            var date = DateTime.Now;

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => date == x.Date);

            //ASSERT
            Assert.AreEqual("(@__P0 = [Date])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableDateTimeColumn_Equals_NullableDateTimeMember_NormalOrder()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto(DateTime.Now);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Date_Nullable == clasWithDate.ChildData.ChildData.Date);

            //ASSERT
            Assert.AreEqual("([Date_Nullable] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(clasWithDate.ChildData.ChildData.Date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableDateTimeColumn_Equals_NullableDateTimeMember_ReversedOrder()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto(DateTime.Now);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => clasWithDate.ChildData.ChildData.Date == x.Date_Nullable);

            //ASSERT
            Assert.AreEqual("(@__P0 = [Date_Nullable])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(clasWithDate.ChildData.ChildData.Date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableDateTimeColumn_GreaterThan_NullableDateTimeMember()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto(DateTime.Now);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Date_Nullable > clasWithDate.ChildData.ChildData.Date);

            //ASSERT
            Assert.AreEqual("([Date_Nullable] > @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(clasWithDate.ChildData.ChildData.Date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableDateTimeMember_GreaterThan_NullableDateTimeColumn()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto(DateTime.Now);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => clasWithDate.ChildData.ChildData.Date > x.Date_Nullable);

            //ASSERT
            Assert.AreEqual("(@__P0 > [Date_Nullable])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(clasWithDate.ChildData.ChildData.Date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableDateTimeColumn_GreaterOrEqualThan_NullableDateTimeMember()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto(DateTime.Now);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Date_Nullable >= clasWithDate.ChildData.ChildData.Date);

            //ASSERT
            Assert.AreEqual("([Date_Nullable] >= @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(clasWithDate.ChildData.ChildData.Date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableDateTimeMember_GreaterOrEqualThan_NullableDateTimeColumn()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto(DateTime.Now);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => clasWithDate.ChildData.ChildData.Date >= x.Date_Nullable);

            //ASSERT
            Assert.AreEqual("(@__P0 >= [Date_Nullable])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(clasWithDate.ChildData.ChildData.Date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableDateTimeColumn_LessThan_NullableDateTimeMember()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto(DateTime.Now);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Date_Nullable < clasWithDate.ChildData.ChildData.Date);

            //ASSERT
            Assert.AreEqual("([Date_Nullable] < @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(clasWithDate.ChildData.ChildData.Date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableDateTimeMember_LessThan_NullableDateTimeColumn()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto(DateTime.Now);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => clasWithDate.ChildData.ChildData.Date < x.Date_Nullable);

            //ASSERT
            Assert.AreEqual("(@__P0 < [Date_Nullable])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(clasWithDate.ChildData.ChildData.Date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableDateTimeColumn_LessOrEqualThan_NullableDateTimeMember()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto(DateTime.Now);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Date_Nullable <= clasWithDate.ChildData.ChildData.Date);

            //ASSERT
            Assert.AreEqual("([Date_Nullable] <= @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(clasWithDate.ChildData.ChildData.Date, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableDateTimeMember_LessOrEqualThan_NullableDateTimeColumn()
        {
            //ARRANGE
            var clasWithDate = CreateTestDataDto(DateTime.Now);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => clasWithDate.ChildData.ChildData.Date <= x.Date_Nullable);

            //ASSERT
            Assert.AreEqual("(@__P0 <= [Date_Nullable])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(clasWithDate.ChildData.ChildData.Date, clause.Parameters["__P0"]);
        }

        #endregion

        #region GUIDs

        [TestMethod]
        public void GuidColumn_Equals_Guid_NormalOrder()
        {
            //ARRANGE
            var guid = Guid.NewGuid();

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Guid == guid);

            //ASSERT
            Assert.AreEqual("([Guid] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(Guid), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(guid, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void GuidColumn_Equals_Guid_ReversedOrder()
        {
            //ARRANGE
            var guid = Guid.NewGuid();

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => guid == x.Guid);

            //ASSERT
            Assert.AreEqual("(@__P0 = [Guid])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(Guid), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(guid, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableGuidColumn_Equals_Guid_NormalOrder()
        {
            //ARRANGE
            var guid = Guid.NewGuid();

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Guid_Nullable == guid);

            //ASSERT
            Assert.AreEqual("([Guid_Nullable] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(Guid), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(guid, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void NullableGuidColumn_Equals_Guid_ReversedOrder()
        {
            //ARRANGE
            var guid = Guid.NewGuid();

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => guid == x.Guid_Nullable);

            //ASSERT
            Assert.AreEqual("(@__P0 = [Guid_Nullable])", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(Guid), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(guid, clause.Parameters["__P0"]);
        }

        #endregion

        #region Arrays

        [TestMethod]
        public void IntColumn_In_IntArray()
        {
            //ARRANGE
            var array = new[] { 1, 2, 3, 4 };

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => array.Contains(x.Id_Int));

            //ASSERT
            Assert.AreEqual("([Id_Int] IN (@__P0, @__P1, @__P2, @__P3))", clause.Sql);
            Assert.AreEqual(4, clause.Parameters.Count);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(typeof(int), clause.Parameters["__P1"].GetType());
            Assert.AreEqual(typeof(int), clause.Parameters["__P2"].GetType());
            Assert.AreEqual(typeof(int), clause.Parameters["__P3"].GetType());
            Assert.AreEqual(array[0], clause.Parameters["__P0"]);
            Assert.AreEqual(array[1], clause.Parameters["__P1"]);
            Assert.AreEqual(array[2], clause.Parameters["__P2"]);
            Assert.AreEqual(array[3], clause.Parameters["__P3"]);
        }

        [TestMethod]
        public void StringColumn_In_StringArray()
        {
            //ARRANGE
            var array = new[] { "a", "b", "c", "d" };

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => array.Contains(x.StringData));

            //ASSERT
            Assert.AreEqual("([StringData] IN (@__P0, @__P1, @__P2, @__P3))", clause.Sql);
            Assert.AreEqual(4, clause.Parameters.Count);
            Assert.AreEqual(typeof(string), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(typeof(string), clause.Parameters["__P1"].GetType());
            Assert.AreEqual(typeof(string), clause.Parameters["__P2"].GetType());
            Assert.AreEqual(typeof(string), clause.Parameters["__P3"].GetType());
            Assert.AreEqual(array[0], clause.Parameters["__P0"]);
            Assert.AreEqual(array[1], clause.Parameters["__P1"]);
            Assert.AreEqual(array[2], clause.Parameters["__P2"]);
            Assert.AreEqual(array[3], clause.Parameters["__P3"]);
        }

        [TestMethod]
        public void DateTimeColumn_In_DateTimeArray()
        {
            //ARRANGE
            var array = new[] { DateTime.Now, DateTime.Now, DateTime.Now, DateTime.Now };

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => array.Contains(x.Date));

            //ASSERT
            Assert.AreEqual("([Date] IN (@__P0, @__P1, @__P2, @__P3))", clause.Sql);
            Assert.AreEqual(4, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P1"].GetType());
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P2"].GetType());
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P3"].GetType());
            Assert.AreEqual(array[0], clause.Parameters["__P0"]);
            Assert.AreEqual(array[1], clause.Parameters["__P1"]);
            Assert.AreEqual(array[2], clause.Parameters["__P2"]);
            Assert.AreEqual(array[3], clause.Parameters["__P3"]);
        }

        [TestMethod]
        public void NullableDateTimeColumn_In_NullableDateTimeArrayWithNullValue()
        {
            //ARRANGE
            var array = new DateTime?[] { DateTime.Now, null, DateTime.Now, DateTime.Now };

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => array.Contains(x.Date_Nullable));

            //ASSERT
            Assert.AreEqual("([Date_Nullable] IN (@__P0, NULL, @__P1, @__P2))", clause.Sql);
            Assert.AreEqual(3, clause.Parameters.Count);
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P1"].GetType());
            Assert.AreEqual(typeof(DateTime), clause.Parameters["__P2"].GetType());
            Assert.AreEqual(array[0], clause.Parameters["__P0"]);
            Assert.AreEqual(array[1], null);
            Assert.AreEqual(array[2], clause.Parameters["__P1"]);
            Assert.AreEqual(array[3], clause.Parameters["__P2"]);
        }

        [TestMethod]
        public void IntColumn_In_EmptyIntArray()
        {
            //ARRANGE
            var intArray = new int[] { };

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => intArray.Contains(x.Id_Int));

            //ASSERT
            Assert.AreEqual("(1=0)", clause.Sql);
            Assert.AreEqual(0, clause.Parameters.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void IntPropertyFromOtherType_In_IntArray_NotSupported()
        {
            //ARRANGE
            var data = CreateTestDataDto();
            var intArray = new[] { 1, 2, 3, 4 };

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => intArray.Contains(data.ChildData.ChildData.Id));
        }

        #endregion

        #region NotSupported 

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void MethodCall_NotSupported()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Guid.GetType() == null);
        }

        #endregion

        #region Supported 

        [TestMethod]
        public void ModelWithBase_IdFromBaseComparison()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<ModelWithBase>(x => x.Base_Id == 1);

            //ASSERT
            Assert.AreEqual("([Base_Id] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(1, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void ModelWithBase_IdFromBaseComparison_OtherClassWithSameBase()
        {
            //ARRANGE
            var otherClass = new OtherClassWithSameBase { Base_Id = 1 };

            //ACT
            var clause = WhereClauseCompiler.ToSql<ModelWithBase>(x => x.Base_Id == otherClass.Base_Id);

            //ASSERT
            Assert.AreEqual("([Base_Id] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(1, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void Model_IdComparison_SameModelButInstanced()
        {
            //ARRANGE
            var instanced = new ModelWithBase { Base_Id = 1 };

            //ACT
            var clause = WhereClauseCompiler.ToSql<ModelWithBase>(x => x.Base_Id == instanced.Base_Id);

            //ASSERT
            Assert.AreEqual("([Base_Id] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(int), clause.Parameters["__P0"].GetType());
            Assert.AreEqual(1, clause.Parameters["__P0"]);
        }

        [TestMethod]
        public void Model_PropertyComparison_SameModelButInstanced()
        {
            //ARRANGE
            var instanced = new Model { Id_Int = 1, StringData = "bla"  };

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.StringData == instanced.StringData);

            //ASSERT
            Assert.AreEqual("([StringData] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(string), clause.Parameters["__P0"].GetType());
            Assert.AreEqual("bla", clause.Parameters["__P0"]);
        }

        #endregion
    }
}
