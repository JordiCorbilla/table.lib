//MIT License

//Copyright (c) 2020-2021 Jordi Corbilla

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
            if (list.Count == 0) return;
            if (options != null) Options = options;

            PropertyNames = new List<PropertyName>();
            MaxWidth = new Dictionary<string, int>();
            Items = list;
            var properties = typeof(T).GetProperties();
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
                                    var res = propertyInfo.GetValue(row, new object[] {index});
                                    if (!MaxWidth.ContainsKey(prop))
                                    {
                                        PropertyNames.Add(new PropertyName(prop, index, propertyIndex));
                                        MaxWidth.Add(prop, prop.Length);
                                    }

                                    if (res.ToString().Length > MaxWidth[prop])
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
                            if (!MaxWidth.ContainsKey(propertyInfo.Name))
                            {
                                PropertyNames.Add(new PropertyName(propertyInfo.Name));
                                MaxWidth.Add(propertyInfo.Name, propertyInfo.Name.Length);
                            }

                            var value = GetValue(row, new PropertyName(propertyInfo.Name));
                            if (value.Length > MaxWidth[propertyInfo.Name])
                                MaxWidth[propertyInfo.Name] = value.Length;
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
            if (!Operation.ContainsKey(operation.Field))
                Operation.Add(operation.Field, new List<HighlightOperator> {operation});
            else
                Operation[operation.Field].Add(operation);
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
                if (ColumnNameOverrides.ContainsKey(property.Name))
                    headerName = ColumnNameOverrides[property.Name];

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
                    var value = GetValue(row, property);
                    var length = MaxWidth[property.Name] - value.Length;
                    var output = value.ToValidOutput();
                    if (ColumnTextJustification.ContainsKey(property.Name))
                        switch (ColumnTextJustification[property.Name])
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
                if (ColumnNameOverrides.ContainsKey(property.Name))
                    headerName = ColumnNameOverrides[property.Name];

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
                    var value = GetValue(row, property);
                    var length = MaxWidth[property.Name] - value.Length;
                    var output = value.ToValidOutput();
                    if (ColumnTextJustification.ContainsKey(property.Name))
                    {
                        switch (ColumnTextJustification[property.Name])
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
                if (ColumnNameOverrides.ContainsKey(property.Name))
                    headerName = ColumnNameOverrides[property.Name];

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
                    var value = GetValue(row, property);
                    var length = MaxWidth[property.Name] - value.Length;
                    var output = value.ToValidOutput();
                    if (ColumnTextJustification.ContainsKey(property.Name))
                    {
                        switch (ColumnTextJustification[property.Name])
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
            if (Items.Count == 0) return;
            var stringBuilder = new StringBuilder();
            var s = "|";

            var filteredPropertyNames = FilterProperties();

            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
                if (ColumnNameOverrides.ContainsKey(property.Name))
                    headerName = ColumnNameOverrides[property.Name];

                var length = MaxWidth[property.Name] - headerName.Length;
                s += $" {headerName.ToValidOutput()}{new string(' ', length)} |";
            }

            stringBuilder.AppendLine(s);

            s = "|";
            foreach (var property in filteredPropertyNames)
            {
                var columnSeparator = $" {new string('-', MaxWidth[property.Name])} |";
                if (ColumnTextJustification.ContainsKey(property.Name))
                    switch (ColumnTextJustification[property.Name])
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
                                $"Unrecognized TextJustification {ColumnTextJustification[property.Name]}");
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

            using var file = new StreamWriter(fileName);
            file.WriteLine(stringBuilder.ToString());

            if (consoleVerbose)
                Console.WriteLine(stringBuilder.ToString());
        }

        public void ToHtml(string fileName)
        {
            var stringBuilder = new StringBuilder();
            if (Items.Count == 0) return;
            stringBuilder.AppendLine("<table style=\"border-collapse: collapse; width: 100%;\">");
            stringBuilder.AppendLine("<tr>");

            var filteredPropertyNames = FilterProperties();
            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
                if (ColumnNameOverrides.ContainsKey(property.Name))
                    headerName = ColumnNameOverrides[property.Name];

                stringBuilder.AppendLine(
                    $"<th style=\"text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;\">{headerName.ToHtml()}</th>");
            }

            stringBuilder.AppendLine("</tr>");

            var rowNumber = 1;
            foreach (var row in Items)
            {
                stringBuilder.AppendLine("<tr>");
                foreach (var property in filteredPropertyNames)
                {
                    var color = rowNumber % 2 == 0 ? "#f2f2f2" : "white";
                    var value = GetValue(row, property);
                    stringBuilder.AppendLine(
                        $"<td style=\"text-align: right; color: black; background-color: {color};padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;\">{value.ToHtml()}</td>");
                }

                rowNumber++;
                stringBuilder.AppendLine("</tr>");
            }

            stringBuilder.AppendLine("</table>");

            using var file = new StreamWriter(fileName);
            file.WriteLine(stringBuilder.ToString());
        }

        public void ToCsv(string fileName)
        {
            var stringBuilder = new StringBuilder();
            if (Items.Count == 0) return;
            var s = "";
            var filteredPropertyNames = FilterProperties();
            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
                if (ColumnNameOverrides.ContainsKey(property.Name))
                    headerName = ColumnNameOverrides[property.Name];

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

            using var file = new StreamWriter(fileName);
            file.WriteLine(stringBuilder.ToString());
        }
    }
}