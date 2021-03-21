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
using table.lib;

namespace table.runner
{
    public static class Samples
    {
        public static List<TestClass> GetSampleOutput()
        {
            var list = new List<TestClass>
            {
                new TestClass
                {
                    Field1 = 321121, Field2 = "Hi 312321", Field3 = 2121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new TestClass
                {
                    Field1 = 32321, Field2 = "Hi long text", Field3 = 21111111.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new TestClass
                {
                    Field1 = 321, Field2 = "Hi longer text", Field3 = 2121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new TestClass
                {
                    Field1 = 13, Field2 = "Hi very long text", Field3 = 21111121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new TestClass
                {
                    Field1 = 13, Field2 = "Hi very, long text", Field3 = 21111121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new TestClass
                {
                    Field1 = 13, Field2 = "Hi \"very\" long\n text", Field3 = 21111121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                }
            };
            return list;
        }

        public static void SimpleConsoleOutputForList()
        {
            Table<TestClass>.Add(GetSampleOutput()).ToConsole();
        }

        public static void SimpleConsoleOutputWithHighlighterForList()
        {
            Table<TestClass>.Add(GetSampleOutput())
                .HighlightValue(new HighlightOperator
                    {Field = "Field3", Type = HighlightType.Decimal, DecimalValue = 2121.32m})
                .ToConsole();
        }

        public static void SimpleCsvOutputForList()
        {
            Table<TestClass>.Add(GetSampleOutput()).ToCsv(@"C:\temp\test-list.csv");
        }

        public static void SimpleHtmlOutputForList()
        {
            Table<TestClass>.Add(GetSampleOutput()).ToHtml(@"C:\temp\test-list.html");
        }
    }
}