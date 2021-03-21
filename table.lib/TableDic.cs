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
        public TableDic(Dictionary<TV, T> dictionary, string overrideDynamicName = null)
        {
            if (dictionary.Count == 0) return;
            if (!string.IsNullOrEmpty(overrideDynamicName))
                DynamicName = overrideDynamicName;
            PropertyNames = new List<PropertyName>();
            MaxWidth = new Dictionary<string, int>();
            Items = dictionary.Select(x => x.Value).ToList();
            var properties = typeof(T).GetProperties();
            foreach (var property in properties)
            {
                PropertyNames.Add(new PropertyName(property.Name));
                MaxWidth.Add(property.Name, property.Name.Length);
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

        public static TableDic<T, TV> Add(Dictionary<T, TV> list)
        {
            return null;
        }
    }
}