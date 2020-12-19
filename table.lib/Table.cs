﻿//MIT License

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
using System.Linq;
using System.Text;

namespace table.lib
{
    public class Table<T>
    {
        public List<PropertyName> PropertyNames { get; set; }
        public Dictionary<string, int> MaxWidth { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, bool> ColumnFilter { get; set; } = new Dictionary<string, bool>();
        private List<T> Items { get; }

        public static Table<T> Add(List<T> list)
        {
            return new Table<T>(list);
        }

        public Table<T> OverrideColumns(Dictionary<string, string> columns)
        {
            Headers = columns;
            foreach (var (key, value) in Headers)
            {
                if (value.Length > MaxWidth[key])
                    MaxWidth[key] = value.Length;
            }
            return this;
        }

        public Table<T> FilterColumns(Dictionary<string, bool> columns)
        {
            ColumnFilter = columns;
            return this;
        }

        public Table(List<T> list)
        {
            if (list.Count <= 0) return;
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
            {
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
                        // Indexed property
                        if (propertyInfo.GetIndexParameters().Length > 0)
                        {
                            bool reading = true;
                            var index = 0;
                            while (reading)
                            {
                                try
                                {
                                    var res = propertyInfo.GetValue(row, new object[] {index});
                                    if (!MaxWidth.ContainsKey($"Dynamic{index}"))
                                    {
                                        PropertyNames.Add(new PropertyName($"Dynamic{index}", index, propertyIndex));
                                        MaxWidth.Add($"Dynamic{index}", $"Dynamic{index}".Length);
                                    }

                                    if (res.ToString().Length > MaxWidth[$"Dynamic{index}"])
                                        MaxWidth[$"Dynamic{index}"] = res.ToString().Length;
                                    index++;
                                }
                                catch (Exception)
                                {
                                    reading = false;
                                }
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
        }

        private static string GetValue(T item, PropertyName property)
        {
            if (string.IsNullOrEmpty(property.Name)) return null;
            object value;
            if (property.IsCollection)
            {
                var prop = item.GetType().GetProperties()[property.PropertyIndex];
                value = prop.GetValue(item, new object[] { property.Index });
            }
            else
            {
                var properties = item.GetType().GetProperty(property.Name);
                value = properties?.GetValue(item,  null);
            }

            return value switch
            {
                string s => s,
                int _ => value.ToString(),
                bool _ => value.ToString(),
                DateTime time => time.ToString("dd-MMM-yyyy"),
                decimal value1 => value1.ToString("#,##0.00"),
                double value1 => value1.ToString("#,##0.00"),
                _ => (value != null ? value.ToString() : "")
            };

        }

        public void WriteToConsole()
        {
            if (Items.Count <= 0) return;
            var s = "|";
            foreach (var property in PropertyNames)
            {
                var headerName = property.Name;
                if (ColumnFilter.ContainsKey(headerName)) continue;
                if (Headers != null && Headers.Count > 0)
                {
                    if (Headers.ContainsKey(property.Name))
                        headerName = Headers[property.Name];
                }

                var length = MaxWidth[property.Name] - headerName.Length;
                s += $" {headerName.ToValidOutput()}{new string(' ', length)} |";
            }
            Console.WriteLine(s);

            s = PropertyNames.Where(name => !ColumnFilter.ContainsKey(name.Name))
                .Aggregate("|", (current, name) => current + $" {new string('-', MaxWidth[name.Name])} |");

            Console.WriteLine(s);

            foreach (var row in Items)
            {
                s = "|";
                foreach (var property in PropertyNames)
                {
                    if (ColumnFilter.ContainsKey(property.Name)) continue;
                    var value = GetValue(row, property);
                    var length = MaxWidth[property.Name] - value.Length;
                    s += $" {value.ToValidOutput()}{new string(' ', length)} |";
                }
                Console.WriteLine(s);
            }
            Console.WriteLine();
        }

        public void WriteToHtml(string fileName)
        {
            var stringBuilder = new StringBuilder();
            if (Items.Count <= 0) return;
            stringBuilder.AppendLine("<table style=\"border-collapse: collapse; width: 100%;\">");
            stringBuilder.AppendLine("<tr>");
            foreach (var property in PropertyNames)
            {
                var headerName = property.Name;
                if (ColumnFilter.ContainsKey(headerName)) continue;
                if (Headers != null && Headers.Count > 0)
                {
                    if (Headers.ContainsKey(property.Name))
                        headerName = Headers[property.Name];
                }

                stringBuilder.AppendLine($"<th style=\"text-align: center; background-color: #052a3d; color: white;padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;\">{headerName.ToHtml()}</th>");
            }

            stringBuilder.AppendLine("</tr>");

            var rowNumber = 1;
            foreach (var row in Items)
            {
                stringBuilder.AppendLine("<tr>");
                foreach (var property in PropertyNames)
                {
                    if (ColumnFilter.ContainsKey(property.Name)) continue;
                    var color = (rowNumber % 2 == 0) ? "#f2f2f2" : "white";
                    var value = GetValue(row, property);
                    stringBuilder.AppendLine($"<td style=\"text-align: right; color: black; background-color: {color};padding: 4px;border: 1px solid #dddddd; font-family:monospace; font-size: 14px;\">{value.ToHtml()}</td>");
                }
                rowNumber++;
                stringBuilder.AppendLine("</tr>");
            }

            stringBuilder.AppendLine("</table>");

            using var file = new System.IO.StreamWriter(fileName);
            file.WriteLine(stringBuilder.ToString());
        }

        public void WriteToCsv(string fileName)
        {
            var stringBuilder = new StringBuilder();
            if (Items.Count <= 0) return;
            var s = "";
            foreach (var property in PropertyNames)
            {
                var headerName = property.Name;
                if (ColumnFilter.ContainsKey(headerName)) continue;
                if (Headers != null && Headers.Count > 0)
                {
                    if (Headers.ContainsKey(property.Name))
                        headerName = Headers[property.Name];
                }

                s += $"{headerName.ToCsv()},";
            }

            s = s.Remove(s.Length - 1);
            stringBuilder.AppendLine(s);

            foreach (var row in Items)
            {
                s = (from property in PropertyNames where !ColumnFilter.ContainsKey(property.Name) 
                    select GetValue(row, property)).Aggregate("", (current, value) => current + $"{value.ToCsv()},");
                s = s.Remove(s.Length - 1);
                stringBuilder.AppendLine(s);
            }

            using var file = new System.IO.StreamWriter(fileName);
            file.WriteLine(stringBuilder.ToString());
        }
    }
}