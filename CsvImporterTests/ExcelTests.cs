using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using StreamImporter.Excel;
using Xunit;

namespace ImporterTests
{
    public class ExcelTests
    {

        [Fact]
        public void ExcelLoadFile()
        {
            Stream excelFile =
                Assembly.GetExecutingAssembly().GetManifestResourceStream("ImporterTests.Testdata1.xlsx");

            ExcelPackage package = new ExcelPackage();
            package.Load(excelFile);
            ExcelStreamImporter streamImporter = new ExcelStreamImporter(package.Workbook.Worksheets.First(), true);
            streamImporter.SetupColumns();
            Assert.True(streamImporter.Read());
            Assert.False(streamImporter.Read());
        }
    }
}
