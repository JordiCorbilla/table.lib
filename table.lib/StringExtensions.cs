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

namespace table.lib
{
    public static class StringExtensions
    {
        public static string ToCsv(this string value)
        {
            var enclose = false;
            if (value.Contains('"'))
            {
                value = value.Replace("\"", "\"\"");
                enclose = true;
            }

            if (value.Contains(Environment.NewLine) || value.Contains(',')) enclose = true;

            return enclose ? $"\"{value}\"" : value;
        }

        public static string ToSql(this string value)
        {
            if (value.Contains('\''))
            {
                value = value.Replace("\'", "\"");
            }

            if (value.Contains(Environment.NewLine))
            {
                value = value.Replace(Environment.NewLine, " ");
            }
                
            if (value.Contains(','))
            {
                value = value.Replace(",", "-");
            }

            return value;
        }

        public static string ToValidOutput(this string value)
        {
            if (value.Contains(Environment.NewLine)) value = value.Replace(Environment.NewLine, " ");

            if (value.Contains('\n')) value = value.Replace("\n", " ");

            return value;
        }

        public static string ToHtml(this string value)
        {
            if (value.Contains(Environment.NewLine)) value = value.Replace(Environment.NewLine, "<br>");

            if (value.Contains('\n')) value = value.Replace("\n", "<br>");

            return value;
        }
    }
}