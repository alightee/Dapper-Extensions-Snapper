using Dapper.Contrib.Extensions;
using Dapper.Extensions.Snapper.Helpers.SqlCompiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using AutoMapper.Configuration.Annotations;

namespace Dapper.Extensions.Snapper.Tests.SqlCompiler
{
    [TestClass]
    public class WhereClauseCompilerTests_Functionality
    {
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

            [Write(false)]
            public object NotAMappedProperty { get; set; }

            [Write(true)]
            [AutoMapper.Configuration.Annotations.Ignore]
            public object AMappedPropertyWithMultipleAttributes { get; set; }
        }

        [TestMethod]
        public void LanguageOptions_MySql()
        {
            //ARRANGE
            var language = WhereClauseCompiler.Language.MYSQL;

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Int == 1, language: language);

            //ASSERT
            Assert.AreEqual($"(`Id_Int` = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(int), clause.Parameters[$"__P0"].GetType());
            Assert.AreEqual(1, clause.Parameters[$"__P0"]);
        }

        [TestMethod]
        public void ParameterNames_DefaultParameterName()
        {
            //ARRANGE
            string defaultParamName = WhereClauseCompiler.ParameterName;

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Int == 1);

            //ASSERT
            Assert.AreEqual($"([Id_Int] = @{defaultParamName}0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(int), clause.Parameters[$"{defaultParamName}0"].GetType());
            Assert.AreEqual(1, clause.Parameters[$"{defaultParamName}0"]);
        }

        [TestMethod]
        public void ParameterNames_CustomParameterName()
        {
            //ARRANGE
            string customParamName = "customParam";

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.Id_Int == 1, customParamName);

            //ASSERT
            Assert.AreEqual($"([Id_Int] = @{customParamName}0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(int), clause.Parameters[$"{customParamName}0"].GetType());
            Assert.AreEqual(1, clause.Parameters[$"{customParamName}0"]);
        }

        [TestMethod]
        public void ParameterLimit_DefaultLimit_NotExceeded()
        {
            //ARRANGE
            var limit = WhereClauseCompiler.ParameterLimit;
            var array = Enumerable.Range(0, limit);
            var parameterListToExpect = string.Join(", ", Enumerable.Range(0, limit).Select(x => $"@{WhereClauseCompiler.ParameterName}{x}"));

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => array.Contains(x.Id_Int));

            //ASSERT
            Assert.AreEqual($"([Id_Int] IN ({parameterListToExpect}))", clause.Sql);
            Assert.AreEqual(limit, clause.Parameters.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ParameterLimit_DefaultLimit_Exceeded()
        {
            //ARRANGE
            var limit = WhereClauseCompiler.ParameterLimit;
            var array = Enumerable.Range(0, limit + 1);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => array.Contains(x.Id_Int));
        }

        [TestMethod]
        public void ParameterLimit_CustomLimit_NotExceeded()
        {
            //ARRANGE
            var limit = 5;
            var array = Enumerable.Range(0, limit);
            var parameterListToExpect = string.Join(", ", Enumerable.Range(0, limit).Select(x => $"@{WhereClauseCompiler.ParameterName}{x}"));

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => array.Contains(x.Id_Int), parameterLimit: limit);

            //ASSERT
            Assert.AreEqual($"([Id_Int] IN ({parameterListToExpect}))", clause.Sql);
            Assert.AreEqual(limit, clause.Parameters.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void ParameterLimit_CustomLimit_Exceeded()
        {
            //ARRANGE
            var limit = 5;
            var array = Enumerable.Range(0, limit + 1);

            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => array.Contains(x.Id_Int), parameterLimit: limit);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void PropertySelectionValidation_SelectedUnmappedProperty_Exception()
        {
            //ACT
            WhereClauseCompiler.ToSql<Model>(x => x.NotAMappedProperty == null);
        }

        [TestMethod]
        public void PropertySelectionValidation_SelectedMapperProperty()
        {
            //ACT
            var clause = WhereClauseCompiler.ToSql<Model>(x => x.AMappedPropertyWithMultipleAttributes == (object)1);

            //ASSERT
            Assert.AreEqual($"([AMappedPropertyWithMultipleAttributes] = @__P0)", clause.Sql);
            Assert.AreEqual(1, clause.Parameters.Count);
            Assert.AreEqual(typeof(int), clause.Parameters[$"__P0"].GetType());
            Assert.AreEqual(1, clause.Parameters[$"__P0"]);
        }
    }
}
