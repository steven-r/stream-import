using System;
using System.Collections.Generic;
using System.Linq;
using OfficeOpenXml;
using StreamImporter.Base;

namespace StreamImporter.Excel
{
    public class ExcelStreamImporter : StreamImporterBase
    {

        #region Fields

        protected bool HasHeader;

        private object[] _data;

        protected readonly ExcelWorksheet Sheet;

        private int _rowNum = 1;

        #endregion


        #region constructors

        public ExcelStreamImporter(ExcelWorksheet worksheet)
        {
            Sheet = worksheet;
        }

        public ExcelStreamImporter(ExcelWorksheet worksheet, bool hasHeader)
            : this(worksheet)
        {
            HasHeader = hasHeader;
        }

        #endregion

        public override object GetValue(int i)
        {
            if (_data == null)
            {
                throw new InvalidOperationException("No data available");
            }
            return _data[i];
        }

        /// <exception cref="InvalidOperationException">Cannot read from closed stream</exception>
        /// <exception cref="InvalidOperationException">Cannot start reading if columns are not defined until now.</exception>
        public override bool Read()
        {
            if (Sheet == null)
            {
                throw new InvalidOperationException("Cannot read from closed stream");
            }
            if ((ColumnDefinitions == null || !ColumnDefinitions.Any()))
            {
                throw new InvalidOperationException("Cannot start reading if columns are not defined until now.");
            }

            if (Sheet.Dimension.End.Row < _rowNum)
            {
                // EOF
                return false;
            }
            List<object> lineData = new List<object>(FieldCount);
            for (int i = 1; i <= FieldCount; i++)
            {
                lineData.Add(Sheet.GetValue(_rowNum, i));
            }
            _rowNum++;
            SetData(lineData.Take(FieldCount).ToArray());
            return true;
        }

        public virtual void SetData(object[] data)
        {
            _data = data;
        }

        /// <exception cref="InvalidOperationException">Could not determine headers</exception>
        public override void SetupColumns()
        {
            if (!HasHeader)
            {
                return;
            }
            List<string> headerFields = ReadHeaderLine();
            if (headerFields == null)
            {
                throw new InvalidOperationException("Could not determine headers");
            }

            foreach (string field in headerFields)
            {
                AddColumnDefinition<string>(field);
            }
        }

        public override int FieldCount
        {
            get { return ColumnDefinitions != null ? ColumnDefinitions.Count() : 0; }
        }

        protected List<string> ReadHeaderLine()
        {
            List<string> lineData = new List<string>(FieldCount);
            bool done = false;
            int col = 1;
            while (!done)
            {
                object header = Sheet.GetValue(_rowNum, col++);
                if (header == null || string.IsNullOrWhiteSpace(header.ToString()))
                {
                    done = true;
                }
                else
                {
                    lineData.Add(header.ToString().Trim());
                }
            }
            _rowNum++; // has been read
            return lineData;
        }


        #region Utility functions
        public void SetHeader(bool hasHeader)
        {
            HasHeader = true;
        }
        #endregion

    }
}
