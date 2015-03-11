using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamImporter.Csv;
using Xunit;

namespace ImporterTests
{
    public class BaseTests
    {
        [Fact]
        public void AddColumnDefinitionTest()
        {
            const string data = "Test1;Test2;Test3";
            CsvImporter importer = new CsvImporter(CsvTests.GenerateStreamFromString(data));
            Assert.NotNull(importer.AddColumnDefinition("test1", typeof(string)));
            Assert.NotNull(importer.AddColumnDefinition("test2", typeof(int)));
            Assert.NotNull(importer.AddColumnDefinition("test3", typeof(decimal)));
            Assert.NotNull(importer.AddColumnDefinition("test4", typeof(short)));
            Assert.Throws<InvalidOperationException>(() => importer.AddColumnDefinition("Fail1", typeof (bool)));
        }
    }
}
