using System;
using table.runner;

namespace table.lib.tests
{
    public class SqlInsertTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestGeneration()
        {
            var s = Table<TestClass>.Add(Samples.GetSampleOutput()).ToSqlInsertString();
            var lines = s.Split(Environment.NewLine);
            Assert.That(lines[0], Is.EqualTo("INSERT INTO Table1 (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (321121,'Hi 312321',2121.32,1,'1970-01-01',34.43);"));
        }
    }
}