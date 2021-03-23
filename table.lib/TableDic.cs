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
using System.Linq;

namespace table.lib
{
    public class TableDic<TV, T> : Base<T>
    {
        public string KeyName { get; set; } = "Key_Id";

        public TableDic(Dictionary<TV, T> dictionary, string overrideDynamicName = null, string overrideKeyName = null)
        {
            if (dictionary.Count == 0) return;
            if (!string.IsNullOrEmpty(overrideDynamicName))
                DynamicName = overrideDynamicName;
            if (!string.IsNullOrEmpty(overrideKeyName))
                KeyName = overrideKeyName;
            PropertyNames = new List<PropertyName>();
            MaxWidth = new Dictionary<string, int>();
            Keys = dictionary.Select(x => x.Key).ToList();
            Items = dictionary.Select(x => x.Value).ToList();
            var properties = typeof(T).GetProperties();

            //Add the additional Key
            PropertyNames.Add(new PropertyName(KeyName));
            MaxWidth.Add(KeyName, KeyName.Length);

            foreach (var property in properties)
            {
                PropertyNames.Add(new PropertyName(property.Name));
                MaxWidth.Add(property.Name, property.Name.Length);
            }

            //Workout the key length
            foreach (var row in Keys)
            {
                var subValueLength = ObjectToString(row);

                if (subValueLength.Length > MaxWidth[KeyName])
                    MaxWidth[KeyName] = subValueLength.Length;
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
                                    var res = propertyInfo.GetValue(row, new object[] {index});
                                    if (!MaxWidth.ContainsKey($"{DynamicName}{index}"))
                                    {
                                        PropertyNames.Add(new PropertyName($"{DynamicName}{index}", index,
                                            propertyIndex));
                                        MaxWidth.Add($"{DynamicName}{index}", $"{DynamicName}{index}".Length);
                                    }

                                    if (res.ToString().Length > MaxWidth[$"{DynamicName}{index}"])
                                        MaxWidth[$"{DynamicName}{index}"] = res.ToString().Length;
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

        public List<TV> Keys { get; set; }

        public TableDic<TV, T> FilterColumns(string[] columns, FilterAction action = FilterAction.Exclude)
        {
            var filter = columns.ToDictionary(column => column, column => false);
            ColumnFilter = filter;
            ColumnAction = action;
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
                    if (property.Name == KeyName)
                    {
                        var keyValueParsed = ObjectToString(Keys[index]);

                        var lengthParsed = MaxWidth[property.Name] - keyValueParsed.Length;

                        if (ColumnTextJustification.ContainsKey(property.Name))
                            switch (ColumnTextJustification[property.Name])
                            {
                                case TextJustification.Centered:
                                    var totalLength = $"{new string(' ', lengthParsed)}{keyValueParsed.ToValidOutput()}"
                                        .Length;
                                    var remaining = totalLength -
                                                    $"{new string(' ', lengthParsed / 2)}{keyValueParsed.ToValidOutput()}"
                                                        .Length;
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
                        var value = GetValue(row, property);
                        var length = MaxWidth[property.Name] - value.Length;

                        if (ColumnTextJustification.ContainsKey(property.Name))
                            switch (ColumnTextJustification[property.Name])
                            {
                                case TextJustification.Centered:
                                    var totalLength = $"{new string(' ', length)}{value.ToValidOutput()}".Length;
                                    var remaining = totalLength -
                                                    $"{new string(' ', length / 2)}{value.ToValidOutput()}".Length;
                                    ConsoleRender(
                                        $"{new string(' ', length / 2)}{value.ToValidOutput()}{new string(' ', remaining)}",
                                        property.Name);
                                    break;
                                case TextJustification.Right:
                                    ConsoleRender($"{new string(' ', length)}{value.ToValidOutput()}", property.Name);
                                    break;
                                case TextJustification.Left:
                                    ConsoleRender($"{value.ToValidOutput()}{new string(' ', length)}", property.Name);
                                    break;
                                case TextJustification.Justified:
                                    ConsoleRender($"{value.ToValidOutput()}{new string(' ', length)}", property.Name);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        else
                            ConsoleRender($"{value.ToValidOutput()}{new string(' ', length)}", property.Name);
                    }

                Console.Write("\n");
            }

            Console.WriteLine();
        }

        public static TableDic<TV, T> Add(Dictionary<TV, T> dictionary)
        {
            return new TableDic<TV, T>(dictionary);
        }

        public static TableDic<TV, T> Add(Dictionary<TV, T> dictionary, string overrideDynamic)
        {
            return new TableDic<TV, T>(dictionary, overrideDynamic);
        }

        public static TableDic<TV, T> Add(Dictionary<TV, T> dictionary, string overrideDynamic, string overrideKey)
        {
            return new TableDic<TV, T>(dictionary, overrideDynamic, overrideKey);
        }
    }
}