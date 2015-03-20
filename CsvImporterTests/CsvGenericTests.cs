using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using StreamImporter.Base;
using StreamImporter.Csv;
using Xunit;

namespace ImporterTests
{
    public class CsvGenericTests
    {
        private class TestClass
        {
            public string Field1 { get; set; }
            public int Field2 { get; set; }
            public decimal Field3 { get; set; }
        }

        [Fact]
        public void Test1()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string data = "Field1;Field2;Field3;\nData1;12;12.5\n";
            CsvStreamImporter<TestClass> streamImporter = new CsvStreamImporter<TestClass>(CsvTests.GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.Equal("Data1", streamImporter.Value.Field1);
            Assert.Equal(12, streamImporter.Value.Field2);
            Assert.Equal(12.5M, streamImporter.Value.Field3);
            Assert.False(streamImporter.Read());
        }


        private class TestClassFail
        {
            public string Field1 { get; set; }
            public int Field2 { get; set; }
            public Boolean Field3 { get; set; }
        }

        [Fact]
        public void TestFailInvalidType()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string data = "Field1;Field2;Field3;\nData1;12;true\n";
            CsvStreamImporter<TestClassFail> streamImporter = new CsvStreamImporter<TestClassFail>(CsvTests.GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            Assert.Throws<InvalidOperationException>(() => streamImporter.SetupColumns());
        }


        private class MapTest
        {
            public string A { get; set; }
            public int B { get; set; }
            public decimal C { get; set; }
            
        }

        [Fact]
        public void TestMapping()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string data = "Field1;Field2;Field3;\nData1;12;12.5\n";
            CsvStreamImporter<MapTest> streamImporter = new CsvStreamImporter<MapTest>(CsvTests.GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.MapField = header =>
            {
                switch (header)
                {
                    case "Field1":
                        return "A";
                    case "Field2":
                        return "B";
                    case "Field3":
                        return "C";
                }
                return null; // field not found
            };
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.Equal("Data1", streamImporter.Value.A);
            Assert.Equal(12, streamImporter.Value.B);
            Assert.Equal(12.5M, streamImporter.Value.C);
            Assert.False(streamImporter.Read());
        }

        #region special decimal handler class

        [Fact]
        public void TestDefaultCulture()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string data = "Field1;Field2;Field3;\nData1;12;12.5\n";
            CsvStreamImporter<TestClass> streamImporter = new CsvStreamImporter<TestClass>(CsvTests.GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetColumnTypeDefinition<decimal>(typeof (MyDecimalColumnDefinition));
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.Equal("Data1", streamImporter.Value.Field1);
            Assert.Equal(12, streamImporter.Value.Field2);
            Assert.Equal(12.5M, streamImporter.Value.Field3);
            Assert.False(streamImporter.Read());
        }

        [Fact]
        public void TestDefaultDeDe()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
            string data = "Field1;Field2;Field3;\nData1;12;12.5\n";
            CsvStreamImporter<TestClass> streamImporter = new CsvStreamImporter<TestClass>(CsvTests.GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetColumnTypeDefinition<decimal>(typeof(MyDecimalColumnDefinition));
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.Equal("Data1", streamImporter.Value.Field1);
            Assert.Equal(12, streamImporter.Value.Field2);
            Assert.Equal(12.5M, streamImporter.Value.Field3);
            Assert.False(streamImporter.Read());
        }

        [Fact]
        public void TestDefaultDeCh()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-CH");
            string data = "Field1;Field2;Field3;\nData1;12;12.5\n";
            CsvStreamImporter<TestClass> streamImporter = new CsvStreamImporter<TestClass>(CsvTests.GenerateStreamFromString(data));
            streamImporter.SetHeader(true);
            streamImporter.SetDelimiter(';');
            streamImporter.SetColumnTypeDefinition<decimal>(typeof(MyDecimalColumnDefinition));
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.Equal("Data1", streamImporter.Value.Field1);
            Assert.Equal(12, streamImporter.Value.Field2);
            Assert.Equal(12.5M, streamImporter.Value.Field3);
            Assert.False(streamImporter.Read());
        }

        /// <summary>
        /// Class MyDecimalColumnDefinition.
        /// </summary>
        /// <remarks>
        /// https://sreindl.wordpress.com/2013/10/26/i18n-parsing-decimal-values/
        /// </remarks>
        private class MyDecimalColumnDefinition : ColumnDefinitionBase<decimal>
        {
            public override decimal Convert(string value)
            {
                return ToDecimal(value);
            }

            private static decimal ToDecimal(string value)
            {
                Contract.Requires(value != null);
                string tempValue = RemoveSwissChars(value);
                NumberFormatInfo format = CultureInfo.InvariantCulture.NumberFormat;

                var punctuation = tempValue.Where(x => x == '.' || x == ',').Distinct().ToList();
                int count = punctuation.Count();

                switch (count)
                {
                    case 0:
                        break;
                    case 1: // only comma or dot
                        if (tempValue.Count(x => x == '.' || x == ',') == 1)
                        {
                            var pos = tempValue.LastIndexOfAny(new[] { ',', '.' });
                            if (pos == tempValue.Length - 4)
                            {
                                // 100,000 can be 100 in german and 100000 in US
                                // format according to current culture settings
                                format = CultureInfo.CurrentCulture.NumberFormat;
                                break;
                            }
                        }
                        if (tempValue.IndexOf(',') != -1)
                        {
                            tempValue = value.Replace(",", ".");
                        }
                        break;
                    case 2: // 123.456,12 --> 123,456.12
                        if (punctuation.ElementAt(0) == '.')
                        {
                            tempValue = SwapChar(value, '.', ',');
                        }
                        break;
                }

                decimal number = Decimal.Parse(tempValue, format);
                return number;
            }

            private static string SwapChar(string value, char from, char to)
            {
                Contract.Requires(value != null);
                Contract.Ensures(Contract.Result<string>() != null);

                StringBuilder builder = new StringBuilder(value.Length);

                foreach (var item in value)
                {
                    char c = item;
                    if (c == @from)
                        c = to;
                    else if (c == to)
                        c = @from;

                    builder.Append(c);
                }
                return builder.ToString();
            }

            private static string RemoveSwissChars(string value)
            {
                Contract.Requires(value != null);
                Contract.Ensures(Contract.Result<string>() != null);
                int pos;
                while ((pos = value.IndexOf('\'')) >= 0)
                {
                    value = value.Remove(pos);
                }
                return value;
            }

        #endregion
        }
    }
}
