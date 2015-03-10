using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using StreamImporter.Base;

namespace StreamImporter.Csv
{
    public class CsvImporter: ImporterBase
    {

        #region Fields

        private char _delimiter = ',';

        private bool _hasHeader;

        private readonly Stream _stream;

        private readonly StreamReader _streamReader;

        private string[] _data;

        #endregion


        #region constructors 

        public CsvImporter(Stream stream)
        {
            _stream = stream;
            _streamReader = new StreamReader(_stream);
        }

        public CsvImporter(Stream stream, bool hasHeader)
        : this(stream)
        {
            _hasHeader = hasHeader;
        }

        public CsvImporter(Stream stream, bool hasHeader, char delimiter)
            : this(stream, hasHeader)
        {
            _delimiter = delimiter;
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

        public override void Close()
        {
            _stream.Close();
        }

        public override bool IsClosed
        {
            get { return !_stream.CanRead; }
        }

        /// <exception cref="InvalidOperationException">Cannot start reading if columns are not defined until now.</exception>
        public override bool Read()
        {
            _data = null;
            if (_stream == null || _stream.CanRead == false)
            {
                throw new InvalidOperationException("Cannot read from closed stream");
            }
            if ((ColumnDefinitions == null || !ColumnDefinitions.Any()) && !_hasHeader)
            {
                throw new InvalidOperationException("Cannot start reading if columns are not defined until now.");
            }
            if ((ColumnDefinitions == null || !ColumnDefinitions.Any()) && _hasHeader)
            {
                ReadHeaderLine();
            }
            string line = _streamReader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                Close();
                return false;
            }
            List<string> lineData = ParseLine(line);
            for (int n = lineData.Count(); n < FieldCount; n++)
            {
                lineData.Add(string.Empty);
            }
            _data = lineData.Take(FieldCount).ToArray();
            return true;
        }

        private void ReadHeaderLine()
        {
            string line = _streamReader.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
            {
                throw new InvalidOperationException("Cannot read header line");
            }

            IEnumerable<string> fields = ParseLine(line);
            foreach (string field in fields)
            {
                AddColumnDefinition(field, DbType.String);
            }
        }

        #region Line parser
        [Flags]
        enum ParseState
        {
            StartOfElement = 1,
            InElement = 2,
            StartOfString = 8,
            Delimiter = 16,
            InString = 32,
            FinalValid = StartOfElement | Delimiter
        }
        private List<string> ParseLine(string line)
        {
            List<string> output = new List<string>(10);
            ParseState state = ParseState.StartOfElement; // in text
            char[] data = line.ToCharArray();
            StringBuilder currentField = new StringBuilder(40); // have enough space in the beginning
            for (int i = 0; i < data.Length; i++)
            {
                switch (state)
                {
                    case ParseState.StartOfElement: // regular start
                        if (data[i] == '"')
                        {
                            state = ParseState.InString; // start of string
                        }
                        else if (data[i] == _delimiter)
                        {
                            output.Add(currentField.ToString());
                            currentField.Clear();
                            // finish field
                            state = ParseState.StartOfElement; // delimiter
                        }
                        else
                        {
                            currentField.Append(data[i]);
                            state = ParseState.InElement;
                        }
                        break;
                    case ParseState.InElement:
                        if (data[i] == _delimiter)
                        {
                            output.Add(currentField.ToString());
                            currentField.Clear();
                            // finish field
                            state = ParseState.StartOfElement; // delimiter
                        }
                        else
                        {
                            currentField.Append(data[i]);
                        }
                        break;
                    case ParseState.StartOfString: // in string
                        state = ParseState.InString;
                        break;
                    case ParseState.InString:
                        if (data[i] == '"')
                        {
                            if (i < data.Length - 1)
                            {
                                if (data[i + 1] == '"')
                                {
                                    i++; // replace with single quote
                                    currentField.Append(data[i]);
                                    break;
                                }
                            }
                            state = ParseState.Delimiter;
                            break;
                        }
                        currentField.Append(data[i]);
                        break;
                    case ParseState.Delimiter:
                        if (data[i] != _delimiter)
                        {
                            throw new InvalidDataException(string.Format(
                                "Wrong character at column {0}, expected '{1}'", i + 1, _delimiter));
                        }
                        output.Add(currentField.ToString());
                        currentField.Clear();
                        state = ParseState.StartOfElement;
                        break;
                }
            }
            if (state == ParseState.InElement)
            {
                output.Add(currentField.ToString());
                state = ParseState.StartOfElement;
            }
            if ((state & ParseState.FinalValid) == 0)
            {
                throw new InvalidDataException("Invalid line end");
            }
            return output;
        }

        #endregion

        protected override void AddSchemaTableRows()
        {
            foreach (ColumnDefinition column in ColumnDefinitions)
            {
                AddColumnMapping(column.Name, column.Name);
            }
        }

        public override int FieldCount
        {
            get { return ColumnDefinitions != null ? ColumnDefinitions.Count() : 0; }
        }

        public override Type GetFieldType(int i)
        {
            return typeof(string);
        }


        #region Utility functions
        public void SetHeader(bool hasHeader)
        {
            _hasHeader = true;
        }

        public void SetDelimiter(char delimiter)
        {
            _delimiter = delimiter;
        }
        #endregion

    }
}
