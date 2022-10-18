//MIT License

//Copyright (c) 2020-2022 Jordi Corbilla

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
using System.Linq;
using System.Text;

namespace table.lib
{
    public class DbTable
    {
        public IEnumerable<IDictionary<string, object>> Items { get; set; }
        public List<PropertyName> PropertyNames { get; set; }
        public Dictionary<string, int> MaxWidth { get; set; }
        public Options Options { get; set; } = new Options();
        public Dictionary<string, bool> ColumnFilter { get; set; } = new Dictionary<string, bool>();
        public FilterAction ColumnAction { get; set; } = FilterAction.Exclude;

        public DbTable(IEnumerable<IDictionary<string, object>> list, Options options = null)
        {
            PropertyNames = new List<PropertyName>();
            MaxWidth = new Dictionary<string, int>();
            Items = list;
            if (options != null)
                Options = options;
            var addedProperties = false;
            foreach (var row in Items)
            {
                var columnIndex = 0;
                foreach (var (key, value) in row)
                    if (!addedProperties)
                    {
                        PropertyNames.Add(new PropertyName(key, columnIndex++, columnIndex));
                        MaxWidth.Add(key, key.Length);
                        if (value != null && ObjectToString(value).Length > MaxWidth[key])
                            MaxWidth[key] = ObjectToString(value).Length;
                    }
                    else
                    {
                        if (value != null && ObjectToString(value).Length > MaxWidth[key])
                            MaxWidth[key] = ObjectToString(value).Length;
                    }

                addedProperties = true;
            }
        }

        public static DbTable Add(IEnumerable<IDictionary<string, object>> list, Options options = null)
        {
            return new DbTable(list, options);
        }

        public DbTable FilterColumns(string[] columns, FilterAction action = FilterAction.Exclude)
        {
            var filter = columns.ToDictionary(column => column, column => false);
            ColumnFilter = filter;
            ColumnAction = action;
            return this;
        }

        public string ObjectToString(object value)
        {
            return value switch
            {
                string s => s,
                int _ => value.ToString(),
                long _ => value.ToString(),
                bool _ => value.ToString(),
                DateTime time => time.ToString(Options.DateFormat),
                decimal value1 => value1.ToString(Options.DecimalFormat),
                double value1 => value1.ToString(Options.DecimalFormat),
                _ => (value != null ? value.ToString() : "")
            };
        }

        public List<PropertyName> FilterProperties()
        {
            var filteredPropertyNames = new List<PropertyName>();

            foreach (var propertyName in PropertyNames)
                switch (ColumnAction)
                {
                    case FilterAction.Include:
                    {
                        if (ColumnFilter.ContainsKey(propertyName.Name))
                            filteredPropertyNames.Add(propertyName);
                        break;
                    }
                    case FilterAction.Exclude:
                    {
                        if (!ColumnFilter.ContainsKey(propertyName.Name))
                            filteredPropertyNames.Add(propertyName);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return filteredPropertyNames;
        }

        public override string ToString()
        {
            if (!Items.Any()) return string.Empty;
            var s = "|";
            var sb = new StringBuilder();

            var filteredPropertyNames = FilterProperties();

            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
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
                foreach (var (key, value) in row)
                {
                    var text = value ?? "";
                    var obj = ObjectToString(text);
                    var length = MaxWidth[key] - obj.Length;
                    var output = obj.ToValidOutput();
                    sb.Append(' ');
                    sb.Append($"{output}{new string(' ', length)}");
                    sb.Append(" |");
                }

                sb.Append(Environment.NewLine);
            }

            sb.AppendLine("");
            return sb.ToString();
        }

        public string ToSpecFlowString()
        {
            if (!Items.Any()) return string.Empty;
            var s = "|";
            var sb = new StringBuilder();

            var filteredPropertyNames = FilterProperties();

            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;
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
                foreach (var (key, value) in row)
                {
                    var text = value ?? "";
                    var obj = ObjectToString(text);
                    var length = MaxWidth[key] - obj.Length;
                    var output = obj.ToValidOutput();
                    sb.Append(' ');
                    sb.Append($"{output}{new string(' ', length)}");
                    sb.Append(" |");
                }

                sb.Append(Environment.NewLine);
            }

            sb.AppendLine("");
            return sb.ToString();
        }

        public string ToSqlInsertString()
        {
            var stringBuilder = new StringBuilder();
            if (!Items.Any()) return "";
            var header = $"INSERT INTO Table1 (";
            var filteredPropertyNames = FilterProperties();
            foreach (var property in filteredPropertyNames)
            {
                var headerName = property.Name;

                header += $"{headerName.ToSqlColumn()},";
            }

            header = header.Remove(header.Length - 1);
            header += ") VALUES (";

            foreach (var row in Items)
            {
                var s = "";

                foreach (var (key, value) in row)
                {
                    var obj = value;
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