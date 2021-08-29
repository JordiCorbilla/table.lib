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

        public DbTable(IEnumerable<IDictionary<string, object>> list)
        {
            PropertyNames = new List<PropertyName>();
            MaxWidth = new Dictionary<string, int>();
            Items = list;
            var addedProperties = false;
            foreach (var row in Items)
            {
                var columnIndex = 0;
                foreach (var (key, value) in row)
                    if (!addedProperties)
                    {
                        PropertyNames.Add(new PropertyName(key, columnIndex++, columnIndex));
                        MaxWidth.Add(key, key.Length);
                    }
                    else
                    {
                        if (value != null && value.ToString().Length > MaxWidth[key])
                            MaxWidth[key] = value.ToString().Length;
                    }

                addedProperties = true;
            }
        }

        public static DbTable Add(IEnumerable<IDictionary<string, object>> list)
        {
            return new DbTable(list);
        }

        public override string ToString()
        {
            if (!Items.Any()) return string.Empty;
            var s = "|";
            var sb = new StringBuilder();

            var filteredPropertyNames = PropertyNames;

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
                    var length = MaxWidth[key] - text.ToString().Length;
                    var output = text.ToString().ToValidOutput();
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

            var filteredPropertyNames = PropertyNames;

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
                    var length = MaxWidth[key] - text.ToString().Length;
                    var output = text.ToString().ToValidOutput();
                    sb.Append(' ');
                    sb.Append($"{output}{new string(' ', length)}");
                    sb.Append(" |");
                }

                sb.Append(Environment.NewLine);
            }

            sb.AppendLine("");
            return sb.ToString();
        }
    }
}