using System;
using System.Collections.Generic;
using System.IO;
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
            var properties = TypeMetadata<T>.Properties;
            ClassName = typeof(T).Name;
            foreach (var property in properties)
            {
                PropertyNames.Add(new PropertyName(property.Name) { Accessor = property });
                if (property.Name.Length > MaxWidthProperties)
                    MaxWidthProperties = property.Name.Length;
                Data.Add(property.Name, []);
            }

            var index = 0;
            foreach (var row in Items)
            {
                if (properties.Count != 0)
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
            return Data.Count == 0 ? string.Empty : RenderingHelpers.RenderToString(WriteTable);
        }


        public void ToConsole()
        {
            if (Data.Count == 0) return;
            WriteTable(Console.Out);
            Console.WriteLine();
        }

        private void WriteTable(TextWriter writer)
        {
            writer.Write('|');
            RenderingHelpers.WriteCenteredCell(writer, "Field", MaxWidthProperties);
            foreach (var column in MaxWidthData)
                RenderingHelpers.WriteCenteredCell(writer, $"T{column.Key}", column.Value);
            writer.WriteLine();

            writer.Write('|');
            RenderingHelpers.WriteSeparatorCell(writer, MaxWidthProperties);
            foreach (var column in MaxWidthData)
                RenderingHelpers.WriteSeparatorCell(writer, column.Value);
            writer.WriteLine();

            foreach (var (name, values) in Data)
            {
                writer.Write('|');
                RenderingHelpers.WriteCenteredCell(writer, name, MaxWidthProperties);
                for (var index = 0; index < values.Count; index++)
                    RenderingHelpers.WriteCenteredCell(writer, values[index].Value, MaxWidthData[index]);
                writer.WriteLine();
            }
        }

        public string GetValue(T item, PropertyName property)
        {
            if (string.IsNullOrEmpty(property.Name)) return null;
            object value;
            if (property.Accessor is PropertyAccessor<T> directAccessor)
                value = directAccessor.GetValue(item);
            else if (TypeMetadata<T>.PropertiesByName.TryGetValue(property.Name, out var accessor))
                value = accessor.GetValue(item);
            else
            {
                var runtimeProperty = Array.Find(TypeMetadata<T>.GetRuntimeProperties(item.GetType()),
                    candidate => candidate.Name == property.Name && candidate.GetIndexParameters().Length == 0);
                value = runtimeProperty?.GetValue(item, null);
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
