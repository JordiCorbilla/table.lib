using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace table.lib.tests
{
    public class DbTableTests
    {
        [Test]
        public void MaterializesOneShotSequenceOnlyOnce()
        {
            var enumerations = 0;
            var table = DbTable.Add(GetRows());

            _ = table.ToString();
            _ = table.ToSpecFlowString();
            _ = table.ToSqlInsertString();

            Assert.That(enumerations, Is.EqualTo(1));
            return;

            IEnumerable<IDictionary<string, object>> GetRows()
            {
                enumerations++;
                yield return new Dictionary<string, object> { ["Id"] = 1, ["Name"] = "one" };
            }
        }

        [Test]
        public void SupportsHeterogeneousRowsAndMissingValues()
        {
            var table = DbTable.Add(
            [
                new Dictionary<string, object> { ["Id"] = 1 },
                new Dictionary<string, object> { ["Id"] = 2, ["Name"] = "two" }
            ]);

            var output = table.ToString();

            Assert.That(output, Does.Contain("Name"));
            Assert.That(output, Does.Contain("two"));
            Assert.That(table.ToSqlInsertString(), Does.StartWith("INSERT INTO Table1 (Id,Name) VALUES (1,NULL);"));
        }

        [Test]
        public void AppliesColumnFiltersToHeadersAndValues()
        {
            var table = DbTable.Add(
            [
                new Dictionary<string, object> { ["Id"] = 1, ["Name"] = "one" }
            ]).FilterColumns(["Name"], FilterAction.Include);

            Assert.That(table.ToSqlInsertString(), Is.EqualTo(
                $"INSERT INTO Table1 (Name) VALUES ('one');{Environment.NewLine}"));
            Assert.That(table.ToString(), Does.Not.Contain("Id"));
        }

        [Test]
        public void StreamingWritersMatchStringOutputs()
        {
            var table = DbTable.Add(
            [
                new Dictionary<string, object> { ["Id"] = 1, ["Name"] = "one" }
            ]);

            AssertWriterMatches(table.ToSpecFlowString(), table.WriteSpecFlow);
            AssertWriterMatches(table.ToSqlInsertString(), table.WriteSqlInsert);
        }

        private static void AssertWriterMatches(string expected, Action<TextWriter> write)
        {
            using var writer = new StringWriter(CultureInfo.CurrentCulture);
            write(writer);
            Assert.That(writer.ToString(), Is.EqualTo(expected));
        }
    }
}
