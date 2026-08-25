using System;
using System.Globalization;
using System.IO;

namespace table.lib.tests
{
    public class RenderingTests
    {
        [Test]
        public void HtmlEncodesContentAndPreservesLineBreaks()
        {
            var table = Table<SpecialRow>.Add(
            [
                new SpecialRow { Text = "<script>&\"'\nnext", Amount = 12.34m }
            ]);

            var output = table.ToHtml();

            Assert.That(output, Does.Contain("&lt;script&gt;&amp;&quot;&#39;<br>next"));
            Assert.That(output, Does.Not.Contain("<script>"));
        }

        [Test]
        public void CsvQuotesCommasQuotesAndAnyLineEnding()
        {
            var table = Table<SpecialRow>.Add(
            [
                new SpecialRow { Text = "one,\"two\"\nthree", Amount = 12.34m }
            ]);

            var lines = table.ToCsv().Split(Environment.NewLine);

            Assert.That(lines[1], Is.EqualTo("\"one,\"\"two\"\"\nthree\",12.34"));
        }

        [Test]
        public void SqlUsesInvariantNumbersAndEscapesApostrophesWithoutChangingCommas()
        {
            var originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
                var table = Table<SpecialRow>.Add(
                [
                    new SpecialRow { Text = "O'Brien, Ltd", Amount = 12.34m }
                ]);

                Assert.That(table.ToSqlInsertString(), Is.EqualTo(
                    $"INSERT INTO SpecialRow (Text,Amount) VALUES ('O''Brien, Ltd',12.34);{Environment.NewLine}"));
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }

        [Test]
        public void StreamingWritersMatchStringOutputs()
        {
            var table = Table<SpecialRow>.Add(
            [
                new SpecialRow { Text = "value", Amount = 12.34m }
            ]);

            AssertWriterMatches(table.ToCsv(), table.WriteCsv);
            AssertWriterMatches(table.ToMarkDown(), table.WriteMarkdown);
            AssertWriterMatches(table.ToHtml(), table.WriteHtml);
            AssertWriterMatches(table.ToSpecFlowString(), table.WriteSpecFlow);
            AssertWriterMatches(table.ToSqlInsertString(), table.WriteSqlInsert);
        }

        [Test]
        public void DictionaryStreamingWritersMatchStringOutputs()
        {
            var table = TableDic<int, SpecialRow>.Add(new()
            {
                [1] = new SpecialRow { Text = "value", Amount = 12.34m }
            });

            AssertWriterMatches(table.ToCsv(), table.WriteCsv);
            AssertWriterMatches(table.ToMarkDown(), table.WriteMarkdown);
            AssertWriterMatches(table.ToHtml(), table.WriteHtml);
        }

        private static void AssertWriterMatches(string expected, Action<TextWriter> write)
        {
            using var writer = new StringWriter(CultureInfo.CurrentCulture);
            write(writer);
            Assert.That(writer.ToString(), Is.EqualTo(expected));
        }

        public class SpecialRow
        {
            public string Text { get; set; }
            public decimal Amount { get; set; }
        }
    }
}
