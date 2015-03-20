using System;
using System.Collections.ObjectModel;

namespace StreamImporter.Base
{
    public interface IStreamImporter
    {
        /// <summary>
        /// Gets the column definitions.
        /// </summary>
        ReadOnlyCollection<IColumnDefinition> ColumnDefinitions { get; }

        /// <summary>
        /// Gets the number of columns in the current row.
        /// </summary>
        int FieldCount { get; }

        /// <summary>
        /// Gets a value indicating whether the data reader is closed.
        /// </summary>
        bool IsClosed { get; }

        /// <summary>
        /// Adds a new column definition with name and type. If the <c>columnName</c> exists, the given record will be upated.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>ColumnDefinition.</returns>
        IColumnDefinition AddColumnDefinition<T>(string columnName);

        IColumnDefinition AddColumnDefinition(string columnName, Type type);
        IColumnDefinition AddColumnDefinition(string columnName, Type columnType, Type columnDefinitionType);
        void SetColumnTypeDefinition<T1>(Type type);
        IColumnDefinition SetDataType(int ordinal, Type columnDefinitionType);
        IColumnDefinition SetDataType(string columnName, Type columnDefinitionType);

        /// <summary>
        /// Defines the data structure to be used for the import.
        /// </summary>
        void SetupColumns();

        /// <summary>
        /// Adds a column mapping.
        /// </summary>
        /// <param name="column">The old column.</param>
        /// <param name="newColumn">The new column.</param>
        void MapColumn(string column, string newColumn);

        /// <summary> Gets the column data located at the specified index. </summary>
        /// <param name="i">The zero-based index of the column to get. </param>
        /// <returns>
        /// The column located at the specified index as an <see cref="Object"/>.
        /// </returns>
        object this[int i] { get; }

        /// <summary> Gets the column with the specified name.
        /// </summary>
        /// <param name="name"> The name of the column to find. </param>
        /// <returns> The column located at the specified name as an <see cref="Object"/>. </returns>
        object this[string name] { get; }

        /// <summary>
        /// Closes the reader.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets the column definition for the field to find.
        /// </summary>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The field or null in case the field index is out of bounds
        /// </returns>
        IColumnDefinition GetColumnDefinition(int i);

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
        int GetOrdinal(string name);

        /// <summary>
        /// Gets the value of the specified column as a <see cref="Object"/>. 
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="StreamImporterBase.FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// The value of the column.
        /// </returns>
        object GetValue(int i);

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
        int GetValues(object[] values);

        /// <summary>
        /// Return whether the specified field is set to null. 
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">
        /// The index passed was outside the range of 0 through <see cref="StreamImporterBase.FieldCount"/>.
        /// </exception>
        /// <param name="i">
        /// The zero-based column ordinal.
        /// </param>
        /// <returns>
        /// True if the specified field is set to null; otherwise, false.
        /// </returns>
        bool IsNull(int i);

        /// <summary>
        /// Advances the reader to the next record.
        /// </summary>
        /// <returns>
        /// True if there are more rows; otherwise, false.
        /// </returns>
        bool Read();
    }
}