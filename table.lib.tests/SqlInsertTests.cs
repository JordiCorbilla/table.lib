using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using table.runner;

namespace table.lib.tests
{
    public class SqlInsertTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestGeneration()
        {
            var s = Table<TestClass>.Add(Samples.GetSampleOutput()).ToSqlInsertString();
            var lines = s.Split(Environment.NewLine);
            Assert.Multiple(() =>
            {
                Assert.That(lines[0], Is.EqualTo("INSERT INTO TestClass (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (321121,'Hi 312321',2121.32,1,'1970-01-01',34.43);"));
                Assert.That(lines[1], Is.EqualTo("INSERT INTO TestClass (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (32321,'Hi long text',21111111.32,1,'1970-01-01',34.43);"));
                Assert.That(lines[2], Is.EqualTo("INSERT INTO TestClass (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (321,'Hi longer text',2121.32,1,'1970-01-01',34.43);"));
                Assert.That(lines[3], Is.EqualTo("INSERT INTO TestClass (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (13,'Hi very long text',21111121.32,1,'1970-01-01',34.43);"));
            });
        }

        [Test]
        public void TestNullGeneration()
        {
            var s = Table<TestClass>.Add(Samples.GetNullOutput()).ToSqlInsertString();
            var lines = s.Split(Environment.NewLine);
            Assert.Multiple(() =>
            {
                Assert.That(lines[0], Is.EqualTo("INSERT INTO TestClass (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (NULL,NULL,NULL,NULL,NULL,NULL);"));
            });
        }

        [Test]
        public void TestNullFromDBGeneration()
        {
            IEnumerable<IDictionary<string, object>> table;
            using (var connection = new SqlConnection(@"Data Source=DESKTOP-TTUSQLJ\SQLEXPRESS;Initial Catalog=store;Integrated Security=True"))
            {
                connection.Open();
                const string data = @"
SELECT PhoneNumber, LockoutEnd
  FROM [store].[dbo].[AspNetUsers]
";
                table = connection.Query(data) as IEnumerable<IDictionary<string, object>>;
            }

            var enumerable = table as IDictionary<string, object>[] ??
                             (table ?? throw new InvalidOperationException()).ToArray();


            var s = DbTable.Add(enumerable).ToSqlInsertString();
            var lines = s.Split(Environment.NewLine);
            Assert.Multiple(() =>
            {
                Assert.That(lines[0], Is.EqualTo("INSERT INTO Table1 (PhoneNumber,LockoutEnd) VALUES (NULL,NULL);"));
            });
        }
    }
}