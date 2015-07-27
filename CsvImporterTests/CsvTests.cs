using System;
using System.Globalization;
using System.IO;
using StreamImporter.Base.ColumnDefinitions;
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
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            const string data = "Test1;Test2;Test3";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(3, streamImporter.ColumnDefinitions.Count);
        }

        [Fact]
        public void HeaderTest2()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;\"Test2\";Test3";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(3, streamImporter.ColumnDefinitions.Count);
            Assert.Equal("Test2", streamImporter.ColumnDefinitions[1].InputColumn);
        }

        [Fact]
        public void HeaderTest3()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test2;";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(2, streamImporter.ColumnDefinitions.Count);
        }

        [Fact]
        public void HeaderTest4()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = ";Test2;Test3";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            Assert.Throws<ArgumentException>(() => streamImporter.SetupColumns());
        }

        [Fact]
        public void HeaderTest5()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(1, streamImporter.ColumnDefinitions.Count);
        }

        [Fact]
        public void HeaderTest6()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;\"Test";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            Exception ex = Assert.Throws<InvalidDataException>(() => streamImporter.SetupColumns());
            Assert.Equal("Invalid line end", ex.Message);
        }


        [Fact]
        public void HeaderTest7()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;\"Test\"text";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            Exception ex = Assert.Throws<InvalidDataException>(() => streamImporter.SetupColumns());
            Assert.Equal("Wrong character at column 13, expected ';'", ex.Message);
        }

        [Fact]
        public void HeaderTest8()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test2;\"Test\";";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(3, streamImporter.FieldCount);
        }

        [Fact]
        public void DataTestEmptyLastLine()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test2;\"Test\";\n\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.Equal(3, streamImporter.FieldCount);
        }

        [Fact]
        public void DataTestOk1()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test2;\"Test\";\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.False(streamImporter.Read());
        }

        [Fact]
        public void DataTestOkCheckEofOnEmptyLine()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test2;\"Test\";\n\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Assert.True(streamImporter.IsClosed);
        }


        [Fact]
        public void DataTestReadAfterEof()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test2;\"Test\";\n\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.False(streamImporter.Read());
            Exception ex = Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
            Assert.Equal("Cannot read from closed stream", ex.Message);
        }

        [Fact]
        public void DataTestFillUp()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test2;\"Test\";\nData1;Data2\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.Equal(string.Empty, streamImporter.GetValue(2));
        }

        [Fact]
        public void DataTestDataMatch()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test2;\"Test\";\nData1;Data2;Data3;Data4\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.Equal("Data1", streamImporter.GetValue(0));
            Assert.Equal("Data2", streamImporter.GetValue(1));
            Assert.Equal("Data3", streamImporter.GetValue(2));
        }

        [Fact]
        public void DataTestDataOverflow()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test2;\"Test\";\nData1;Data2;Data3;Data4\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
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
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test2;\"Test\";\nData1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.True(streamImporter.Read());
            Assert.True(streamImporter.Read());
            Assert.False(streamImporter.Read());
        }


        [Fact]
        public void DataTestMissingSetupColumns()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test3;\"Test\";\nData1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
        }

        [Fact]
        public void DataTestDecimal()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test3;\"Test\";\nData1;Data2;1.0\nData1;Data2;2.0\nData1;Data2\nData1;Data2;2.43\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            streamImporter.SetDataType("Test", typeof (DecimalColumnDefinition));
            Assert.True(streamImporter.Read());
            Assert.Equal(1M, streamImporter.GetValue(2));
            Assert.True(streamImporter.Read());
            Assert.Equal(2M, streamImporter.GetValue(2));
            Assert.True(streamImporter.Read());
            Assert.Equal(0M, streamImporter.GetValue(2));
            Assert.True(streamImporter.Read());
            Assert.Equal(2.43M, streamImporter.GetValue(2));
        }

        [Fact]
        public void DecimalDeCh()
        {
            CultureInfo info = new CultureInfo("de-CH");
            Assert.Equal("de-CH", info.Name);
            info.TextInfo.ListSeparator = ";";
#if FIX_APVOYER
            info.NumberFormat.NumberDecimalSeparator = ".";
            info.NumberFormat.NumberGroupSeparator = "'";
#endif
            string data = "Test1;Test3;\"Test\";\nData1;Data2;1.0\nData1;Data2;2.0\nData1;Data2\nData1;Data2;2.43\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            streamImporter.SetDataType("Test", typeof(DecimalColumnDefinition));
            Assert.True(streamImporter.Read());
            Assert.Equal(1M, streamImporter.GetValue(2));
            Assert.True(streamImporter.Read());
            Assert.Equal(2M, streamImporter.GetValue(2));
            Assert.True(streamImporter.Read());
            Assert.Equal(0M, streamImporter.GetValue(2));
            Assert.True(streamImporter.Read());
            Assert.Equal(2.43M, streamImporter.GetValue(2));
        }


        [Fact]
        public void DecimalDeDe()
        {
            CultureInfo info = new CultureInfo("de-DE");
            Assert.Equal("de-DE", info.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;Test3;\"Test\";\nData1;Data2;1,0\nData1;Data2;2,0\nData1;Data2\nData1;Data2;2,43\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            streamImporter.SetupColumns();
            streamImporter.SetDataType("Test", typeof(DecimalColumnDefinition));
            Assert.True(streamImporter.Read());
            Assert.Equal(1M, streamImporter.GetValue(2));
            Assert.True(streamImporter.Read());
            Assert.Equal(2M, streamImporter.GetValue(2));
            Assert.True(streamImporter.Read());
            Assert.Equal(0M, streamImporter.GetValue(2));
            Assert.True(streamImporter.Read());
            Assert.Equal(2.43M, streamImporter.GetValue(2));
        }

        [Fact]
        public void DataTestMissingSetupColumnsReadAhead()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Test1;;\"Test\";\nData1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            streamImporter.SetHeader(true);
            Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
            Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
        }
        
        [Fact]
        public void DataTestWithoutHeaderOk()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Data1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
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
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Data1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            CsvStreamImporter streamImporter = new CsvStreamImporter(GenerateStreamFromString(data), info);
            Exception ex = Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
            Assert.Equal("Cannot start reading if columns are not defined until now.", ex.Message);
        }

        [Fact]
        public void DataTestClosedStream()
        {
            CultureInfo info = new CultureInfo(CultureInfo.InvariantCulture.Name);
            info.TextInfo.ListSeparator = ";";

            string data = "Data1;Data2;Data3\nData1;Data2;Data3\nData1;Data2;Data3\n";
            StreamReader stream = GenerateStreamFromString(data);
            CsvStreamImporter streamImporter = new CsvStreamImporter(stream, info);
            streamImporter.AddColumnDefinition("Field1", typeof(string));
            streamImporter.AddColumnDefinition("Field2", typeof(string));
            streamImporter.AddColumnDefinition("Field3", typeof(string));
            Assert.True(streamImporter.Read());
            stream.Close();
            Exception ex = Assert.Throws<InvalidOperationException>(() => streamImporter.Read());
            Assert.Equal("Cannot read from closed stream", ex.Message);
        }


        #region helper classes

        internal static StreamReader GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return new StreamReader(stream);
        }

        #endregion
    }
}
