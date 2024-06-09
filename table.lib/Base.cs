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

using System;
using System.Collections.Generic;

namespace table.lib
{
    public class Base<T>
    {
        public string ClassName { get; set; }
        public List<PropertyName> PropertyNames { get; set; }
        public Dictionary<string, int> MaxWidth { get; set; }
        public Dictionary<string, string> ColumnNameOverrides { get; set; } = [];
        public Dictionary<string, bool> ColumnFilter { get; set; } = [];
        public FilterAction ColumnAction { get; set; } = FilterAction.Exclude;
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Green;
        public List<T> Items { get; set; } = [];

        public Dictionary<string, List<HighlightOperator>> Operation { get; set; } = [];

        public Options Options { get; set; } = new Options();

        public Dictionary<string, TextJustification> ColumnTextJustification { get; set; } = [];

        public string GetValue(T item, PropertyName property)
        {
            if (string.IsNullOrEmpty(property.Name)) return null;
            object value;
            if (property.IsCollection)
            {
                var prop = item.GetType().GetProperties()[property.PropertyIndex];
                value = prop.GetValue(item, [property.Index]);
            }
            else
            {
                var properties = item.GetType().GetProperty(property.Name);
                value = properties?.GetValue(item, null);
            }

            return ObjectToString(value);
        }

        public object GetOriginalValue(T item, PropertyName property)
        {
            if (string.IsNullOrEmpty(property.Name)) return null;
            object value;
            if (property.IsCollection)
            {
                var prop = item.GetType().GetProperties()[property.PropertyIndex];
                value = prop.GetValue(item, [property.Index]);
            }
            else
            {
                var properties = item.GetType().GetProperty(property.Name);
                value = properties?.GetValue(item, null);
            }

            return value;
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

        public void ConsoleRender(string value, string column)
        {
            Console.Write(" ");
            Console.BackgroundColor = BackgroundColor;
            Console.ForegroundColor = ForegroundColor;

            if (Operation != null)
                if (Operation.TryGetValue(column, out List<HighlightOperator> operation))
                    foreach (var item in operation)
                        switch (item.Type)
                        {
                            case HighlightType.Decimal:
                                try
                                {
                                    var parsed = decimal.Parse(value.Trim());
                                    foreach (var num in item.DecimalValue)
                                        switch (item.Operation)
                                        {
                                            case HighlightOperation.Differences
                                                when decimal.Compare(Math.Round(parsed, Options.NumberDecimals),
                                                    Math.Round(num, Options.NumberDecimals)) != 0:
                                                Console.BackgroundColor = item.BackgroundColorIfDifferent;
                                                Console.ForegroundColor = item.ForegroundColorIfDifferent;
                                                break;
                                            case HighlightOperation.Differences:
                                                Console.BackgroundColor = item.BackgroundColorIfEqual;
                                                Console.ForegroundColor = item.ForegroundColorIfEqual;
                                                break;
                                            case HighlightOperation.Equality:
                                                {
                                                    if (decimal.Compare(Math.Round(parsed, Options.NumberDecimals),
                                                        Math.Round(num, Options.NumberDecimals)) == 0)
                                                    {
                                                        Console.BackgroundColor = item.BackgroundColorIfEqual;
                                                        Console.ForegroundColor = item.ForegroundColorIfEqual;
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
                                                when value.Trim() != str:
                                                Console.BackgroundColor = item.BackgroundColorIfDifferent;
                                                Console.ForegroundColor = item.ForegroundColorIfDifferent;
                                                break;
                                            case HighlightOperation.Differences:
                                                Console.BackgroundColor = item.BackgroundColorIfEqual;
                                                Console.ForegroundColor = item.ForegroundColorIfEqual;
                                                break;
                                            case HighlightOperation.Equality:
                                                {
                                                    if (value.Trim() == str)
                                                    {
                                                        Console.BackgroundColor = item.BackgroundColorIfEqual;
                                                        Console.ForegroundColor = item.ForegroundColorIfEqual;
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

            Console.Write(value);
            Console.ResetColor();
            Console.Write(" |");
        }
    }
}