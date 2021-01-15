using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text;
using Autofac.Extras.Moq;
using Dapper.Contrib.Extensions;
using Dapper.Extensions.Snapper.DatabaseConnection;
using Dapper.Extensions.Snapper.Helpers.Cache;
using Dapper.Extensions.Snapper.Logic;
using Dapper.Extensions.Snapper.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Dapper.Extensions.Snapper.Tests.Repositories
{
    [TestClass]
    public class SnapperDatabaseRepositoryTests
    {
        [TestMethod]
        public void TestGetTableNameMethod()
        {
            //ARRANGE
            var connectionManagerMock = new Mock<IDatabaseConnectionFactory<DbConnection>>();
            //var cacheMock = new Mock<ICache>();

            //cacheMock.Setup(x => x.CacheExecution(It.IsAny<Func<string>>(), "", false, 0))
            //    .Returns< Func<string>, string, bool, int>((Func<string> func, string ds, bool b, int a) => func.Invoke());
            
            var snapperMemoryCache = new SnapperMemoryCache();
            SnapperDatabaseRepositoryHelper<DbConnection> testRepository = new SnapperDatabaseRepositoryHelper<DbConnection>(connectionManagerMock.Object, snapperMemoryCache);

            //ACT
            var result = testRepository.ExposeGetTableNameMethod<Model>();

            //ASSERT
            Assert.AreEqual("Models", result);
        }

        [TestMethod]
        public void TestGetIntegerColumnName()
        {
            //ARRANGE
            var connectionManagerMock = new Mock<IDatabaseConnectionFactory<DbConnection>>();

            var snapperMemoryCache = new SnapperMemoryCache();
            SnapperDatabaseRepositoryHelper<DbConnection> testRepository = new SnapperDatabaseRepositoryHelper<DbConnection>(connectionManagerMock.Object, snapperMemoryCache);

            //ACT
            var result = testRepository.ExposeGetColumnName<Model>(x => x.Id_Int);

            //ASSERT
            Assert.AreEqual("Id_Int", result);
        }

        [TestMethod]
        public void TestGetStringColumnName()
        {
            //ARRANGE
            var connectionManagerMock = new Mock<IDatabaseConnectionFactory<DbConnection>>();
            var snapperMemoryCache = new SnapperMemoryCache();
            SnapperDatabaseRepositoryHelper<DbConnection> testRepository = new SnapperDatabaseRepositoryHelper<DbConnection>(connectionManagerMock.Object, snapperMemoryCache);

            //ACT
            var result = testRepository.ExposeGetColumnName<Model>(x => x.StringData);

            //ASSERT
            Assert.AreEqual("StringData", result);
        }

        [TestMethod]
        public void TestGetDateColumnName()
        {
            //ARRANGE
            var connectionManagerMock = new Mock<IDatabaseConnectionFactory<DbConnection>>();
            var snapperMemoryCache = new SnapperMemoryCache();
            SnapperDatabaseRepositoryHelper<DbConnection> testRepository = new SnapperDatabaseRepositoryHelper<DbConnection>(connectionManagerMock.Object, snapperMemoryCache);

            //ACT
            var result = testRepository.ExposeGetColumnName<Model>(x => x.Date);

            //ASSERT
            Assert.AreEqual("Date", result);
        }


        [TestMethod]
        public void TestGetBoolColumnName()
        {
            //ARRANGE
            var connectionManagerMock = new Mock<IDatabaseConnectionFactory<DbConnection>>();
            var snapperMemoryCache = new SnapperMemoryCache();
            SnapperDatabaseRepositoryHelper<DbConnection> testRepository = new SnapperDatabaseRepositoryHelper<DbConnection>(connectionManagerMock.Object, snapperMemoryCache);

            //ACT
            var result = testRepository.ExposeGetColumnName<Model>(x => x.Date);

            //ASSERT
            Assert.AreEqual("Date", result);
        }

        [TestMethod]
        public void TestGetGuidColumnName()
        {
            //ARRANGE
            var connectionManagerMock = new Mock<IDatabaseConnectionFactory<DbConnection>>();
            var snapperMemoryCache = new SnapperMemoryCache();
            SnapperDatabaseRepositoryHelper<DbConnection> testRepository = new SnapperDatabaseRepositoryHelper<DbConnection>(connectionManagerMock.Object, snapperMemoryCache);

            //ACT
            var result = testRepository.ExposeGetColumnName<Model>(x => x.Guid);

            //ASSERT
            Assert.AreEqual("Guid", result);
        }

        [TestMethod]
        public void TestDefineSelectQueryMethod()
        {
            //ARRANGE
            var connectionManagerMock = new Mock<IDatabaseConnectionFactory<DbConnection>>();
            var snapperMemoryCache = new SnapperMemoryCache();
            SnapperDatabaseRepositoryHelper<DbConnection> testRepository = new SnapperDatabaseRepositoryHelper<DbConnection>(connectionManagerMock.Object, snapperMemoryCache);

            //ACT
            var result = testRepository.ExposeDefineQuery
            ("Model",
                $@"SELECT c.*, a.Id as ModelId, a.* FROM { testRepository.ExposeGetTableNameMethod<Model>()}
                   WHERE c.{testRepository.ExposeGetColumnName<Model>(x => x.Id_Int)} = @companyId
                   ORDER BY c.{testRepository.ExposeGetColumnName<Model>(x => x.StringData)};");

            //ASSERT
            var expectedResult = @"SELECT c.*, a.Id as ModelId, a.* FROM Models
                   WHERE c.Id_Int = @companyId
                   ORDER BY c.StringData;";
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void TestDefineInsertQueryMethod()
        {
            //ARRANGE
            var connectionManagerMock = new Mock<IDatabaseConnectionFactory<DbConnection>>();
            var snapperMemoryCache = new SnapperMemoryCache();
            SnapperDatabaseRepositoryHelper<DbConnection> testRepository = new SnapperDatabaseRepositoryHelper<DbConnection>(connectionManagerMock.Object, snapperMemoryCache);

            //ACT
            var insertQueryResult = testRepository.ExposeDefineQuery
            ("Model",
                $@"INSERT INTO {testRepository.ExposeGetTableNameMethod<Model>()} 
                ({testRepository.ExposeGetColumnName<Model>(x => x.Id_Int)},{testRepository.ExposeGetColumnName<Model>(x => x.StringData)}) 
                VALUES(@Id_Int,@String_data);");

            //ASSERT
            var expectedResult = @"INSERT INTO Models
                 (Id_Int,StringData) 
                VALUES(@Id_Int,@String_data);";
            Assert.AreEqual(expectedResult.Replace("\n", "").Replace("\r", ""), insertQueryResult.Replace("\n", "").Replace("\r", ""));
        }

        [TestMethod]
        public void TestDefineUpdateQueryMethod()
        {
            //ARRANGE
            var connectionManagerMock = new Mock<IDatabaseConnectionFactory<DbConnection>>();
            var snapperMemoryCache = new SnapperMemoryCache();
            SnapperDatabaseRepositoryHelper<DbConnection> testRepository = new SnapperDatabaseRepositoryHelper<DbConnection>(connectionManagerMock.Object, snapperMemoryCache);

            //ACT
            var updateQueryResult = testRepository.ExposeDefineQuery
            ("Model",
                $@"UPDATE {testRepository.ExposeGetTableNameMethod<Model>()} c 
                    SET c.{testRepository.ExposeGetColumnName<Model>(x => x.Flag)} = false
                    WHERE c.{testRepository.ExposeGetColumnName<Model>(x => x.Id_Int)} = @id_Int
                        AND c.{testRepository.ExposeGetColumnName<Model>(x => x.StringData)} != @stringData;");

            //ASSERT
            var expectedResult = @"UPDATE Models c 
                    SET c.Flag = false
                    WHERE c.Id_Int = @id_Int
                        AND c.StringData != @stringData;";
            Assert.AreEqual(expectedResult.Replace("\n", "").Replace("\r", ""), updateQueryResult.Replace("\n", "").Replace("\r", ""));
        }

        [TestMethod]
        public void TestDefineDeleteQueryMethod()
        {
            //ARRANGE
            var connectionManagerMock = new Mock<IDatabaseConnectionFactory<DbConnection>>();
            var snapperMemoryCache = new SnapperMemoryCache();
            SnapperDatabaseRepositoryHelper<DbConnection> testRepository = new SnapperDatabaseRepositoryHelper<DbConnection>(connectionManagerMock.Object, snapperMemoryCache);

            //ACT
            var deleteQueryResult = testRepository.ExposeDefineQuery
            ("Model",
                $@"DELETE
                FROM {testRepository.ExposeGetTableNameMethod<Model>()} 
                WHERE {testRepository.ExposeGetColumnName<Model>(x => x.Id_Int)}=@Id_Int
                    AND {testRepository.ExposeGetColumnName<Model>(x => x.StringData)}=@String_data");

            //ASSERT
            var expectedResult = @"DELETE
                FROM Models 
                WHERE Id_Int=@Id_Int
                    AND StringData=@String_data";
            Assert.AreEqual(expectedResult.Replace("\n", "").Replace("\r", ""), deleteQueryResult.Replace("\n", "").Replace("\r", ""));
        }
    }
}
