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
            CsvStreamImporter streamImporter = new CsvStreamImporter(CsvTests.GenerateStreamFromString(data));
            Assert.NotNull(streamImporter.AddColumnDefinition("test1", typeof(string)));
            Assert.NotNull(streamImporter.AddColumnDefinition("test2", typeof(int)));
            Assert.NotNull(streamImporter.AddColumnDefinition("test3", typeof(decimal)));
            Assert.NotNull(streamImporter.AddColumnDefinition("test4", typeof(short)));
            Assert.Throws<InvalidOperationException>(() => streamImporter.AddColumnDefinition("Fail1", typeof (bool)));
        }
    }
}
