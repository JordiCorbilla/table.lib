//MIT License

//Copyright (c) 2020-2024 Jordi Corbilla

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
using System.Net;

namespace table.lib
{
    public static class StringExtensions
    {
        public static string ToCsv(this string value)
        {
            if (value == null) return string.Empty;

            var enclose = value.IndexOfAny([',', '"', '\r', '\n']) >= 0;
            if (value.Contains('"')) value = value.Replace("\"", "\"\"");
            return enclose ? $"\"{value}\"" : value;
        }

        public static string ToSqlColumn(this string value)
        {
            if (value.Contains(' '))
            {
                value = $"[{value}]";
            }

            return value;
        }

        public static string ToSql(this string value)
        {
            if (value == null) return string.Empty;

            return value
                .Replace("'", "''")
                .Replace("\r\n", " ")
                .Replace('\r', ' ')
                .Replace('\n', ' ');
        }

        public static string ToValidOutput(this string value)
        {
            if (value == null) return string.Empty;

            return value
                .Replace("\r\n", " ")
                .Replace('\r', ' ')
                .Replace('\n', ' ');
        }

        public static string ToHtml(this string value)
        {
            if (value == null) return string.Empty;

            return WebUtility.HtmlEncode(value)
                .Replace("\r\n", "<br>")
                .Replace("\r", "<br>")
                .Replace("\n", "<br>");
        }
    }
}
