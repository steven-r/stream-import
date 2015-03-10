using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace StreamImporter.Base
{
    /// <summary>
    /// An implementation of <see cref="IDataReader"/> that can be used to input streamed data from fixed data sources.
    /// </summary>
    /// <remarks>
    /// This implementation is not to be used for SQL bulk imports. 
    /// 
    /// The implementation is type agnostic. The list of supported types are referenced from <see cref="F:System.Data.DbType"/>
    /// </remarks>
    public abstract class ImporterBase : IDataReader
    {
        #region Fields

        /// <summary>
        /// The mapping from the row set input to the target table's columns.
        /// </summary>
        private List<ColumnMapping> _columnMappings = new List<ColumnMapping>();

        private bool _isOpen = true;

        /// <summary>
        /// List of columns that are handled through this interface
        /// </summary>
        private readonly List<ColumnDefinition> _columnDefinitions = new List<ColumnDefinition>();

        #endregion

        #region Subclass utility routines

        /// <summary>
        /// The mapping from the row set input to the target table's columns.
        /// </summary>
        /// <remarks>
        /// If necessary, <see cref="ImporterBase.AddSchemaTableRows()"/> will be called to initialize the mapping.
        /// </remarks>
        /// <exception cref="InvalidOperationException" accessor="get">AddSchemaTableRows did not add rows.</exception>
        public ReadOnlyCollection<ColumnMapping> ColumnMappings
        {
            get
            {
                if (_columnMappings.Count == 0)
                {
                    // Need to add the column definitions and mappings.
                    AddSchemaTableRows();

                    if (_columnMappings.Count == 0)
                    {
                        throw new InvalidOperationException("AddSchemaTableRows did not add rows.");
                    }
                }

                return new ReadOnlyCollection<ColumnMapping>(_columnMappings);
            }
        }


        /// <summary>
        /// Gets the column definitions.
        /// </summary>
        public ReadOnlyCollection<ColumnDefinition> ColumnDefinitions
        {
            get
            {
                return new ReadOnlyCollection<ColumnDefinition>(_columnDefinitions);
            }
        }


        /// <summary>
        /// Adds a new column definition.
        /// </summary>
        /// <returns>The column definition.</returns>
        public ColumnDefinition AddColumnDefinition()
        {
            ColumnDefinition definition = new ColumnDefinition();
            definition.Ordinal = _columnDefinitions.Count;
            _columnDefinitions.Add(definition);
            return definition;
        }

        /// <summary>
        /// Adds a new column definition with name and type
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="type">The type.</param>
        /// <returns>ColumnDefinition.</returns>
        public ColumnDefinition AddColumnDefinition(string columnName, DbType type)
        {
            ColumnDefinition definition = AddColumnDefinition();
            definition.Name = columnName;
            definition.Type = type;
            return definition;
        }

        /// <summary>
        /// Inserts the column definition at the given position
        /// </summary>
        /// <param name="position">The position (zero based).</param>
        /// <param name="columnName">Name of the column.</param>
        /// <param name="type">The type.</param>
        /// <returns>ColumnDefinition.</returns>
        public ColumnDefinition InsertColumnDefinition(int position, string columnName, DbType type)
        {
            ColumnDefinition definition = new ColumnDefinition();
            definition.Name = columnName;
            definition.Ordinal = position;
            definition.Type = type;
            _columnDefinitions.Insert(position, definition);
            return definition;
        }

            
        /// <summary>
        /// Adds the input row set's schema to the object.
        /// </summary>
        protected abstract void AddSchemaTableRows();


        /// <summary>
        /// Adds a column mapping.
        /// </summary>
        /// <param name="column">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        protected virtual void AddColumnMapping(string column, string newColumn)
        {
            _columnMappings.Add(new ColumnMapping(column, newColumn));
        }

        #endregion

        #region Constructors


        #endregion

        #region IDataReader

        /// <summary>
        /// Gets a value indicating the depth of nesting for the current row. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <remarks>
        /// This is not used for this implementation, therefore 0 and not overwritable
        /// </remarks>
        /// <seealso cref="IDataReader.Depth"/>
        public int Depth
        {
            get { return 0; }
        }

        /// <summary>
        /// Gets the number of columns in the current row. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <seealso cref="IDataRecord.FieldCount"/>
        public virtual int FieldCount {
            get { return _columnDefinitions.Count(); }
        }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <seealso cref="IDataReader.IsClosed"/>
        public virtual bool IsClosed
        {
            get { return !_isOpen; }
        }

        /// <summary> Gets the column located at the specified index. (Inherited from <see cref="IDataReader"/>.)</summary>
        /// <param name="i">The zero-based index of the column to get. </param>
        /// <returns>
        /// The column located at the specified index as an <see cref="Object"/>.
        /// </returns>
        /// <seealso cref="P:IDataRecord.Item(Int32)"/>
        /// <exception cref="IndexOutOfRangeException">
        /// No column with the specified index was found.
        /// </exception>
        public object this[int i]
        {
            get { return GetValue(i); }
        }

        /// <summary> Gets the column with the specified name. (Inherited from <see cref="IDataReader"/>.) </summary>
        /// <param name="name"> The name of the column to find. </param>
        /// <returns> The column located at the specified name as an <see cref="Object"/>. </returns>
        /// <seealso cref="P:IDataRecord.Item(String)"/>
        /// <exception cref="IndexOutOfRangeException">No column with the specified name was found.</exception>
        public object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        /// <summary> Gets the number of rows changed, inserted, or deleted by execution of the SQL statement. (Inherited from <see cref="IDataReader"/>.) </summary>
        /// <remarks> Always returns -1 (not used for this implementation) </remarks>
        /// <seealso cref="IDataReader.RecordsAffected"/>
        public int RecordsAffected
        {
            get { return -1; }
        }

        /// <summary>
        /// Closes the <see cref="IDataReader"/>. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <seealso cref="IDataReader.Close"/>
        public virtual void Close()
        {
            _isOpen = false;
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Boolean"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetBoolean(Int32)"/>
        public bool GetBoolean(int i)
        {
            return (bool) GetValue(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Byte"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetByte(Int32)"/>
        public byte GetByte(int i)
        {
            return (byte) GetValue(i);
        }

        /// <summary>
        /// Reads a stream of bytes from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <remarks>
        /// If you pass a buffer that is null, <see cref="GetBytes"/> returns the length of the row in bytes.
        /// </remarks>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <param name="fieldOffset">
        /// The index within the field from which to start the read operation.
        /// </param>
        /// <param name="buffer">
        /// The buffer into which to read the stream of bytes.
        /// </param>
        /// <param name="bufferOffset">
        /// The index for buffer to start the read operation.
        /// </param>
        /// <param name="length">
        /// The number of bytes to read.
        /// </param>
        /// <returns>
        /// The actual number of bytes read.
        /// </returns>
        /// <seealso cref="IDataRecord.GetBytes(Int32,Int64,Byte[],Int32,Int32)"/>
        /// <exception cref="IndexOutOfRangeException">The index passed was outside the range of 0 through <see cref="FieldCount"/>.</exception>
        /// <exception cref="ArgumentNullException">The value of 'buffer' cannot be null. </exception>
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferOffset, int length)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            byte[] data = (byte[]) GetValue(i);

            Array.Copy(data, fieldOffset, buffer, bufferOffset, length);

            return data.LongLength;
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Char"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <seealso cref="IDataRecord.GetChar(Int32)"/>
        public char GetChar(int i)
        {
            char result;

            object data = GetValue(i);
            char? dataAsChar = data as char?;
            char[] dataAsCharArray = data as char[];
            string dataAsString = data as string;

            if (dataAsChar.HasValue)
            {
                result = dataAsChar.Value;
            }
            else if (dataAsCharArray != null &&
                     dataAsCharArray.Length == 1)
            {
                result = dataAsCharArray[0];
            }
            else if (dataAsString != null &&
                     dataAsString.Length == 1)
            {
                result = dataAsString[0];
            }
            else
            {
                throw new InvalidOperationException("GetValue did not return a Char compatible type.");
            }

            return result;
        }

        /// <summary>
        /// Reads a stream of characters from the specified column offset into the buffer as an array, starting at the given buffer offset.
        /// (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <remarks>
        /// If you pass a buffer that is null, <see cref="GetChars"/> returns the length of the row in bytes.
        /// </remarks>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <param name="fieldOffset">
        /// The index within the field from which to start the read operation.
        /// </param>
        /// <param name="buffer">
        /// The buffer into which to read the stream of characters.
        /// </param>
        /// <param name="bufferOffset">
        /// The index for buffer to start the read operation.
        /// </param>
        /// <param name="length">
        /// The number of characters to read.
        /// </param>
        /// <returns>
        /// The actual number of characters read.
        /// </returns>
        /// <seealso cref="IDataRecord.GetChars(Int32,Int64,Char[],Int32,Int32)"/>
        /// <exception cref="InvalidOperationException">GetValue did not return either a Char array or a String.</exception>
        public long GetChars(int i, long fieldOffset, char[] buffer, int bufferOffset, int length)
        {
            object data = GetValue(i);

            string dataAsString = data as string;
            char[] dataAsCharArray = data as char[];

            if (dataAsString != null)
            {
                dataAsCharArray = dataAsString.ToCharArray((int) fieldOffset, length);
            }
            else if (dataAsCharArray == null)
            {
                throw new InvalidOperationException("GetValue did not return either a Char array or a String.");
            }

            Array.Copy(dataAsCharArray, fieldOffset, buffer, bufferOffset, length);

            return dataAsCharArray.LongLength;
        }

        /// <summary>
        /// The data type information for the specified field. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The data type information for the specified field.
        /// </returns>
        /// <seealso cref="IDataRecord.GetDataTypeName(Int32)"/>
        public string GetDataTypeName(int i)
        {
            // ReSharper disable once PossibleNullReferenceException
            return GetFieldType(i).Name;
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="DateTime"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetDateTime(Int32)"/>
        public DateTime GetDateTime(int i)
        {
            return (DateTime) GetValue(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        public DateTimeOffset GetDateTimeOffset(int i)
        {
            return (DateTimeOffset) GetValue(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Decimal"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetDecimal(Int32)"/>
        public decimal GetDecimal(int i)
        {
            return (Decimal) GetValue(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Double"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetDouble(Int32)"/>
        public double GetDouble(int i)
        {
            return (double) GetValue(i);
        }

        /// <summary>
        /// Gets the <see cref="Type"/> information corresponding to the type of <see cref="Object"/> that would be returned from <see cref="GetValue"/>.
        /// (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The <see cref="Type"/> information corresponding to the type of <see cref="Object"/> that would be returned from <see cref="GetValue"/>.
        /// </returns>
        /// <seealso cref="IDataRecord.GetFieldType(Int32)"/>
        [NotNull]
        public virtual Type GetFieldType(int i)
        {
            ColumnDefinition column = _columnDefinitions[i];
            switch (column.Type)
            {
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.AnsiString:
                case DbType.Xml:
                case DbType.String:
                    return typeof(string);
                case DbType.Binary:
                    return typeof (byte[]);
                case DbType.Byte:
                    return typeof (byte);
                case DbType.Boolean:
                    return typeof (bool);
                case DbType.Decimal:
                case DbType.VarNumeric:
                case DbType.Currency:
                    return typeof (decimal);
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                case DbType.Time:
                    return typeof(DateTime);
                case DbType.Double:
                    return typeof(double);
                case DbType.Guid:
                    return typeof(Guid);
                case DbType.Int16:
                    return typeof(short);
                case DbType.Int32:
                    return typeof(Int32);
                case DbType.Int64:
                    return typeof(Int64);
                case DbType.Object:
                    return typeof(object);
                case DbType.SByte:
                    return typeof(SByte);
                case DbType.Single:
                    return typeof (float);
                case DbType.UInt16:
                    return typeof(UInt16);
                case DbType.UInt32:
                    return typeof(UInt32);
                case DbType.UInt64:
                    return typeof(UInt64);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Single"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetFloat(Int32)"/>
        public float GetFloat(int i)
        {
            return (float) this[i];
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Guid"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetGuid(Int32)"/>
        public Guid GetGuid(int i)
        {
            return (Guid) GetValue(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Int16"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetInt16(Int32)"/>
        public short GetInt16(int i)
        {
            return (short) GetValue(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Int32"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetInt32(Int32)"/>
        public int GetInt32(int i)
        {
            return (int) GetValue(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Int64"/>.  (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetInt64(Int32)"/>
        public long GetInt64(int i)
        {
            return (long) GetValue(i);
        }

        /// <summary>
        /// Gets the name for the field to find. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The name of the field or the empty string (""), if there is no value to return.
        /// </returns>
        /// <seealso cref="IDataRecord.GetName(Int32)"/>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="IDataReader"/> is closed.
        /// </exception>
        public string GetName(int i)
        {
            // ReSharper disable once PossibleNullReferenceException
            return (string) GetSchemaTable().Rows[i][SchemaTableColumn.ColumnName];
        }

        /// <summary>
        /// Return the index of the named field. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <param name="name">
        /// The name of the field to find.
        /// </param>
        /// <returns>
        /// The index of the named field.
        /// </returns>
        /// <seealso cref="IDataRecord.GetOrdinal(String)"/>
        /// <exception cref="ArgumentNullException">The value of 'name' cannot be null. </exception>
        public int GetOrdinal(string name)
        {
            return ColumnDefinitions.Single(x => x.Name == name).Ordinal;
        }

        /// <summary>
        /// Returns a <see cref="DataTable"/> that describes the column metadata of the <see cref="IDataReader"/>. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="IDataReader"/> is closed.
        /// </exception>
        /// <returns>
        /// A <see cref="DataTable"/> that describes the column metadata.
        /// </returns>
        /// <seealso cref="IDataReader.GetSchemaTable()"/>
        public DataTable GetSchemaTable()
        {
            throw new InvalidOperationException("If you use this BulkDataReader with SqlBulkReader, you have to override this function.");
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="String"/>. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetString(Int32)"/>
        public string GetString(int i)
        {
            return (string) GetValue(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="TimeSpan"/>.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        public TimeSpan GetTimeSpan(int i)
        {
            return (TimeSpan) GetValue(i);
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Object"/>. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        /// <seealso cref="IDataRecord.GetValue(Int32)"/>
        public abstract object GetValue(int i);

        /// <summary>
        /// Populates an array of objects with the column values of the current record. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="values"/> was null.
        /// </exception>
        /// <param name="values">
        /// An array of <see cref="Object"/> to copy the attribute fields into.
        /// </param>
        /// <returns>
        /// The number of instances of <see cref="Object"/> in the array.
        /// </returns>
        /// <seealso cref="IDataRecord.GetValues(Object[])"/>
        /// <exception cref="OverflowException">Das Array ist mehrdimensional und enthält mehr als <see cref="F:System.Int32.MaxValue" />-Elemente.</exception>
        public int GetValues(object[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            int fieldCount = Math.Min(FieldCount, values.Length);

            for (int i = 0; i < fieldCount; i++)
            {
                values[i] = GetValue(i);
            }

            return fieldCount;
        }

        /// <summary>
        /// Return whether the specified field is set to null. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// True if the specified field is set to null; otherwise, false.
        /// </returns>
        /// <seealso cref="IDataRecord.IsDBNull(Int32)"/>
        public bool IsDBNull(int i)
        {
            object data = GetValue(i);

            return data == null || Convert.IsDBNull(data);
        }

        /// <summary>
        /// Advances the data reader to the next result, when reading the results of batch SQL statements. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <remarks>
        /// <see cref="IDataReader"/> for <see cref="SqlBulkCopy"/> returns a single result set so false is always returned.
        /// </remarks>
        /// <returns>
        /// True if there are more rows; otherwise, false. <see cref="IDataReader"/> for <see cref="SqlBulkCopy"/> returns a single result set so false is always returned.
        /// </returns>
        /// <seealso cref="IDataReader.NextResult()"/>
        public bool NextResult()
        {
            return false;
        }

        /// <summary>
        /// Advances the <see cref="IDataReader"/> to the next record. (Inherited from <see cref="IDataReader"/>.)
        /// </summary>
        /// <returns>
        /// True if there are more rows; otherwise, false.
        /// </returns>
        /// <seealso cref="IDataReader.Read()"/>
        public abstract bool Read();

        IDataReader IDataRecord.GetData(int ordinal)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Has the object been disposed?
        /// </summary>
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Dispose of any disposable and expensive resources.
        /// </summary>
        /// <param name="disposing">
        /// Is this call the result of a <see cref="IDisposable.Dispose"/> call?
        /// </param>
        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;

                if (disposing)
                {
                    _columnMappings = null;

                    _isOpen = false;

                    GC.SuppressFinalize(this);
                }
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        /// <remarks>
        /// <see cref="ImporterBase"/> has no unmanaged resources but a subclass may thus a finalizer is required.
        /// </remarks>
        ~ImporterBase()
        {
            Dispose(false);
        }

        #endregion
    }
}