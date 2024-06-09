using System;
using System.Collections.Generic;
using System.Text;
using table.lib.Entities;

namespace table.lib
{
    public class Pivot<T>
    {
        public Dictionary<string, List<DynamicType>> Data { get; set; }
        public string ClassName { get; set; }
        public List<PropertyName> PropertyNames { get; set; }
        public int MaxWidthProperties { get; set; }
        public Dictionary<int, int> MaxWidthData { get; set; }
        public Dictionary<string, string> ColumnNameOverrides { get; set; } = [];
        public Dictionary<string, bool> ColumnFilter { get; set; } = [];
        public FilterAction ColumnAction { get; set; } = FilterAction.Exclude;
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
        public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Green;
        public List<T> Items { get; set; } = [];

        public Dictionary<string, List<HighlightOperator>> Operation { get; set; } =
            [];

        public Options Options { get; set; } = new Options();

        public Dictionary<string, TextJustification> ColumnTextJustification { get; set; } =
            [];

        /// <summary>
        ///     Where the magic happens.
        ///     Retrieve the entire content of the collection using reflection
        /// </summary>
        /// <param name="list"></param>
        /// <param name="options"></param>
        public Pivot(List<T> list)
        {
            Data = [];

            PropertyNames = [];
            MaxWidthProperties = "Field".Length;
            MaxWidthData = [];
            Items = list;
            var properties = typeof(T).GetProperties();
            ClassName = typeof(T).Name;
            foreach (var property in properties)
            {
                PropertyNames.Add(new PropertyName(property.Name));
                if (property.Name.Length > MaxWidthProperties)
                    MaxWidthProperties = property.Name.Length;
                Data.Add(property.Name, []);
            }

            var index = 0;
            foreach (var row in Items)
            {
                if (properties.Length != 0)
                {
                    MaxWidthData.Add(index, $"T{index}".Length);
                    foreach (var property in PropertyNames)
                    {
                        var value = GetValue(row, new PropertyName(property.Name));
                        Data[property.Name].Add(new DynamicType { DataType = "string", Value = value });


                        if (value.Length > MaxWidthData[index])
                            MaxWidthData[index] = value.Length;
                    }
                }
                index++;
            }
        }

        public static Pivot<T> Add(List<T> list)
        {
            return new Pivot<T>(list);
        }

        public override string ToString()
        {
            if (Data.Count == 0) return string.Empty;

            var s = "|";
            var sb = new StringBuilder();

            var header = "Field";
            var length = MaxWidthProperties - header.Length;
            var totalLength = $"{new string(' ', length)}{header}".Length;
            var remaining = totalLength - $"{new string(' ', length / 2)}{header}".Length;
            s += $" {new string(' ', length / 2)}{header}{new string(' ', remaining)} |";

            foreach (var col in MaxWidthData)
            {
                header = $"T{col.Key}";
                length = MaxWidthData[col.Key] - header.Length;
                totalLength = $"{new string(' ', length)}{header}".Length;
                remaining = totalLength - $"{new string(' ', length / 2)}{header}".Length;
                s += $" {new string(' ', length / 2)}{header}{new string(' ', remaining)} |";
            }

            sb.AppendLine(s);

            s = "|";
            s += $" {new string('-', MaxWidthProperties)} |";

            foreach (var col in MaxWidthData)
            {
                s += $" {new string('-', MaxWidthData[col.Key])} |";
            }

            sb.AppendLine(s);

            foreach (var p in Data)
            {
                s = "|";
                header = p.Key;
                length = MaxWidthProperties - header.Length;
                totalLength = $"{new string(' ', length)}{header}".Length;
                remaining = totalLength - $"{new string(' ', length / 2)}{header}".Length;
                s += $" {new string(' ', length / 2)}{header}{new string(' ', remaining)} |";

                var index = 0;
                foreach (var q in p.Value)
                {
                    header = q.Value;
                    length = MaxWidthData[index++] - header.Length;
                    totalLength = $"{new string(' ', length)}{header}".Length;
                    remaining = totalLength - $"{new string(' ', length / 2)}{header}".Length;
                    s += $" {new string(' ', length / 2)}{header}{new string(' ', remaining)} |";
                }

                sb.AppendLine(s);
            }

            return sb.ToString();
        }


        public void ToConsole()
        {
            if (Data.Count == 0) return;

            var s = "|";

            var header = "Field";
            var length = MaxWidthProperties - header.Length;
            var totalLength = $"{new string(' ', length)}{header}".Length;
            var remaining = totalLength - $"{new string(' ', length / 2)}{header}".Length;
            s += $" {new string(' ', length / 2)}{header}{new string(' ', remaining)} |";

            foreach (var col in MaxWidthData)
            {
                header = $"T{col.Key}";
                length = MaxWidthData[col.Key] - header.Length;
                totalLength = $"{new string(' ', length)}{header}".Length;
                remaining = totalLength - $"{new string(' ', length / 2)}{header}".Length;
                s += $" {new string(' ', length / 2)}{header}{new string(' ', remaining)} |";
            }

            Console.WriteLine(s);

            s = "|";
            s += $" {new string('-', MaxWidthProperties)} |";

            foreach (var col in MaxWidthData)
            {
                s += $" {new string('-', MaxWidthData[col.Key])} |";
            }

            Console.WriteLine(s);

            foreach (var p in Data)
            {
                s = "|";
                header = p.Key;
                length = MaxWidthProperties - header.Length;
                totalLength = $"{new string(' ', length)}{header}".Length;
                remaining = totalLength - $"{new string(' ', length / 2)}{header}".Length;
                s += $" {new string(' ', length / 2)}{header}{new string(' ', remaining)} |";

                var index = 0;
                foreach (var q in p.Value)
                {
                    header = q.Value;
                    length = MaxWidthData[index++] - header.Length;
                    totalLength = $"{new string(' ', length)}{header}".Length;
                    remaining = totalLength - $"{new string(' ', length / 2)}{header}".Length;
                    s += $" {new string(' ', length / 2)}{header}{new string(' ', remaining)} |";
                }

                Console.WriteLine(s);
            }

            Console.WriteLine();
        }

        public string GetValue(T item, PropertyName property)
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
                value = properties?.GetValue(item, null);
            }

            return ObjectToString(value);
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
    }
}
