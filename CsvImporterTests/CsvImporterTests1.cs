using System;
using System.IO;
using StreamImporter.Csv;
using Xunit;
// ReSharper disable ExceptionNotDocumented

namespace CsvImporterTests
{
    public class CsvImporterTests1
    {
        [Fact]
        public void HeaderTest1()
        {
            const string data = "Test1;Test2;Test3";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.False(importer.Read());
            Assert.Equal(3, importer.ColumnMappings.Count);
        }

        [Fact]
        public void HeaderTest2()
        {
            string data = "Test1;\"Test2\";Test3";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.False(importer.Read());
            Assert.Equal(3, importer.ColumnMappings.Count);
            Assert.Equal("Test2", importer.ColumnMappings[1].NewName);
        }

        [Fact]
        public void HeaderTest3()
        {
            string data = "Test1;Test2;";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.False(importer.Read());
            Assert.Equal(2, importer.ColumnMappings.Count);
        }

        [Fact]
        public void HeaderTest4()
        {
            string data = ";Test2;Test3";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.False(importer.Read());
            Assert.Equal(3, importer.ColumnMappings.Count);
        }

        [Fact]
        public void HeaderTest5()
        {
            string data = "Test1";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.False(importer.Read());
            Assert.Equal(1, importer.ColumnMappings.Count);
        }

        [Fact]
        public void HeaderTest6()
        {
            string data = "Test1;\"Test";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Exception ex = Assert.Throws<InvalidDataException>(() => importer.Read());
            Assert.Equal("Invalid line end", ex.Message);
        }


        [Fact]
        public void HeaderTest7()
        {
            string data = "Test1;\"Test\"text";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Exception ex = Assert.Throws<InvalidDataException>(() => importer.Read());
            Assert.Equal("Wrong character at column 13, expected ';'", ex.Message);
        }

        [Fact]
        public void HeaderTest8()
        {
            string data = "Test1;;\"Test\";";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.False(importer.Read());
            Assert.Equal(3, importer.FieldCount);
        }

        [Fact]
        public void DataTestEmptyLastLine()
        {
            string data = "Test1;;\"Test\";\n\n";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.False(importer.Read());
            Assert.Equal(3, importer.FieldCount);
        }

        [Fact]
        public void DataTestOk1()
        {
            string data = "Test1;;\"Test\";\nData1;Data2;Data3\n";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.True(importer.Read());
            Assert.False(importer.Read());
        }

        [Fact]
        public void DataTestOkCheckEofOnEmptyLine()
        {
            string data = "Test1;;\"Test\";\n\nData1;Data2;Data3\n";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.False(importer.Read());
            Assert.True(importer.IsClosed);
        }


        [Fact]
        public void DataTestReadAfterEof()
        {
            string data = "Test1;;\"Test\";\n\nData1;Data2;Data3\n";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.False(importer.Read());
            Exception ex = Assert.Throws<InvalidOperationException>(() => importer.Read());
            Assert.Equal("Cannot read from closed stream", ex.Message);
        }

        [Fact]
        public void DataTestFillUp()
        {
            string data = "Test1;;\"Test\";\nData1;Data2\n";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.True(importer.Read());
            Assert.Equal(string.Empty, importer.GetValue(2));
        }

        [Fact]
        public void DataTestDataMatch()
        {
            string data = "Test1;;\"Test\";\nData1;Data2;Data3;Data4\n";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.True(importer.Read());
            Assert.Equal("Data1", importer.GetValue(0));
            Assert.Equal("Data2", importer.GetValue(1));
            Assert.Equal("Data3", importer.GetValue(2));
        }

        [Fact]
        public void DataTestDataOverflow()
        {
            string data = "Test1;;\"Test\";\nData1;Data2;Data3;Data4\n";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.True(importer.Read());
            Assert.Equal("Data1", importer.GetValue(0));
            Assert.Equal("Data2", importer.GetValue(1));
            Assert.Equal("Data3", importer.GetValue(2));
            Assert.Throws<IndexOutOfRangeException>(() => importer.GetValue(3));
        }

        [Fact]
        public void DataTestOk2()
        {
            string data = "Test1;;\"Test\";\nData1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvImporter importer = new CsvImporter(GenerateStreamFromString(data));
            importer.SetHeader(true);
            importer.SetDelimiter(';');
            Assert.True(importer.Read());
            Assert.True(importer.Read());
            Assert.True(importer.Read());
            Assert.False(importer.Read());
        }
        
        #region helper classes

        private Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        #endregion
    }
}
