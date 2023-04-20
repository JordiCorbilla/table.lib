using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace table.lib.tests
{

    public class SqlInsertDbIntegrationTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestGeneration()
        {
            IEnumerable<IDictionary<string, object>> table;
            using (var connection =
                new SqlConnection(@"Data Source=DESKTOP-TTUSQLJ\SQLEXPRESS;Initial Catalog=Store;Integrated Security=True"))
            {
                connection.Open();
                const string data = @"
SELECT [Id]
      ,[Name]
      ,[Description]
      ,[Price]
      ,[PictureUrl]
      ,[Type]
      ,[Brand]
      ,[QuantityInStock] from dbo.Products
";
                table = connection.Query(data) as IEnumerable<IDictionary<string, object>>;
            }

            var enumerable = table as IDictionary<string, object>[] ??
                             (table ?? throw new InvalidOperationException()).ToArray();

            var s = DbTable.Add(enumerable, new Options
            {
                DateFormat = "dd-MM-yy",
                DecimalFormat = "#,##0.########"
            }).ToSqlInsertString();

            var lines = s.Split(Environment.NewLine);

            Assert.Multiple(() =>
            {
                Assert.That(lines[0], Is.EqualTo("INSERT INTO Table1 (Id,Name,Description,Price,PictureUrl,Type,Brand,QuantityInStock) VALUES (1,'Angular Speedster Board 2000','Lorem ipsum dolor sit amet- consectetuer adipiscing elit. Maecenas porttitor congue massa. Fusce posuere- magna sed pulvinar ultricies- purus lectus malesuada libero- sit amet commodo magna eros quis urna.',20000,'/images/products/sb-ang1.png','Boards','Angular',100);"));
            });


        }
    }
}
