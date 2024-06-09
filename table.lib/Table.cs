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
            var properties = typeof(T).GetProperties();
            ClassName = typeof(T).Name;
            foreach (var property in properties)
            {
                PropertyNames.Add(new PropertyName(property.Name));
                MaxWidth.Add(property.Name, property.Name.Length);
            }

            foreach (var row in Items)
                if (properties.Length != 0)
                {
                    foreach (var property in PropertyNames)
                    {
                        var value = GetValue(row, new PropertyName(property.Name));
                        if (value.Length > MaxWidth[property.Name])
                            MaxWidth[property.Name] = value.Length;
                    }
                }
                else
                {
                    var props = row.GetType().GetProperties();

                    var propertyIndex = 0;
                    foreach (var propertyInfo in props)
                    {
                        // Indexed property (collections)
                        if (propertyInfo.GetIndexParameters().Length > 0)
                        {
                            var reading = true;
                            var index = 0;
                            while (reading)
                                try
                                {
                                    var prop = $"{Options.DynamicName}{index}";
                                    var res = propertyInfo.GetValue(row, [index]);
                                    if (!MaxWidth.TryGetValue(prop, out int value))
                                    {
                                        PropertyNames.Add(new PropertyName(prop, index, propertyIndex));
                                        value = prop.Length;
                                        MaxWidth.Add(prop, value);
                                    }

                                    if (res.ToString().Length > value)
                                        MaxWidth[prop] = res.ToString().Length;
                                    index++;
                                }
                                catch (Exception)
                                {
                                    reading = false;
                                }
                        }
                        else
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

        public void ToMarkDown(string fileName, bool consoleVerbose = false)
        {
            using var file = new StreamWriter(fileName);
            file.WriteLine(ToMarkDown());

            if (consoleVerbose)
                Console.WriteLine(ToMarkDown());
        }

        public string ToMarkDown()
        {
            if (Items.Count == 0) return "";
            var stringBuilder = new StringBuilder();
            var s = "|";

            var filteredPropertyNames = FilterProperties();

            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
                if (ColumnNameOverrides.TryGetValue(property.Name, out string value))
                    headerName = value;

                var length = MaxWidth[property.Name] - headerName.Length;
                s += $" {headerName.ToValidOutput()}{new string(' ', length)} |";
            }

            stringBuilder.AppendLine(s);

            s = "|";
            foreach (var property in filteredPropertyNames)
            {
                var columnSeparator = $" {new string('-', MaxWidth[property.Name])} |";
                if (ColumnTextJustification.TryGetValue(property.Name, out TextJustification value))
                    switch (value)
                    {
                        case TextJustification.Centered:
                            columnSeparator = columnSeparator.Replace("- ", ": ");
                            columnSeparator = columnSeparator.Replace(" -", " :");
                            break;
                        case TextJustification.Right:
                            columnSeparator = columnSeparator.Replace("- ", ": ");
                            break;
                        case TextJustification.Left:
                            columnSeparator = columnSeparator.Replace(" -", " :");
                            break;
                        case TextJustification.Justified:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(
                                $"Unrecognized TextJustification {value}");
                    }

                s += columnSeparator;
            }

            stringBuilder.AppendLine(s);

            foreach (var row in Items)
            {
                s = "|";
                foreach (var property in filteredPropertyNames)
                {
                    var value = GetValue(row, property);
                    var length = MaxWidth[property.Name] - value.Length;
                    s += $" {value.ToValidOutput()}{new string(' ', length)} |";
                }

                stringBuilder.AppendLine(s);
            }

            stringBuilder.AppendLine();

            return stringBuilder.ToString();
        }

        public void ToHtml(string fileName)
        {
            using var file = new StreamWriter(fileName);
            file.WriteLine(ToHtml());
        }

        public string ToHtml()
        {
            var stringBuilder = new StringBuilder();
            if (Items.Count == 0) return "";
            stringBuilder.AppendLine("<table style=\"border-collapse: collapse; width: 100%;\">");
            stringBuilder.AppendLine("<tr>");

            var filteredPropertyNames = FilterProperties();
            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
                if (ColumnNameOverrides.TryGetValue(property.Name, out string value))
                    headerName = value;

                stringBuilder.AppendLine(
                    $"<th style=\"text-align: center; background-color: #04163d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;\">{headerName.ToHtml()}</th>");
            }

            stringBuilder.AppendLine("</tr>");

            var rowNumber = 1;
            foreach (var row in Items)
            {
                stringBuilder.AppendLine("<tr>");
                foreach (var property in filteredPropertyNames)
                {
                    var valueProp = GetValue(row, property);
                    var color = rowNumber % 2 == 0 ? "#f2f2f2" : "white";

                    if (Operation != null && Operation.TryGetValue(property.Name, out List<HighlightOperator> value))
                    {
                        foreach (var item in value)
                            switch (item.Type)
                            {
                                case HighlightType.Decimal:
                                    try
                                    {
                                        var parsed = decimal.Parse(valueProp.Trim());
                                        foreach (var num in item.DecimalValue)
                                            switch (item.Operation)
                                            {
                                                case HighlightOperation.Differences
                                                    when decimal.Compare(Math.Round(parsed, Options.NumberDecimals),
                                                        Math.Round(num, Options.NumberDecimals)) != 0:
                                                    color = "#f9f948";
                                                    break;
                                                case HighlightOperation.Differences:
                                                    color = "#f9f948";
                                                    break;
                                                case HighlightOperation.Equality:
                                                    {
                                                        if (decimal.Compare(Math.Round(parsed, Options.NumberDecimals),
                                                            Math.Round(num, Options.NumberDecimals)) == 0)
                                                        {
                                                            color = "#f9f948";
                                                        }

                                                        break;
                                                    }
                                                default:
                                                    throw new ArgumentOutOfRangeException(
                                                        $"Unrecognized operation {item.Operation}");
                                            }
                                    }
                                    catch
                                    {
                                        // do nothing
                                    }

                                    break;
                                case HighlightType.String:
                                    try
                                    {
                                        foreach (var str in item.StringValue)
                                            switch (item.Operation)
                                            {
                                                case HighlightOperation.Differences
                                                    when valueProp.Trim() != str:
                                                    color = "#f9f948";
                                                    break;
                                                case HighlightOperation.Differences:
                                                    color = "#f9f948";
                                                    break;
                                                case HighlightOperation.Equality:
                                                    {
                                                        if (valueProp.Trim() == str)
                                                        {
                                                            color = "#f9f948";
                                                        }

                                                        break;
                                                    }
                                                default:
                                                    throw new ArgumentOutOfRangeException(
                                                        $"Unrecognized operation {item.Operation}");
                                            }
                                    }
                                    catch
                                    {
                                        // do nothing
                                    }

                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException($"Unrecognized type {item.Type}");
                            }

                    }

                    stringBuilder.AppendLine(
                        $"<td style=\"text-align: right; color: black; background-color: {color};padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;\">{valueProp.ToHtml()}</td>");
                }

                rowNumber++;
                stringBuilder.AppendLine("</tr>");
            }

            stringBuilder.AppendLine("</table>");

            return stringBuilder.ToString();
        }

        public void ToCsv(string fileName)
        {
            using var file = new StreamWriter(fileName);
            file.WriteLine(ToCsv());
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
            var stringBuilder = new StringBuilder();
            if (Items.Count == 0)
            {
                if (Options != null && Options.DiscardEmptyList)
                    return "";
            }
            var s = "";
            var filteredPropertyNames = FilterProperties();
            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
                if (ColumnNameOverrides.TryGetValue(property.Name, out string value))
                    headerName = value;

                s += $"{headerName.ToCsv()},";
            }

            s = s.Remove(s.Length - 1);
            stringBuilder.AppendLine(s);

            foreach (var row in Items)
            {
                s = (from property in filteredPropertyNames
                     select GetValue(row, property)).Aggregate("", (current, value) => current + $"{value.ToCsv()},");
                s = s.Remove(s.Length - 1);
                stringBuilder.AppendLine(s);
            }

            return stringBuilder.ToString();
        }

        public void ToSqlInsertString(string fileName)
        {
            using var file = new StreamWriter(fileName);
            file.WriteLine(ToSqlInsertString());
        }

        public string ToSqlInsertString()
        {
            var stringBuilder = new StringBuilder();
            if (Items.Count == 0) return "";
            var header = $"INSERT INTO {ClassName} (";
            var filteredPropertyNames = FilterProperties();
            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
                if (ColumnNameOverrides.TryGetValue(property.Name, out string value))
                    headerName = value;

                header += $"{headerName.ToSqlColumn()},";
            }

            header = header.Remove(header.Length - 1);
            header += ") VALUES (";

            foreach (var row in Items)
            {
                var s = "";
                foreach (var property in filteredPropertyNames)
                {
                    var obj = GetOriginalValue(row, property);

                    var p = obj switch
                    {
                        string z => "'" + z.ToSql() + "'",
                        int _ => obj.ToString().ToSql(),
                        long _ => obj.ToString().ToSql(),
                        bool _ => obj.ToString().ToSql() == "True" ? "1" : "0",
                        DateTime time => "'" + time.ToString("yyyy-MM-dd") + "'",
                        decimal value1 => value1.ToString("#0.0###"),
                        double value1 => value1.ToString("#0.0###"),
                        _ => (obj != null ? obj.ToString().ToSql() : "NULL")
                    };
                    s += $"{p},";
                }

                s = s.Remove(s.Length - 1);
                s += ");";
                stringBuilder.AppendLine($"{header}{s}");
            }

            return stringBuilder.ToString();
        }
    }
}