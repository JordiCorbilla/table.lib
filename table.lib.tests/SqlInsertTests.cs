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
            Assert.Multiple(() =>
            {
                Assert.That(lines[0], Is.EqualTo("INSERT INTO TestClass (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (321121,'Hi 312321',2121.32,1,'1970-01-01',34.43);"));
                Assert.That(lines[1], Is.EqualTo("INSERT INTO TestClass (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (32321,'Hi long text',21111111.32,1,'1970-01-01',34.43);"));
                Assert.That(lines[2], Is.EqualTo("INSERT INTO TestClass (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (321,'Hi longer text',2121.32,1,'1970-01-01',34.43);"));
                Assert.That(lines[3], Is.EqualTo("INSERT INTO TestClass (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (13,'Hi very long text',21111121.32,1,'1970-01-01',34.43);"));
            });
        }

        [Test]
        public void TestNullGeneration()
        {
            var s = Table<TestClass>.Add(Samples.GetNullOutput()).ToSqlInsertString();
            var lines = s.Split(Environment.NewLine);
            Assert.Multiple(() =>
            {
                Assert.That(lines[0], Is.EqualTo("INSERT INTO TestClass (Field1,Field2,Field3,Field4,Field5,Field6) VALUES (321121,NULL,2121.32,1,'1970-01-01',34.43);"));
            });
        }
    }
}