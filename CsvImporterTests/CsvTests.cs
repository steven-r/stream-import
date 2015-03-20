using System;
using System.IO;
using StreamImporter.Csv;
using Xunit;

// ReSharper disable ExceptionNotDocumented

namespace ImporterTests
{
    public class CsvTests
    {
        [Fact]
        public void HeaderTest1()
        {
            const string data = "Test1;Test2;Test3";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(3, streamImporter.ColumnDefinitions.Count);
        }

        [Fact]
        public void HeaderTest2()
        {
            string data = "Test1;\"Test2\";Test3";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(3, streamImporter.ColumnDefinitions.Count);
            Assert.Equal("Test2", streamImporter.ColumnDefinitions[1].InputColumn);
        }

        [Fact]
        public void HeaderTest3()
        {
            string data = "Test1;Test2;";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(2, streamImporter.ColumnDefinitions.Count);
        }

        [Fact]
        public void HeaderTest4()
        {
            string data = ";Test2;Test3";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            Assert.Throws<ArgumentException>(() => streamImporter.SetupColumns());
        }

        [Fact]
        public void HeaderTest5()
        {
            string data = "Test1";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(1, streamImporter.ColumnDefinitions.Count);
        }

        [Fact]
        public void HeaderTest6()
        {
            string data = "Test1;\"Test";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            Exception ex = Assert.Throws<InvalidDataException>(() => streamImporter.SetupColumns());
            Assert.Equal("Invalid line end", ex.Message);
        }


        [Fact]
        public void HeaderTest7()
        {
            string data = "Test1;\"Test\"text";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            Exception ex = Assert.Throws<InvalidDataException>(() => streamImporter.SetupColumns());
            Assert.Equal("Wrong character at column 13, expected ';'", ex.Message);
        }

        [Fact]
        public void HeaderTest8()
        {
            string data = "Test1;Test2;\"Test\";";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(3, streamImporter.FieldCount);
        }

        [Fact]
        public void DataTestEmptyLastLine()
        {
            string data = "Test1;Test2;\"Test\";\n\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(3, streamImporter.FieldCount);
        }

        [Fact]
        public void DataTestOk1()
        {
            string data = "Test1;Test2;\"Test\";\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.False(streamImporter.Read());
        }

        [Fact]
        public void DataTestOkCheckEofOnEmptyLine()
        {
            string data = "Test1;Test2;\"Test\";\n\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.True(streamImporter.IsClosed);
        }


        [Fact]
        public void DataTestReadAfterEof()
        {
            string data = "Test1;Test2;\"Test\";\n\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Exception ex = Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
            Assert.Equal("Cannot read from closed stream", ex.Message);
        }

        [Fact]
        public void DataTestFillUp()
        {
            string data = "Test1;Test2;\"Test\";\nData1;Data2\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.Equal(string.Empty, streamImporter.GetValue(2));
        }

        [Fact]
        public void DataTestDataMatch()
        {
            string data = "Test1;Test2;\"Test\";\nData1;Data2;Data3;Data4\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.Equal("Data1", streamImporter.GetValue(0));
            Assert.Equal("Data2", streamImporter.GetValue(1));
            Assert.Equal("Data3", streamImporter.GetValue(2));
        }

        [Fact]
        public void DataTestDataOverflow()
        {
            string data = "Test1;Test2;\"Test\";\nData1;Data2;Data3;Data4\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.Equal("Data1", streamImporter.GetValue(0));
            Assert.Equal("Data2", streamImporter.GetValue(1));
            Assert.Equal("Data3", streamImporter.GetValue(2));
            Assert.Throws<IndexOutOfRangeException>(() => streamImporter.GetValue(3));
        }

        [Fact]
        public void DataTestOk2()
        {
            string data = "Test1;Test2;\"Test\";\nData1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.True(streamImporter.Read());
            Assert.True(streamImporter.Read());
            Assert.False(streamImporter.Read());
        }

        [Fact]
        public void DataTestMissingSetupColumns()
        {
            string data = "Test1;Test3;\"Test\";\nData1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
        }

        [Fact]
        public void DataTestMissingSetupColumnsReadAhead()
        {
            string data = "Test1;;\"Test\";\nData1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
            Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
        }
        
        [Fact]
        public void DataTestWithoutHeaderOk()
        {
            string data = "Data1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetDelimiter(';');
            streamImporter.AddColumnDefinition("Field1", typeof(string));
            streamImporter.AddColumnDefinition("Field2", typeof(string));
            streamImporter.AddColumnDefinition("Field3", typeof(string));
            Assert.True(streamImporter.Read());
            Assert.Equal("Data1", streamImporter.GetValue(0));
            Assert.Equal("Data2", streamImporter.GetValue(1));
            Assert.Equal("Data3", streamImporter.GetValue(2));
        }

        [Fact]
        public void DataTestFailWithoutHeader()
        {
            string data = "Data1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data));
            streamImporter.SetDelimiter(';');
            Exception ex = Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
            Assert.Equal("Cannot start reading if columns are not defined until now.", ex.Message);
        }

        [Fact]
        public void DataTestClosedStream()
        {
            string data = "Data1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            Stream stream = GenerateStreamFromString(data);
            CsvStreamImporter streamImporter = new CsvStreamImporter(stream);
            streamImporter.SetDelimiter(';');
            streamImporter.AddColumnDefinition("Field1", typeof(string));
            streamImporter.AddColumnDefinition("Field2", typeof(string));
            streamImporter.AddColumnDefinition("Field3", typeof(string));
            Assert.True(streamImporter.Read());
            stream.Close();
            Exception ex = Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
            Assert.Equal("Cannot read from closed stream", ex.Message);
        }


        #region helper classes

        internal static Stream GenerateStreamFromString(string s)
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
