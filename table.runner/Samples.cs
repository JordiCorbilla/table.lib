//MIT License

//Copyright (c) 2020-2023 Jordi Corbilla

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using table.lib;

namespace table.runner
{
    public static class Samples
    {
        public static List<TestClass> GetSampleOutput()
        {
            var list = new List<TestClass>
            {
                new TestClass
                {
                    Field1 = 321121, Field2 = "Hi 312321", Field3 = 2121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new TestClass
                {
                    Field1 = 32321, Field2 = "Hi long text", Field3 = 21111111.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new TestClass
                {
                    Field1 = 321, Field2 = "Hi longer text", Field3 = 2121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new TestClass
                {
                    Field1 = 13, Field2 = "Hi very long text", Field3 = 21111121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new TestClass
                {
                    Field1 = 13, Field2 = "Hi very, long text", Field3 = 21111121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new TestClass
                {
                    Field1 = 13, Field2 = "Hi \"very\" long\n text", Field3 = 21111121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.4300001
                },
                new TestClass
                {
                    Field1 = 13, Field2 = null, Field3 = 21111121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.4300001
                }
            };
            return list;
        }

        public static List<TestClass> GetNullOutput()
        {
            var list = new List<TestClass>
            {
                new TestClass
                {
                    Field1 = null, Field2 = null, Field3 = null, Field4 = null,
                    Field5 = null, Field6 = null
                }
            };
            return list;
        }

        public static void SimpleDbRecordWithOptionsAndMultipleColumnsAndFilter()
        {
            IEnumerable<IDictionary<string, object>> table;
            using (var connection =
                new SqlConnection("Data Source=localhost;Initial Catalog=DatingApp;Integrated Security=True"))
            {
                connection.Open();
                const string data = @"
SELECT * from test
";
                table = connection.Query(data) as IEnumerable<IDictionary<string, object>>;
            }

            var enumerable = table as IDictionary<string, object>[] ??
                             (table ?? throw new InvalidOperationException()).ToArray();
            var s = DbTable.Add(enumerable, new Options
            {
                DateFormat = "dd-MM-yy",
                DecimalFormat = "#,##0.########"
            }).FilterColumns(new[] { "AssetClass" }, FilterAction.Exclude).ToString();
            Console.WriteLine(s);

            s = DbTable.Add(enumerable, new Options
            {
                DateFormat = "dd-MM-yy",
                DecimalFormat = "#,##0.########"
            }).FilterColumns(new[] { "AssetClass" }, FilterAction.Exclude).ToSpecFlowString();
            Console.WriteLine(s);
        }

        public static void SimpleToSpecFlowString()
        {
            var s = Table<TestClass>.Add(GetSampleOutput()).ToSpecFlowString();
            Console.Write(s);
        }

        public static void SimpleToString()
        {
            var s = Table<TestClass>.Add(GetSampleOutput()).ToString();
            Console.Write(s);
        }

        public static List<IEnumerable<string>> GetStringMatrix()
        {
            var test = new List<IEnumerable<string>>
            {
                new List<string> {"AAA", "BBB", "CCC"},
                new List<string> {"AAA", "BBB", "CCC"},
                new List<string> {"AAA", "BBB", "CCC"},
                new List<string> {"AAA", "BBB", "CCC"}
            };
            return test;
        }

        public static List<IEnumerable<int>> GetIntMatrix()
        {
            var matrix = new List<IEnumerable<int>>
            {
                new List<int> {1, 2, 3},
                new List<int> {1, 2, 3},
                new List<int> {1, 2, 3},
                new List<int> {1, 2, 3}
            };
            return matrix;
        }

        public static Dictionary<string, TestClass> GetSimpleDictionary()
        {
            var dic = new Dictionary<string, TestClass>
            {
                {
                    "1", new TestClass
                    {
                        Field1 = 321121,
                        Field2 = "Hi 312321",
                        Field3 = 2121.32m,
                        Field4 = true,
                        Field5 = new DateTime(1970, 1, 1),
                        Field6 = 34.432
                    }
                },
                {
                    "2", new TestClass
                    {
                        Field1 = 321121,
                        Field2 = "Hi 312321",
                        Field3 = 2121.32m,
                        Field4 = true,
                        Field5 = new DateTime(1970, 1, 1),
                        Field6 = 134.43
                    }
                },
                {
                    "3", new TestClass
                    {
                        Field1 = 321121,
                        Field2 = "Hi 312321",
                        Field3 = 2121.32m,
                        Field4 = true,
                        Field5 = new DateTime(1970, 1, 1),
                        Field6 = 354.43
                    }
                }
            };
            return dic;
        }

        public static Dictionary<decimal, TestClass> GetSimpleDictionaryDecimal()
        {
            var dic = new Dictionary<decimal, TestClass>
            {
                {
                    123m, new TestClass
                    {
                        Field1 = 321121,
                        Field2 = "Hi 312321",
                        Field3 = 2121.32m,
                        Field4 = true,
                        Field5 = new DateTime(1970, 1, 1),
                        Field6 = 34.43
                    }
                },
                {
                    124m, new TestClass
                    {
                        Field1 = 321121,
                        Field2 = "Hi 312321",
                        Field3 = 2121.32m,
                        Field4 = true,
                        Field5 = new DateTime(1970, 1, 1),
                        Field6 = 34.43
                    }
                },
                {
                    125.67m, new TestClass
                    {
                        Field1 = 321121,
                        Field2 = "Hi 312321",
                        Field3 = 2121.32m,
                        Field4 = true,
                        Field5 = new DateTime(1970, 1, 1),
                        Field6 = 34.43
                    }
                }
            };
            return dic;
        }

        public static void SimpleConsoleOutputForList()
        {
            Table<TestClass>.Add(GetSampleOutput()).ToConsole();
            Table<TestClass>.Add(GetSampleOutput(), new Options
            {
                DateFormat = "dd-MM-yy",
                DecimalFormat = "#,##0.########"
            })
                .ToConsole();
        }

        public static void SimpleConsoleOutputForListNoException()
        {
            Table<TestClass>.Add(new List<TestClass>()).ToConsole();
        }

        public static void SimpleConsoleOutputWithHighlighterForList()
        {
            Table<TestClass>.Add(GetSampleOutput())
                .HighlightValue(new HighlightOperator
                { Field = "Field3", Type = HighlightType.Decimal, DecimalValue = new List<decimal> { 2121.32m } })
                .ToConsole();
        }

        public static void SimpleCsvOutputForList()
        {
            Table<TestClass>.Add(GetSampleOutput()).ToCsv(@"C:\temp\test-list.csv");
        }

        public static void SimpleCsvOutputForListMultiple()
        {
            var cache = new List<string>();
            Table<TestClass>.Add(GetSampleOutput()).ToCsv(@"C:\temp\test-list-m.csv", CsvOptionType.StartFile, cache);
            Table<TestClass>.Add(GetSampleOutput()).ToCsv(@"C:\temp\test-list-m.csv", CsvOptionType.AddToFile, cache);
            Table<TestClass>.Add(GetSampleOutput()).ToCsv(@"C:\temp\test-list-m.csv", CsvOptionType.EndFile, cache);
        }

        public static void SimpleCsvEmptyOutputForList()
        {
            Table<TestClass>.Add(new List<TestClass>(), new Options { DiscardEmptyList = false }).ToCsv(@"C:\temp\test-list-empty.csv");
        }

        public static void SimpleHtmlOutputForList()
        {
            Table<TestClass>.Add(GetSampleOutput()).ToHtml(@"C:\temp\test-list.html");
        }

        public static void SimpleHtmlOutputWithHighlighterForList()
        {
            Table<TestClass>.Add(GetSampleOutput())
                .HighlightValue(new HighlightOperator
                { Field = "Field3", Type = HighlightType.Decimal, DecimalValue = new List<decimal> { 2121.32m }, Operation = HighlightOperation.Equality })
                .ToHtml(@"C:\temp\test-list-highlight.html");
        }

        public static void ComplexConsoleOutputForList()
        {
            Table<IEnumerable<string>>.Add(GetStringMatrix()).ToConsole();
        }

        public static void ComplexConsoleOutputFilteringForList()
        {
            Table<IEnumerable<string>>.Add(GetStringMatrix())
                .FilterColumns(new[] { "Dynamic0" }, FilterAction.Include)
                .ToConsole();

            Table<IEnumerable<string>>.Add(GetStringMatrix())
                .FilterColumns(new[] { "Dynamic0" }, FilterAction.Include)
                .ToConsole();

            Table<IEnumerable<string>>.Add(GetStringMatrix())
                .FilterColumns(new[] { "Dynamic0", "Dynamic1" }, FilterAction.Include)
                .HighlightRows(ConsoleColor.Red, ConsoleColor.White)
                .ToConsole();
        }

        public static void ComplexConsoleOutputOverrideFilteringForList()
        {
            Table<IEnumerable<string>>.Add(GetStringMatrix())
                .OverrideColumnsNames(new Dictionary<string, string> { { "Dynamic0", "ColumnA" } })
                .FilterColumns(new[] { "Dynamic0" }, FilterAction.Include)
                .ToConsole();

            Table<IEnumerable<string>>.Add(GetStringMatrix())
                .OverrideColumnsNames(new Dictionary<string, string> { { "Dynamic0", "ColumnA" } }).ToConsole();

            Table<IEnumerable<string>>.Add(GetStringMatrix())
                .OverrideColumnsNames(new Dictionary<string, string> { { "Dynamic0", "ColumnA" } })
                .FilterColumns(new[] { "Capacity", "Count" }).ToConsole();

            Table<IEnumerable<string>>.Add(GetStringMatrix()).OverrideColumnsNames(new Dictionary<string, string>
            {
                {"Dynamic0", "A"},
                {"Dynamic1", "B"},
                {"Dynamic2", "C"}
            }).FilterColumns(new[] { "Capacity", "Count" }).ColumnContentTextJustification(
                new Dictionary<string, TextJustification>
                {
                    {"Dynamic0", TextJustification.Right},
                    {"Dynamic1", TextJustification.Centered}
                }).ToConsole();
        }

        public static void ComplexCsvOutputFilteringForList()
        {
            Table<IEnumerable<string>>.Add(GetStringMatrix())
                .OverrideColumnsNames(new Dictionary<string, string> { { "Dynamic0", "ColumnA" } })
                .FilterColumns(new[] { "Capacity", "Count" }).ToCsv(@"C:\temp\test.csv");
        }

        public static void ComplexHtmlOutputFilteringForList()
        {
            Table<IEnumerable<string>>.Add(GetStringMatrix())
                .OverrideColumnsNames(new Dictionary<string, string> { { "Dynamic0", "ColumnA" } })
                .FilterColumns(new[] { "Capacity", "Count" }).ToHtml(@"C:\temp\test.html");
        }

        public static void ComplexMarkDownOutputFilteringForList()
        {
            Table<IEnumerable<string>>.Add(GetStringMatrix())
                .OverrideColumnsNames(new Dictionary<string, string> { { "Dynamic0", "ColumnA" } })
                .FilterColumns(new[] { "Capacity", "Count" })
                .ColumnContentTextJustification(new Dictionary<string, TextJustification>
                    {{"Dynamic0", TextJustification.Right}}).ToMarkDown(@"C:\temp\test.md", true);
        }

        public static void ComplexConsoleMatrix()
        {
            Table<IEnumerable<int>>.Add(GetIntMatrix(), new Options
            {
                DynamicName = "T"
            })
                .ToConsole();
        }

        public static void SimpleConsoleForDictionary()
        {
            TableDic<string, TestClass>.Add(GetSimpleDictionary())
                .ToConsole();

            TableDic<string, TestClass>.Add(GetSimpleDictionary(), new Options
            {
                DynamicName = "D",
                KeyName = "Id"
            })
                .ToConsole();

            TableDic<string, TestClass>.Add(GetSimpleDictionary())
                .FilterColumns(["Key_Id"])
                .ToConsole();

            TableDic<decimal, TestClass>.Add(GetSimpleDictionaryDecimal())
                .ToConsole();
        }

        public static void SimpleConsoleOutputWithHighlighterForDictionary()
        {
            TableDic<string, TestClass>.Add(GetSimpleDictionary(), new Options
            {
                NumberDecimals = 0
            })
                .HighlightValue(new HighlightOperator
                {
                    Field = "Field3",
                    Type = HighlightType.Decimal,
                    DecimalValue = new List<decimal> { 2121.32m },
                    Operation = HighlightOperation.Equality
                })
                .HighlightValue(new HighlightOperator
                {
                    Field = "Field6",
                    Type = HighlightType.Decimal,
                    DecimalValue = new List<decimal>() { 34.43m, 134.43m },
                    Operation = HighlightOperation.Equality
                })
                .ToConsole();
        }

        public static void SimpleCsvOutputForDictionary()
        {
            TableDic<string, TestClass>.Add(GetSimpleDictionary())
                .ToCsv(@"C:\temp\testDic.csv");
        }

        public static void SimpleConsoleOutputForDictionary()
        {
            //var dic = new Dictionary<string, string> {{"1", "1"}, {"2", "2"}};
            //TableDic<string, string>.Add(dic)
            //    .ToConsole();
        }

        public static void ComplexMarkDownOutputFilteringForDictionary()
        {
            TableDic<string, TestClass>.Add(GetSimpleDictionary())
                .ColumnContentTextJustification(new Dictionary<string, TextJustification>
                    {{"Field3", TextJustification.Right}}).ToMarkDown(@"C:\temp\testDic.md", true);
        }

        public static void SimpleHtmlOutputForDictionary()
        {
            TableDic<string, TestClass>.Add(GetSimpleDictionary()).ToHtml(@"C:\temp\test-list-dic.html");
        }

        public static void SimpleDbRecord()
        {
            IEnumerable<IDictionary<string, object>> table;
            using (var connection =
                new SqlConnection("Data Source=localhost;Initial Catalog=DatingApp;Integrated Security=True"))
            {
                connection.Open();
                const string data = @"
SELECT [Id]
      ,[SenderId]
      ,[SenderUsername]
      ,[RecipientId]
      ,[RecipientUsername]
      ,[Content]
      ,[DateRead]
      ,[MessageSent]
      ,[SenderDeleted]
      ,[RecipientDeleted]
  FROM [DatingApp].[dbo].[Messages]
";
                table = connection.Query(data) as IEnumerable<IDictionary<string, object>>;
            }

            var enumerable = table as IDictionary<string, object>[] ??
                             (table ?? throw new InvalidOperationException()).ToArray();
            var s = DbTable.Add(enumerable).ToString();
            Console.WriteLine(s);

            s = DbTable.Add(enumerable).ToSpecFlowString();
            Console.WriteLine(s);
        }

        public static void SimpleDbRecordWithOptions()
        {
            IEnumerable<IDictionary<string, object>> table;
            using (var connection =
                new SqlConnection("Data Source=localhost;Initial Catalog=DatingApp;Integrated Security=True"))
            {
                connection.Open();
                const string data = @"
SELECT [Id]
      ,[SenderId]
      ,[SenderUsername]
      ,[RecipientId]
      ,[RecipientUsername]
      ,[Content]
      ,[DateRead]
      ,[MessageSent]
      ,[SenderDeleted]
      ,[RecipientDeleted]
  FROM [DatingApp].[dbo].[Messages] where id = 1
";
                table = connection.Query(data) as IEnumerable<IDictionary<string, object>>;
            }

            var enumerable = table as IDictionary<string, object>[] ??
                             (table ?? throw new InvalidOperationException()).ToArray();
            var s = DbTable.Add(enumerable, new Options
            {
                DateFormat = "dd-MM-yy",
                DecimalFormat = "#,##0.########"
            }).ToString();
            Console.WriteLine(s);

            s = DbTable.Add(enumerable, new Options
            {
                DateFormat = "dd-MM-yy",
                DecimalFormat = "#,##0.########"
            }).ToSpecFlowString();
            Console.WriteLine(s);
        }

        public static void SimpleDbRecordWithOptionsAndMultipleColumns()
        {
            IEnumerable<IDictionary<string, object>> table;
            using (var connection =
                new SqlConnection("Data Source=localhost;Initial Catalog=DatingApp;Integrated Security=True"))
            {
                connection.Open();
                const string data = @"
SELECT * from test
";
                table = connection.Query(data) as IEnumerable<IDictionary<string, object>>;
            }

            var enumerable = table as IDictionary<string, object>[] ??
                             (table ?? throw new InvalidOperationException()).ToArray();
            var s = DbTable.Add(enumerable, new Options
            {
                DateFormat = "dd-MM-yy",
                DecimalFormat = "#,##0.########"
            }).ToString();
            Console.WriteLine(s);

            s = DbTable.Add(enumerable, new Options
            {
                DateFormat = "dd-MM-yy",
                DecimalFormat = "#,##0.########"
            }).ToSpecFlowString();
            Console.WriteLine(s);
        }
    }
}