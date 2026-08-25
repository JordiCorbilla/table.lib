//MIT License

//Copyright (c) 2020-2024 Jordi Corbilla

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace table.lib
{
    public class Table<T> : Base<T> where T : class
    {
        /// <summary>
        ///     Where the magic happens.
        ///     Retrieve the entire content of the collection using reflection
        /// </summary>
        /// <param name="list"></param>
        /// <param name="options"></param>
        public Table(List<T> list, Options options = null)
        {
            if (options != null) Options = options;

            if (list.Count == 0)
            {
                if (Options != null && Options.DiscardEmptyList)
                    return;
            }

            PropertyNames = [];
            MaxWidth = [];
            Items = list;
            var properties = TypeMetadata<T>.Properties;
            ClassName = typeof(T).Name;
            foreach (var property in properties)
            {
                PropertyNames.Add(new PropertyName(property.Name) { Accessor = property });
                MaxWidth.Add(property.Name, property.Name.Length);
            }

            foreach (var row in Items)
                if (properties.Count != 0)
                {
                    foreach (var property in PropertyNames)
                    {
                        var value = GetValue(row, property);
                        if (value.Length > MaxWidth[property.Name])
                            MaxWidth[property.Name] = value.Length;
                    }
                }
                else
                {
                    var props = TypeMetadata<T>.GetRuntimeProperties(row.GetType());

                    var propertyIndex = 0;
                    foreach (var propertyInfo in props)
                    {
                        // Indexed property (collections)
                        var indexParameters = propertyInfo.GetIndexParameters();
                        if (indexParameters.Length == 1 && indexParameters[0].ParameterType == typeof(int) &&
                            row is IEnumerable enumerable)
                        {
                            var index = 0;
                            foreach (var res in enumerable)
                            {
                                var prop = $"{Options.DynamicName}{index}";
                                if (!MaxWidth.TryGetValue(prop, out var width))
                                {
                                    PropertyNames.Add(new PropertyName(prop, index, propertyIndex));
                                    width = prop.Length;
                                    MaxWidth.Add(prop, width);
                                }

                                var text = ObjectToString(res);
                                if (text.Length > width) MaxWidth[prop] = text.Length;
                                index++;
                            }
                        }
                        else if (indexParameters.Length == 0)
                        {
                            if (!MaxWidth.TryGetValue(propertyInfo.Name, out int value))
                            {
                                PropertyNames.Add(new PropertyName(propertyInfo.Name));
                                value = propertyInfo.Name.Length;
                                MaxWidth.Add(propertyInfo.Name, value);
                            }

                            var valueProp = GetValue(row, new PropertyName(propertyInfo.Name));
                            if (valueProp.Length > value)
                                MaxWidth[propertyInfo.Name] = valueProp.Length;
                        }

                        propertyIndex++;
                    }
                }
        }


        public static Table<T> Add(List<T> list)
        {
            return new Table<T>(list);
        }

        public static Table<T> Add(List<T> list, Options options)
        {
            return new Table<T>(list, options);
        }

        public Table<T> HighlightValue(HighlightOperator operation)
        {
            if (!Operation.TryGetValue(operation.Field, out List<HighlightOperator> value))
                Operation.Add(operation.Field, [operation]);
            else
                value.Add(operation);
            return this;
        }

        public Table<T> OverrideColumnsNames(Dictionary<string, string> columns)
        {
            ColumnNameOverrides = columns;
            foreach (var (key, value) in ColumnNameOverrides)
                if (value.Length > MaxWidth[key])
                    MaxWidth[key] = value.Length;
            return this;
        }

        public Table<T> FilterColumns(string[] columns, FilterAction action = FilterAction.Exclude)
        {
            var filter = columns.ToDictionary(column => column, column => false);
            ColumnFilter = filter;
            ColumnAction = action;
            return this;
        }

        public Table<T> ColumnContentTextJustification(Dictionary<string, TextJustification> columns)
        {
            ColumnTextJustification = columns;
            return this;
        }

        public Table<T> HighlightRows(ConsoleColor backgroundColor, ConsoleColor foregroundColor)
        {
            BackgroundColor = backgroundColor;
            ForegroundColor = foregroundColor;
            return this;
        }

        public void ToConsole()
        {
            if (Items.Count == 0) return;
            var s = "|";

            var filteredPropertyNames = FilterProperties();

            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
                if (ColumnNameOverrides.TryGetValue(property.Name, out string value))
                    headerName = value;

                var length = MaxWidth[property.Name] - headerName.Length;

                var header = headerName.ToValidOutput();
                var totalLength = $"{new string(' ', length)}{header}".Length;
                var remaining = totalLength - $"{new string(' ', length / 2)}{header}".Length;
                s += $" {new string(' ', length / 2)}{header}{new string(' ', remaining)} |";
            }

            Console.WriteLine(s);

            s = filteredPropertyNames.Aggregate("|",
                (current, name) => current + $" {new string('-', MaxWidth[name.Name])} |");

            Console.WriteLine(s);

            foreach (var row in Items)
            {
                Console.Write("|");
                foreach (var property in filteredPropertyNames)
                {
                    var valueProp = GetValue(row, property);
                    var length = MaxWidth[property.Name] - valueProp.Length;
                    var output = valueProp.ToValidOutput();
                    if (ColumnTextJustification.TryGetValue(property.Name, out TextJustification value))
                        switch (value)
                        {
                            case TextJustification.Centered:
                                var totalLength = $"{new string(' ', length)}{output}".Length;
                                var remaining = totalLength - $"{new string(' ', length / 2)}{output}".Length;
                                ConsoleRender($"{new string(' ', length / 2)}{output}{new string(' ', remaining)}",
                                    property.Name);
                                break;
                            case TextJustification.Right:
                                ConsoleRender($"{new string(' ', length)}{output}", property.Name);
                                break;
                            case TextJustification.Left:
                                ConsoleRender($"{output}{new string(' ', length)}", property.Name);
                                break;
                            case TextJustification.Justified:
                                ConsoleRender($"{output}{new string(' ', length)}", property.Name);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    else
                        ConsoleRender($"{output}{new string(' ', length)}", property.Name);
                }

                Console.Write(Environment.NewLine);
            }

            Console.WriteLine();
        }

        public override string ToString()
        {
            if (Items.Count == 0) return string.Empty;
            var s = "|";
            var sb = new StringBuilder();

            var filteredPropertyNames = FilterProperties();

            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
                if (ColumnNameOverrides.TryGetValue(property.Name, out string value))
                    headerName = value;

                var length = MaxWidth[property.Name] - headerName.Length;

                var header = headerName.ToValidOutput();
                var totalLength = $"{new string(' ', length)}{header}".Length;
                var remaining = totalLength - $"{new string(' ', length / 2)}{header}".Length;
                s += $" {new string(' ', length / 2)}{header}{new string(' ', remaining)} |";
            }

            sb.AppendLine(s);

            s = filteredPropertyNames.Aggregate("|",
                (current, name) => current + $" {new string('-', MaxWidth[name.Name])} |");

            sb.AppendLine(s);

            foreach (var row in Items)
            {
                sb.Append('|');
                foreach (var property in filteredPropertyNames)
                {
                    var valueProp = GetValue(row, property);
                    var length = MaxWidth[property.Name] - valueProp.Length;
                    var output = valueProp.ToValidOutput();
                    if (ColumnTextJustification.TryGetValue(property.Name, out TextJustification value))
                    {
                        switch (value)
                        {
                            case TextJustification.Centered:
                                var totalLength = $"{new string(' ', length)}{output}".Length;
                                var remaining = totalLength - $"{new string(' ', length / 2)}{output}".Length;
                                sb.Append(' ');
                                sb.Append($"{new string(' ', length / 2)}{output}{new string(' ', remaining)}");
                                sb.Append(" |");
                                break;
                            case TextJustification.Right:
                                sb.Append(' ');
                                sb.Append($"{new string(' ', length)}{output}");
                                sb.Append(" |");
                                break;
                            case TextJustification.Left:
                                sb.Append(' ');
                                sb.Append($"{output}{new string(' ', length)}");
                                sb.Append(" |");
                                break;
                            case TextJustification.Justified:
                                sb.Append(' ');
                                sb.Append($"{output}{new string(' ', length)}");
                                sb.Append(" |");
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        sb.Append(' ');
                        sb.Append($"{output}{new string(' ', length)}");
                        sb.Append(" |");
                    }
                }

                sb.Append(Environment.NewLine);
            }

            sb.AppendLine("");
            return sb.ToString();
        }

        public string ToSpecFlowString()
        {
            return Items.Count == 0 ? string.Empty : RenderingHelpers.RenderToString(WriteSpecFlow);
        }

        public void WriteSpecFlow(TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            if (Items.Count == 0) return;

            var properties = FilterProperties();
            writer.Write('|');
            foreach (var property in properties)
            {
                var header = ColumnNameOverrides.TryGetValue(property.Name, out var value) ? value : property.Name;
                RenderingHelpers.WriteCenteredCell(writer, header.ToValidOutput(), MaxWidth[property.Name]);
            }
            writer.WriteLine();

            foreach (var row in Items)
            {
                writer.Write('|');
                foreach (var property in properties)
                {
                    var output = GetValue(row, property).ToValidOutput();
                    var justification = ColumnTextJustification.TryGetValue(property.Name, out var value)
                        ? value
                        : TextJustification.Left;
                    RenderingHelpers.WriteValueCell(writer, output, MaxWidth[property.Name], justification);
                }
                writer.WriteLine();
            }
            writer.WriteLine();
        }

        public void ToMarkDown(string fileName, bool consoleVerbose = false)
        {
            using var file = new StreamWriter(fileName);
            WriteMarkdown(file);

            if (consoleVerbose)
                WriteMarkdown(Console.Out);
        }

        public string ToMarkDown()
        {
            return Items.Count == 0 ? string.Empty : RenderingHelpers.RenderToString(WriteMarkdown);
        }

        public void WriteMarkdown(TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            if (Items.Count == 0) return;

            var properties = FilterProperties();
            writer.Write('|');
            foreach (var property in properties)
            {
                var header = ColumnNameOverrides.TryGetValue(property.Name, out var value) ? value : property.Name;
                RenderingHelpers.WriteValueCell(writer, header.ToValidOutput(), MaxWidth[property.Name]);
            }
            writer.WriteLine();

            writer.Write('|');
            foreach (var property in properties)
            {
                var justification = ColumnTextJustification.TryGetValue(property.Name, out var value)
                    ? value
                    : TextJustification.Justified;
                writer.Write(' ');
                if (justification is TextJustification.Centered or TextJustification.Left) writer.Write(':');
                RenderingHelpers.WriteRepeated(writer, '-', MaxWidth[property.Name] -
                    (justification == TextJustification.Centered ? 2 : justification is TextJustification.Right or TextJustification.Left ? 1 : 0));
                if (justification is TextJustification.Centered or TextJustification.Right) writer.Write(':');
                writer.Write(" |");
            }
            writer.WriteLine();

            foreach (var row in Items)
            {
                writer.Write('|');
                foreach (var property in properties)
                    RenderingHelpers.WriteValueCell(writer, GetValue(row, property).ToValidOutput(), MaxWidth[property.Name]);
                writer.WriteLine();
            }
            writer.WriteLine();
        }

        public void ToHtml(string fileName)
        {
            using var file = new StreamWriter(fileName);
            WriteHtml(file);
        }

        public string ToHtml()
        {
            return Items.Count == 0 ? string.Empty : RenderingHelpers.RenderToString(WriteHtml);
        }

        public void WriteHtml(TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            if (Items.Count == 0) return;

            writer.WriteLine("<table style=\"border-collapse: collapse; width: 100%;\">");
            writer.WriteLine("<tr>");
            var properties = FilterProperties();
            foreach (var property in properties)
            {
                var header = ColumnNameOverrides.TryGetValue(property.Name, out var value) ? value : property.Name;
                writer.Write("<th style=\"text-align: center; background-color: #04163d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;\">");
                writer.Write(header.ToHtml());
                writer.WriteLine("</th>");
            }
            writer.WriteLine("</tr>");

            var rowNumber = 1;
            foreach (var row in Items)
            {
                writer.WriteLine("<tr>");
                foreach (var property in properties)
                {
                    var value = GetValue(row, property);
                    var color = GetHtmlCellColor(property.Name, value, rowNumber);
                    writer.Write("<td style=\"text-align: right; color: black; background-color: ");
                    writer.Write(color);
                    writer.Write(";padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;\">");
                    writer.Write(value.ToHtml());
                    writer.WriteLine("</td>");
                }
                rowNumber++;
                writer.WriteLine("</tr>");
            }
            writer.WriteLine("</table>");
        }

        private string GetHtmlCellColor(string propertyName, string value, int rowNumber)
        {
            var color = rowNumber % 2 == 0 ? "#f2f2f2" : "white";
            if (!Operation.TryGetValue(propertyName, out var operations)) return color;

            foreach (var operation in operations)
            {
                switch (operation.Type)
                {
                    case HighlightType.Decimal when decimal.TryParse(value.Trim(), out var parsed):
                        foreach (var expected in operation.DecimalValue)
                        {
                            var equal = decimal.Compare(Math.Round(parsed, Options.NumberDecimals),
                                Math.Round(expected, Options.NumberDecimals)) == 0;
                            if ((operation.Operation == HighlightOperation.Equality && equal) ||
                                (operation.Operation == HighlightOperation.Differences && !equal))
                                color = "#f9f948";
                        }
                        break;
                    case HighlightType.String:
                        foreach (var expected in operation.StringValue)
                        {
                            var equal = value.Trim() == expected;
                            if ((operation.Operation == HighlightOperation.Equality && equal) ||
                                (operation.Operation == HighlightOperation.Differences && !equal))
                                color = "#f9f948";
                        }
                        break;
                }
            }
            return color;
        }

        public void ToCsv(string fileName)
        {
            using var file = new StreamWriter(fileName);
            WriteCsv(file);
        }

        public void ToCsv(string fileName, CsvOptionType option, List<string> cache)
        {
            if (option == CsvOptionType.StartFile || option == CsvOptionType.AddToFile)
                cache.Add(ToCsv());
            if (option == CsvOptionType.EndFile)
            {
                using var file = new StreamWriter(fileName);
                cache.Add(ToCsv());
                foreach (var item in cache)
                    file.WriteLine(item);
            }
        }

        public string ToCsv()
        {
            if (Items.Count == 0 && Options.DiscardEmptyList) return string.Empty;
            return RenderingHelpers.RenderToString(WriteCsv);
        }

        public void WriteCsv(TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            if (Items.Count == 0 && Options.DiscardEmptyList) return;

            var properties = FilterProperties();
            for (var index = 0; index < properties.Count; index++)
            {
                if (index > 0) writer.Write(',');
                var property = properties[index];
                var header = ColumnNameOverrides.TryGetValue(property.Name, out var value) ? value : property.Name;
                RenderingHelpers.WriteCsvValue(writer, header);
            }
            writer.WriteLine();

            foreach (var row in Items)
            {
                for (var index = 0; index < properties.Count; index++)
                {
                    if (index > 0) writer.Write(',');
                    RenderingHelpers.WriteCsvValue(writer, GetValue(row, properties[index]));
                }
                writer.WriteLine();
            }
        }

        public void ToSqlInsertString(string fileName)
        {
            using var file = new StreamWriter(fileName);
            WriteSqlInsert(file);
        }

        public string ToSqlInsertString()
        {
            return Items.Count == 0 ? string.Empty : RenderingHelpers.RenderToString(WriteSqlInsert);
        }

        public void WriteSqlInsert(TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            if (Items.Count == 0) return;

            var properties = FilterProperties();
            var prefix = new StringBuilder("INSERT INTO ");
            prefix.Append(ClassName);
            prefix.Append(" (");
            for (var index = 0; index < properties.Count; index++)
            {
                if (index > 0) prefix.Append(',');
                var property = properties[index];
                var header = ColumnNameOverrides.TryGetValue(property.Name, out var value) ? value : property.Name;
                prefix.Append(header.ToSqlColumn());
            }
            prefix.Append(") VALUES (");

            foreach (var row in Items)
            {
                writer.Write(prefix);
                for (var index = 0; index < properties.Count; index++)
                {
                    if (index > 0) writer.Write(',');
                    writer.Write(ToSqlLiteral(GetOriginalValue(row, properties[index])));
                }
                writer.WriteLine(");");
            }
        }
    }
}
