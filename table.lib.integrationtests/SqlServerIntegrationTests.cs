using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace table.lib.integrationtests
{
    public class SqlServerIntegrationTests
    {
        [Test]
        public void GeneratesProductInserts()
        {
            var connectionString = Environment.GetEnvironmentVariable("TABLE_LIB_SQL_PRODUCTS_CONNECTION");
            if (string.IsNullOrWhiteSpace(connectionString))
                Assert.Ignore("Set TABLE_LIB_SQL_PRODUCTS_CONNECTION to run this SQL Server integration test.");

            IEnumerable<IDictionary<string, object>> table;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                table = connection.Query(@"
SELECT [Id], [Name], [Description], [Price], [PictureUrl], [Type], [Brand], [QuantityInStock]
FROM dbo.Products") as IEnumerable<IDictionary<string, object>>;
            }

            var rows = table?.ToArray() ?? throw new InvalidOperationException();
            var output = DbTable.Add(rows).ToSqlInsertString();
            Assert.That(output, Does.StartWith("INSERT INTO Table1 (Id,Name,Description,Price,PictureUrl,Type,Brand,QuantityInStock) VALUES ("));
        }

        [Test]
        public void GeneratesNullUserValues()
        {
            var connectionString = Environment.GetEnvironmentVariable("TABLE_LIB_SQL_USERS_CONNECTION");
            if (string.IsNullOrWhiteSpace(connectionString))
                Assert.Ignore("Set TABLE_LIB_SQL_USERS_CONNECTION to run this SQL Server integration test.");

            IEnumerable<IDictionary<string, object>> table;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                table = connection.Query(@"
SELECT PhoneNumber, LockoutEnd
FROM [store].[dbo].[AspNetUsers]") as IEnumerable<IDictionary<string, object>>;
            }

            var output = DbTable.Add(table ?? throw new InvalidOperationException()).ToSqlInsertString();
            Assert.That(output, Does.StartWith("INSERT INTO Table1 (PhoneNumber,LockoutEnd) VALUES (NULL,NULL);"));
        }
    }
}
