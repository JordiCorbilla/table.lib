using System;
using System.Collections.Generic;
using table.runner;

namespace table.lib.tests
{
    public class PivotTests
    {
        public static List<TestClass> GetSampleOutput()
        {
            var list = new List<TestClass>
            {
                new() {
                    Field1 = 321121, Field2 = "AA", Field3 = 2121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new() {
                    Field1 = 32321, Field2 = "BB", Field3 = 21111111.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                },
                new() {
                    Field1 = 321, Field2 = "CC", Field3 = 2121.32m, Field4 = true,
                    Field5 = new DateTime(1970, 1, 1), Field6 = 34.43
                }
            };
            return list;
        }

        [Test]
        public void TestPivot()
        {
            var s = Pivot<TestClass>.Add(GetSampleOutput()).ToString();
            Console.Write(s);
        }

        [Test]
        public void TestPivot2()
        {
            Pivot<TestClass>.Add(GetSampleOutput()).ToConsole();
        }
    }
}
