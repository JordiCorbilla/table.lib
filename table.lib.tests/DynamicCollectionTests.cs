using System.Collections.Generic;

namespace table.lib.tests
{
    public class DynamicCollectionTests
    {
        [Test]
        public void RendersIndexedCollectionValuesWithoutExceptionDrivenTraversal()
        {
            var rows = new List<IEnumerable<string>>
            {
                new List<string> { "AAA", "BBB", "CCC" },
                new List<string> { "DDD" }
            };

            var output = Table<IEnumerable<string>>.Add(rows).ToCsv();

            Assert.That(output, Does.Contain("Dynamic0,Dynamic1,Dynamic2"));
            Assert.That(output, Does.Contain("AAA,BBB,CCC"));
            Assert.That(output, Does.Contain("DDD,,"));
        }
    }
}
