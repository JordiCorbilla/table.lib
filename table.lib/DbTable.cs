//MIT License

//Copyright (c) 2020-2024 Jordi Corbilla

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace table.lib
{
    public class DbTable
    {
        private List<IDictionary<string, object>> _items = [];

        public IEnumerable<IDictionary<string, object>> Items
        {
            get => _items;
            set
            {
                ArgumentNullException.ThrowIfNull(value);
                _items = value as List<IDictionary<string, object>> ?? value.ToList();
                RebuildMetadata();
            }
        }

        public List<PropertyName> PropertyNames { get; set; } = [];
        public Dictionary<string, int> MaxWidth { get; set; } = [];
        public Options Options { get; set; } = new Options();
        public Dictionary<string, bool> ColumnFilter { get; set; } = [];
        public FilterAction ColumnAction { get; set; } = FilterAction.Exclude;

        public DbTable(IEnumerable<IDictionary<string, object>> list, Options options = null)
        {
            ArgumentNullException.ThrowIfNull(list);
            if (options != null) Options = options;
            Items = list;
        }

        public static DbTable Add(IEnumerable<IDictionary<string, object>> list, Options options = null)
        {
            return new DbTable(list, options);
        }

        public DbTable FilterColumns(string[] columns, FilterAction action = FilterAction.Exclude)
        {
            ArgumentNullException.ThrowIfNull(columns);
            ColumnFilter = columns.ToDictionary(column => column, _ => false, StringComparer.Ordinal);
            ColumnAction = action;
            return this;
        }

        public string ObjectToString(object value)
        {
            return value switch
            {
                string text => text,
                DateTime time => time.ToString(Options.DateFormat),
                decimal number => number.ToString(Options.DecimalFormat),
                double number => number.ToString(Options.DecimalFormat),
                _ => value?.ToString() ?? string.Empty
            };
        }

        public List<PropertyName> FilterProperties()
        {
            if (ColumnAction is not FilterAction.Include and not FilterAction.Exclude)
                throw new ArgumentOutOfRangeException(nameof(ColumnAction), ColumnAction, "Unsupported filter action.");

            var properties = new List<PropertyName>(PropertyNames.Count);
            foreach (var property in PropertyNames)
            {
                var selected = ColumnFilter.ContainsKey(property.Name);
                if ((ColumnAction == FilterAction.Include && selected) ||
                    (ColumnAction == FilterAction.Exclude && !selected))
                    properties.Add(property);
            }
            return properties;
        }

        public override string ToString()
        {
            return _items.Count == 0 ? string.Empty : RenderingHelpers.RenderToString(WriteTable);
        }

        public string ToSpecFlowString()
        {
            return _items.Count == 0 ? string.Empty : RenderingHelpers.RenderToString(WriteSpecFlow);
        }

        public void WriteSpecFlow(TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            if (_items.Count == 0) return;

            var properties = FilterProperties();
            WriteHeader(writer, properties);
            WriteRows(writer, properties);
            writer.WriteLine();
        }

        public string ToSqlInsertString()
        {
            return _items.Count == 0 ? string.Empty : RenderingHelpers.RenderToString(WriteSqlInsert);
        }

        public void WriteSqlInsert(TextWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            if (_items.Count == 0) return;

            var properties = FilterProperties();
            var prefix = new StringBuilder("INSERT INTO Table1 (");
            for (var index = 0; index < properties.Count; index++)
            {
                if (index > 0) prefix.Append(',');
                prefix.Append(properties[index].Name.ToSqlColumn());
            }
            prefix.Append(") VALUES (");

            foreach (var row in _items)
            {
                writer.Write(prefix);
                for (var index = 0; index < properties.Count; index++)
                {
                    if (index > 0) writer.Write(',');
                    row.TryGetValue(properties[index].Name, out var value);
                    writer.Write(RenderingHelpers.ToSqlLiteral(value));
                }
                writer.WriteLine(");");
            }
        }

        private void RebuildMetadata()
        {
            PropertyNames = [];
            MaxWidth = [];

            foreach (var row in _items)
            {
                foreach (var (key, value) in row)
                {
                    if (!MaxWidth.TryGetValue(key, out var width))
                    {
                        PropertyNames.Add(new PropertyName(key));
                        width = key.Length;
                        MaxWidth.Add(key, width);
                    }

                    var text = ObjectToString(value);
                    if (text.Length > width) MaxWidth[key] = text.Length;
                }
            }
        }

        private void WriteTable(TextWriter writer)
        {
            var properties = FilterProperties();
            WriteHeader(writer, properties);

            writer.Write('|');
            foreach (var property in properties)
                RenderingHelpers.WriteSeparatorCell(writer, MaxWidth[property.Name]);
            writer.WriteLine();

            WriteRows(writer, properties);
            writer.WriteLine();
        }

        private void WriteHeader(TextWriter writer, IReadOnlyList<PropertyName> properties)
        {
            writer.Write('|');
            foreach (var property in properties)
                RenderingHelpers.WriteCenteredCell(writer, property.Name.ToValidOutput(), MaxWidth[property.Name]);
            writer.WriteLine();
        }

        private void WriteRows(TextWriter writer, IReadOnlyList<PropertyName> properties)
        {
            foreach (var row in _items)
            {
                writer.Write('|');
                foreach (var property in properties)
                {
                    row.TryGetValue(property.Name, out var value);
                    RenderingHelpers.WriteValueCell(writer, ObjectToString(value).ToValidOutput(), MaxWidth[property.Name]);
                }
                writer.WriteLine();
            }
        }
    }
}
