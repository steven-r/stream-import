using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using StreamImporter.Base.ColumnDefinitions;

namespace StreamImporter.Base
{
    /// <summary>
    /// (Abstract) base class to define basic operations for the import
    /// </summary>
    public abstract class StreamImporterBase : IStreamImporter
    {
        #region Fields

        private bool _isOpen = true;

        /// <summary>
        /// List of columns that are handled through this interface
        /// </summary>
        private readonly List<IColumnDefinition> _columnDefinitions = new List<IColumnDefinition>();

        #endregion

        #region Subclass utility routines

        /// <summary>
        /// Gets the column definitions.
        /// </summary>
        public ReadOnlyCollection<IColumnDefinition> ColumnDefinitions
        {
            get
            {
                return new ReadOnlyCollection<IColumnDefinition>(_columnDefinitions);
            }
        }

        #region AddColumnDefinition

        private Dictionary<Type, Type> _standardTypeColumnDefinitionMapping = new Dictionary<Type, Type>
        {
            {typeof (string), typeof (StringColumnDefinition)},
            {typeof (int), typeof (Int32ColumnDefinition)},
            {typeof (decimal), typeof (DecimalColumnDefinition)},
            {typeof (short), typeof (Int16ColumnDefinition)},
        };

        private CultureInfo _cultureInfo;

        /// <summary>
        /// Adds a new column definition with name and type. If the <c>columnName</c> exists, the given record will be upated.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>ColumnDefinition.</returns>
        public IColumnDefinition AddColumnDefinition<T>(string columnName)
        {
            Type targetType;
            if (_standardTypeColumnDefinitionMapping.TryGetValue(typeof (T), out targetType))
            {
                return AddColumnDefinition(columnName, typeof (T), targetType);
            }
            else
            {
                throw new InvalidOperationException("Cannot map the following type to ColumnDefinition: " + typeof(T).FullName);
            }
        }

        public IColumnDefinition AddColumnDefinition(string columnName, Type type)
        {
            Type targetType;
            if (_standardTypeColumnDefinitionMapping.TryGetValue(type, out targetType))
            {
                return AddColumnDefinition(columnName, type, targetType);
            }
            else
            {
                throw new InvalidOperationException("Cannot map the following type to ColumnDefinition: " + type.FullName);
            }
        }

        public IColumnDefinition AddColumnDefinition(string columnName, Type columnType, Type columnDefinitionType)
        {
            #region Checks
            if (columnDefinitionType == null)
            {
                throw new ArgumentNullException("columnDefinitionType");
            }
            if (columnType == null)
            {
                throw new ArgumentNullException("columnType");
            }
            if (columnName == null)
            {
                throw new ArgumentNullException("columnName");
            }
            if (columnName.Length == 0)
            {
                throw new ArgumentException("columnName must contain a value");
            }
            #endregion

            IColumnDefinition definition = _columnDefinitions.SingleOrDefault(x => x.Name == columnName);
            if (definition == null)
            {
                definition = (IColumnDefinition)Activator.CreateInstance(columnDefinitionType);
                definition.Ordinal = _columnDefinitions.Count;
                definition.CultureInfo = CultureInfo;
                _columnDefinitions.Add(definition);
            }
            definition.Name = columnName;
            definition.InputColumn = columnName;
            definition.Type = columnType;
            return definition;
        }


        public void SetColumnTypeDefinition<T1>(Type type)
        {
            _standardTypeColumnDefinitionMapping[typeof (T1)] = type;
        }


        public IColumnDefinition SetDataType(int ordinal, Type columnDefinitionType)
        {
            IColumnDefinition def = _columnDefinitions.Single(x => x.Ordinal == ordinal);
            int position = _columnDefinitions.IndexOf(def);
            IColumnDefinition newColumnDefinition = (IColumnDefinition)Activator.CreateInstance(columnDefinitionType);
            newColumnDefinition.Name = def.Name;
            newColumnDefinition.InputColumn = def.InputColumn;
            newColumnDefinition.Ordinal = def.Ordinal;
            newColumnDefinition.CultureInfo = CultureInfo;
            _columnDefinitions[position] = newColumnDefinition;
            return newColumnDefinition;
        }

        public IColumnDefinition SetDataType(string columnName, Type columnDefinitionType)
        {
            IColumnDefinition def = _columnDefinitions.Single(x => x.Name == columnName);
            return SetDataType(def.Ordinal, columnDefinitionType);
        }

        #endregion

        /// <summary>
        /// Defines the data structure to be used for the import.
        /// </summary>
        public abstract void SetupColumns();


        /// <summary>
        /// Adds a column mapping.
        /// </summary>
        /// <param name="column">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        public virtual void MapColumn(string column, string newColumn)
        {
            IColumnDefinition definition = _columnDefinitions.SingleOrDefault(x => x.Name == column);
            if (definition == null)
            {
                throw new ArgumentException("Column does not exist", column);
            }
            definition.InputColumn = newColumn;
        }

        #endregion

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        public virtual int FieldCount {
            get { return _columnDefinitions.Count(); }
        }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        public virtual bool IsClosed
        {
            get { return !_isOpen; }
        }

        public CultureInfo CultureInfo
        {
            get {
                if (_cultureInfo == null)
                {
                    return CultureInfo.CurrentCulture;
                }
                return _cultureInfo;
            }
            set { _cultureInfo = value; }
        }

        /// <summary> Gets the column data located at the specified index. </summary>
        /// <param name="i">The zero-based index of the column to get. </param>
        /// <returns>
        /// The column located at the specified index as an <see cref="Object"/>.
        /// </returns>
        /// <exception cref="IndexOutOfRangeException">
        /// No column with the specified index was found.
        /// </exception>
        public object this[int i]
        {
            get { return GetValue(i); }
        }

        /// <summary> Gets the column with the specified name.
        /// </summary>
        /// <param name="name"> The name of the column to find. </param>
        /// <returns> The column located at the specified name as an <see cref="Object"/>. </returns>
        /// <exception cref="IndexOutOfRangeException">No column with the specified name was found.</exception>
        public object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        /// <summary>
        /// Closes the reader.
        /// </summary>
        public virtual void Close()
        {
            _isOpen = false;
        }

        /// <summary>
        /// Gets the column definition for the field to find.
        /// </summary>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The field or null in case the field index is out of bounds
        /// </returns>
        public IColumnDefinition GetColumnDefinition(int i)
        {
            if (i < 0 || i > FieldCount)
            {
                return null;
            }
            return _columnDefinitions[i];
        }

        /// <summary>
        /// Return the index of the named field.
        /// </summary>
        /// <param name="name">
        /// The name of the field to find.
        /// </param>
        /// <returns>
        /// The index of the named field.
        /// </returns>
        /// <exception cref="ArgumentNullException">The value of 'name' cannot be null. </exception>
        public int GetOrdinal(string name)
        {
            return ColumnDefinitions.Single(x => x.Name == name).Ordinal;
        }

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Object"/>. 
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
        public abstract object GetValue(int i);

        /// <summary>
        /// Populates an array of objects with the column values of the current record. 
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
        /// Return whether the specified field is set to null. 
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
        public bool IsNull(int i)
        {
            object data = GetValue(i);

            return data == null || Convert.IsDBNull(data);
        }

        /// <summary>
        /// Advances the reader to the next record.
        /// </summary>
        /// <returns>
        /// True if there are more rows; otherwise, false.
        /// </returns>
        public abstract bool Read();

    }
}