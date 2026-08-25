using System;
using System.Collections.Generic;

namespace table.lib.tests
{
    public class TestClass
    {
        public int? Field1 { get; set; }
        public string Field2 { get; set; }
        public decimal? Field3 { get; set; }
        public bool? Field4 { get; set; }
        public DateTime? Field5 { get; set; }
        public double? Field6 { get; set; }
    }

    internal static class Samples
    {
        public static List<TestClass> GetSampleOutput()
        {
            return
            [
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
                }
            ];
        }

        public static List<TestClass> GetNullOutput()
        {
            return [new TestClass()];
        }
    }
}
