using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using StreamImporter.Base;

namespace StreamImporter.Csv
{
    public class CsvStreamImporter: StreamImporterBase
    {

        #region Fields

        protected bool HasHeader;

        /// <summary>
        /// The stream to be read
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly Stream Stream;

        /// <summary>
        /// The used stream reader
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        protected readonly StreamReader StreamReader;

        private string[] _data;


        #endregion


        #region constructors 

        public CsvStreamImporter(Stream stream)
        {
            Stream = stream;
            StreamReader = new StreamReader(Stream);
        }

        public CsvStreamImporter(Stream stream, bool hasHeader)
        : this(stream)
        {
            HasHeader = hasHeader;
        }

        public CsvStreamImporter(Stream stream, CultureInfo cultureInfo)
        : this(stream)
        {
            CultureInfo = cultureInfo;
        }

        public CsvStreamImporter(Stream stream, bool hasHeader, CultureInfo cultureInfo)
        : this(stream, hasHeader)
        {
            CultureInfo = cultureInfo;
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
            Stream.Close();
        }

        public override bool IsClosed
        {
            get { return !Stream.CanRead; }
        }

        /// <exception cref="InvalidOperationException">Cannot read from closed stream</exception>
        /// <exception cref="InvalidOperationException">Cannot start reading if columns are not defined until now.</exception>
        public override bool Read()
        {
            if (Stream == null || Stream.CanRead == false)
            {
                throw new InvalidOperationException("Cannot read from closed stream");
            }
            if ((ColumnDefinitions == null || !ColumnDefinitions.Any()))
            {
                throw new InvalidOperationException("Cannot start reading if columns are not defined until now.");
            }
            string line = StreamReader.ReadLine();
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

            SetData(lineData.Take(FieldCount).ToArray());
            return true;
        }

        public virtual void SetData(string[] data)
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
            string line = StreamReader.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
            {
                throw new InvalidOperationException("Cannot read header line");
            }
            var headerLine = ParseLine(line);
            for (int i = 0; i < headerLine.Count(); i++)
            {
                headerLine[i] = headerLine[i].Trim();
            }
            return headerLine;
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

        /// <summary>
        /// Parse a CSV line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        // ReSharper disable once MemberCanBePrivate.Global
        protected List<string> ParseLine(string line)
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
                        else if (data[i] == CultureInfo.TextInfo.ListSeparator[0])
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
                        if (data[i] == CultureInfo.TextInfo.ListSeparator[0])
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
                        if (data[i] != CultureInfo.TextInfo.ListSeparator[0])
                        {
                            throw new InvalidDataException(string.Format(
                                "Wrong character at column {0}, expected '{1}'", i + 1, CultureInfo.TextInfo.ListSeparator));
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

        #region Utility functions
        public void SetHeader(bool hasHeader)
        {
            HasHeader = true;
        }

        #endregion

    }
}
