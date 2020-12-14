//MIT License

//Copyright (c) 2020 Jordi Corbilla

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

namespace table.lib
{
    public class Table<T>
    {
        public List<string> PropertyNames { get; set; }
        public Dictionary<string, int> MaxWidth { get; set; }
        private List<T> Items { get; }

        public static Table<T> Add(List<T> list)
        {
            return new Table<T>(list);
        }

        public static void WriteToConsole(List<T> list)
        {
            new Table<T>(list).WriteConsole();
        }

        public Table(List<T> list)
        {
            if (list.Count <= 0) return;
            PropertyNames = new List<string>();
            MaxWidth = new Dictionary<string, int>();
            Items = list;
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                PropertyNames.Add(property.Name);
                MaxWidth.Add(property.Name, property.Name.Length);
            }

            foreach (var row in Items)
            {
                foreach (var property in PropertyNames)
                {
                    var value = GetValue(row, property);
                    if (value.Length > MaxWidth[property])
                        MaxWidth[property] = value.Length;
                }
            }
        }

        private static string GetValue(T item, string property)
        {
            var value = item.GetType().GetProperty(property)?.GetValue(item, null);
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

        public void WriteConsole()
        {
            if (Items.Count <= 0) return;
            var s = "|";
            foreach (var property in PropertyNames)
            {
                var length = MaxWidth[property] - property.Length;
                s += $" {property}{new string(' ', length)} |";
            }
            Console.WriteLine(s);

            s = PropertyNames.Aggregate("|", (current, property) => current + $" {new string('-', MaxWidth[property])} |");
            Console.WriteLine(s);

            foreach (var row in Items)
            {
                s = "|";
                foreach (var property in PropertyNames)
                {
                    var value = GetValue(row, property);
                    var length = MaxWidth[property] - value.Length;
                    s += $" {value}{new string(' ', length)} |";
                }
                Console.WriteLine(s);
            }
        }
    }
}
