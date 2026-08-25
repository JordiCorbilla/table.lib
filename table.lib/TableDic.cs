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
    public class TableDic<TV, T> : Base<T> where T : class
    {
        public TableDic(Dictionary<TV, T> dictionary, Options options = null)
        {
            if (options != null)
                Options = options;
            PropertyNames = [];
            MaxWidth = [];
            Keys = new List<TV>(dictionary.Count);
            Items = new List<T>(dictionary.Count);
            foreach (var (key, value) in dictionary)
            {
                Keys.Add(key);
                Items.Add(value);
            }

            var properties = TypeMetadata<T>.Properties;

            //Add the additional Key
            PropertyNames.Add(new PropertyName(Options.KeyName));
            MaxWidth.Add(Options.KeyName, Options.KeyName.Length);

            foreach (var property in properties)
            {
                PropertyNames.Add(new PropertyName(property.Name) { Accessor = property });
                MaxWidth.Add(property.Name, property.Name.Length);
            }

            //Workout the key length
            foreach (var row in Keys)
            {
                var subValueLength = ObjectToString(row);

                if (subValueLength.Length > MaxWidth[Options.KeyName])
                    MaxWidth[Options.KeyName] = subValueLength.Length;
            }

            foreach (var row in Items)
                if (properties.Count != 0)
                {
                    foreach (var property in properties)
                    {
                        var value = ObjectToString(property.GetValue(row));
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
                            foreach (var result in enumerable)
                            {
                                var valueName = $"{Options.DynamicName}{index}";
                                if (!MaxWidth.TryGetValue(valueName, out var width))
                                {
                                    PropertyNames.Add(new PropertyName(valueName, index, propertyIndex));
                                    width = valueName.Length;
                                    MaxWidth.Add(valueName, width);
                                }

                                var text = ObjectToString(result);
                                if (text.Length > width) MaxWidth[valueName] = text.Length;
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

        public List<TV> Keys { get; set; }

        public TableDic<TV, T> FilterColumns(string[] columns, FilterAction action = FilterAction.Exclude)
        {
            var filter = columns.ToDictionary(column => column, column => false);
            ColumnFilter = filter;
            ColumnAction = action;
            return this;
        }

        public TableDic<TV, T> HighlightValue(HighlightOperator operation)
        {
            if (!Operation.TryGetValue(operation.Field, out List<HighlightOperator> value))
                Operation.Add(operation.Field, [operation]);
            else
                value.Add(operation);
            return this;
        }

        public TableDic<TV, T> OverrideColumnsNames(Dictionary<string, string> columns)
        {
            ColumnNameOverrides = columns;
            foreach (var (key, value) in ColumnNameOverrides)
                if (value.Length > MaxWidth[key])
                    MaxWidth[key] = value.Length;
            return this;
        }

        public TableDic<TV, T> ColumnContentTextJustification(Dictionary<string, TextJustification> columns)
        {
            ColumnTextJustification = columns;
            return this;
        }

        public TableDic<TV, T> HighlightRows(ConsoleColor backgroundColor, ConsoleColor foregroundColor)
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

                var totalLength = $"{new string(' ', length)}{headerName.ToValidOutput()}".Length;
                var remaining = totalLength - $"{new string(' ', length / 2)}{headerName.ToValidOutput()}".Length;
                s += $" {new string(' ', length / 2)}{headerName.ToValidOutput()}{new string(' ', remaining)} |";
            }

            Console.WriteLine(s);

            s = filteredPropertyNames.Aggregate("|",
                (current, name) => current + $" {new string('-', MaxWidth[name.Name])} |");

            Console.WriteLine(s);

            for (var index = 0; index < Items.Count; index++)
            {
                var row = Items[index];
                Console.Write("|");
                foreach (var property in filteredPropertyNames)
                    if (property.Name == Options.KeyName)
                    {
                        var keyValueParsed = ObjectToString(Keys[index]);

                        var lengthParsed = MaxWidth[property.Name] - keyValueParsed.Length;

                        if (ColumnTextJustification.TryGetValue(property.Name, out TextJustification value))
                            switch (value)
                            {
                                case TextJustification.Centered:
                                    var totalLength = $"{new string(' ', lengthParsed)}{keyValueParsed.ToValidOutput()}"
                                        .Length;
                                    var remaining =
                                        totalLength -
                                        $"{new string(' ', lengthParsed / 2)}{keyValueParsed.ToValidOutput()}".Length;
                                    ConsoleRender(
                                        $"{new string(' ', lengthParsed / 2)}{keyValueParsed.ToValidOutput()}{new string(' ', remaining)}",
                                        property.Name);
                                    break;
                                case TextJustification.Right:
                                    ConsoleRender($"{new string(' ', lengthParsed)}{keyValueParsed.ToValidOutput()}",
                                        property.Name);
                                    break;
                                case TextJustification.Left:
                                    ConsoleRender($"{keyValueParsed.ToValidOutput()}{new string(' ', lengthParsed)}",
                                        property.Name);
                                    break;
                                case TextJustification.Justified:
                                    ConsoleRender($"{keyValueParsed.ToValidOutput()}{new string(' ', lengthParsed)}",
                                        property.Name);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        else
                            ConsoleRender($"{keyValueParsed.ToValidOutput()}{new string(' ', lengthParsed)}",
                                property.Name);
                    }
                    else
                    {
                        var valueProp = GetValue(row, property);
                        var length = MaxWidth[property.Name] - valueProp.Length;

                        if (ColumnTextJustification.TryGetValue(property.Name, out TextJustification value))
                            switch (value)
                            {
                                case TextJustification.Centered:
                                    var totalLength = $"{new string(' ', length)}{valueProp.ToValidOutput()}".Length;
                                    var remaining =
                                        totalLength - $"{new string(' ', length / 2)}{valueProp.ToValidOutput()}".Length;
                                    ConsoleRender(
                                        $"{new string(' ', length / 2)}{valueProp.ToValidOutput()}{new string(' ', remaining)}",
                                        property.Name);
                                    break;
                                case TextJustification.Right:
                                    ConsoleRender($"{new string(' ', length)}{valueProp.ToValidOutput()}", property.Name);
                                    break;
                                case TextJustification.Left:
                                    ConsoleRender($"{valueProp.ToValidOutput()}{new string(' ', length)}", property.Name);
                                    break;
                                case TextJustification.Justified:
                                    ConsoleRender($"{valueProp.ToValidOutput()}{new string(' ', length)}", property.Name);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        else
                            ConsoleRender($"{valueProp.ToValidOutput()}{new string(' ', length)}", property.Name);
                    }

                Console.Write(Environment.NewLine);
            }

            Console.WriteLine();
        }

        public override string ToString()
        {
            if (Items.Count == 0) return "";
            var s = "|";
            var stringBuilder = new StringBuilder();

            var filteredPropertyNames = FilterProperties();

            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
                if (ColumnNameOverrides.TryGetValue(property.Name, out string value))
                    headerName = value;

                var length = MaxWidth[property.Name] - headerName.Length;

                var totalLength = $"{new string(' ', length)}{headerName.ToValidOutput()}".Length;
                var remaining = totalLength - $"{new string(' ', length / 2)}{headerName.ToValidOutput()}".Length;
                s += $" {new string(' ', length / 2)}{headerName.ToValidOutput()}{new string(' ', remaining)} |";
            }

            stringBuilder.AppendLine(s);

            s = filteredPropertyNames.Aggregate("|",
                (current, name) => current + $" {new string('-', MaxWidth[name.Name])} |");

            stringBuilder.AppendLine(s);

            for (var index = 0; index < Items.Count; index++)
            {
                var row = Items[index];
                stringBuilder.Append('|');
                foreach (var property in filteredPropertyNames)
                    if (property.Name == Options.KeyName)
                    {
                        var keyValueParsed = ObjectToString(Keys[index]);

                        var lengthParsed = MaxWidth[property.Name] - keyValueParsed.Length;

                        if (ColumnTextJustification.TryGetValue(property.Name, out TextJustification value))
                            switch (value)
                            {
                                case TextJustification.Centered:
                                    var totalLength = $"{new string(' ', lengthParsed)}{keyValueParsed.ToValidOutput()}"
                                        .Length;
                                    var remaining =
                                        totalLength -
                                        $"{new string(' ', lengthParsed / 2)}{keyValueParsed.ToValidOutput()}".Length;
                                    stringBuilder.Append(' ');
                                    stringBuilder.Append($"{new string(' ', lengthParsed / 2)}{keyValueParsed.ToValidOutput()}{new string(' ', remaining)}");
                                    stringBuilder.Append(" |");
                                    break;
                                case TextJustification.Right:
                                    stringBuilder.Append(' ');
                                    stringBuilder.Append($"{new string(' ', lengthParsed)}{keyValueParsed.ToValidOutput()}");
                                    stringBuilder.Append(" |");
                                    break;
                                case TextJustification.Left:
                                    stringBuilder.Append(' ');
                                    stringBuilder.Append($"{keyValueParsed.ToValidOutput()}{new string(' ', lengthParsed)}");
                                    stringBuilder.Append(" |");
                                    break;
                                case TextJustification.Justified:
                                    stringBuilder.Append(' ');
                                    stringBuilder.Append($"{keyValueParsed.ToValidOutput()}{new string(' ', lengthParsed)}");
                                    stringBuilder.Append(" |");
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        else
                        {
                            stringBuilder.Append(' ');
                            stringBuilder.Append($"{keyValueParsed.ToValidOutput()}{new string(' ', lengthParsed)}");
                            stringBuilder.Append(" |");
                        }
                    }
                    else
                    {
                        var valueProp = GetValue(row, property);
                        var length = MaxWidth[property.Name] - valueProp.Length;

                        if (ColumnTextJustification.TryGetValue(property.Name, out TextJustification value))
                            switch (value)
                            {
                                case TextJustification.Centered:
                                    var totalLength = $"{new string(' ', length)}{valueProp.ToValidOutput()}".Length;
                                    var remaining = totalLength - $"{new string(' ', length / 2)}{valueProp.ToValidOutput()}".Length;
                                    stringBuilder.Append(' ');
                                    stringBuilder.Append($"{new string(' ', length / 2)}{valueProp.ToValidOutput()}{new string(' ', remaining)}");
                                    stringBuilder.Append(" |");
                                    break;
                                case TextJustification.Right:
                                    stringBuilder.Append(' ');
                                    stringBuilder.Append($"{new string(' ', length)}{valueProp.ToValidOutput()}");
                                    stringBuilder.Append(" |");
                                    break;
                                case TextJustification.Left:
                                    stringBuilder.Append(' ');
                                    stringBuilder.Append($"{valueProp.ToValidOutput()}{new string(' ', length)}");
                                    stringBuilder.Append(" |");
                                    break;
                                case TextJustification.Justified:
                                    stringBuilder.Append(' ');
                                    stringBuilder.Append($"{valueProp.ToValidOutput()}{new string(' ', length)}");
                                    stringBuilder.Append(" |");
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        else
                        {
                            stringBuilder.Append(' ');
                            stringBuilder.Append($"{valueProp.ToValidOutput()}{new string(' ', length)}");
                            stringBuilder.Append(" |");
                        }
                    }

                stringBuilder.Append(Environment.NewLine);
            }

            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }

        public void ToCsv(string fileName)
        {
            using var file = new StreamWriter(fileName);
            WriteCsv(file);
        }

        public string ToCsv()
        {
            return Items.Count == 0 ? string.Empty : RenderingHelpers.RenderToString(WriteCsv);
        }

        public void WriteCsv(TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            if (Items.Count == 0) return;

            var properties = FilterProperties();
            for (var index = 0; index < properties.Count; index++)
            {
                if (index > 0) writer.Write(',');
                var property = properties[index];
                var header = ColumnNameOverrides.TryGetValue(property.Name, out var value) ? value : property.Name;
                RenderingHelpers.WriteCsvValue(writer, header);
            }
            writer.WriteLine();

            for (var rowIndex = 0; rowIndex < Items.Count; rowIndex++)
            {
                for (var index = 0; index < properties.Count; index++)
                {
                    if (index > 0) writer.Write(',');
                    var property = properties[index];
                    var value = property.Name == Options.KeyName
                        ? ObjectToString(Keys[rowIndex])
                        : GetValue(Items[rowIndex], property);
                    RenderingHelpers.WriteCsvValue(writer, value);
                }
                writer.WriteLine();
            }
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

            for (var rowIndex = 0; rowIndex < Items.Count; rowIndex++)
            {
                writer.Write('|');
                foreach (var property in properties)
                {
                    var value = property.Name == Options.KeyName
                        ? ObjectToString(Keys[rowIndex])
                        : GetValue(Items[rowIndex], property);
                    RenderingHelpers.WriteValueCell(writer, value.ToValidOutput(), MaxWidth[property.Name]);
                }
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

            for (var rowIndex = 0; rowIndex < Items.Count; rowIndex++)
            {
                writer.WriteLine("<tr>");
                var color = (rowIndex + 1) % 2 == 0 ? "#f2f2f2" : "white";
                foreach (var property in properties)
                {
                    var value = property.Name == Options.KeyName
                        ? ObjectToString(Keys[rowIndex])
                        : GetValue(Items[rowIndex], property);
                    writer.Write("<td style=\"text-align: right; color: black; background-color: ");
                    writer.Write(color);
                    writer.Write(";padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;\">");
                    writer.Write(value.ToHtml());
                    writer.WriteLine("</td>");
                }
                writer.WriteLine("</tr>");
            }
            writer.WriteLine("</table>");
        }

        public static TableDic<TV, T> Add(Dictionary<TV, T> dictionary)
        {
            return new TableDic<TV, T>(dictionary);
        }

        public static TableDic<TV, T> Add(Dictionary<TV, T> dictionary, Options options)
        {
            return new TableDic<TV, T>(dictionary, options);
        }
    }
}
