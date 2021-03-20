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

namespace table.lib
{
    public class HighlightOperator
    {
        public string Field { get; set; }
        public string StringValue { get; set; }
        public decimal DecimalValue { get; set; }
        public ConsoleColor ForegroundColorIfEqual { get; set; } = ConsoleColor.White;
        public ConsoleColor BackgroundColorIfEqual { get; set; } = ConsoleColor.Green;
        public ConsoleColor ForegroundColorIfDifferent { get; set; } = ConsoleColor.White;
        public ConsoleColor BackgroundColorIfDifferent { get; set; } = ConsoleColor.Red;
        public HighlightType Type { get; set; } = HighlightType.String;
    }
}
