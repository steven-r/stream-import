using System;

namespace StreamImporter.Base
{
    /// <summary>
    /// A single column in the data area.
    /// </summary>
    /// <remarks>
    /// A <c>ColumnDescription</c> describes the format of a field in the target structure. The <see cref="ImporterBase"/> handling need
    /// to map the actual input to values of the given type <see cref="P:Type"/>.
    /// </remarks>
    public abstract class ColumnDefinitionBase<T>: IColumnDefinition<T>
    {
        #region TargetAttributes
        /// <summary>
        /// The name of the field in the target structure
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the target field..
        /// </summary>
        public Type Type { get; set; }
        #endregion

        #region Source attributes

        /// <summary>
        /// Gets or sets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        public int Ordinal { get; set; }
        
        public string InputColumn { get; set; }

        #endregion

        #region Converter

        // explicit implementation
        object IColumnDefinition.Convert(string value)
        {
            return Convert(value);
        }
        public abstract T Convert(string value);

        #endregion
    }

    public interface IColumnDefinition
    {
        #region TargetAttributes
        /// <summary>
        /// The name of the field in the target structure
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The type of the target field..
        /// </summary>
        Type Type { get; set; }
        #endregion

        #region Source attributes

        /// <summary>
        /// Gets or sets the ordinal.
        /// </summary>
        /// <value>The ordinal.</value>
        int Ordinal { get; set; }

        string InputColumn { get; set; }

        #endregion

        object Convert(string value);
    }

    public interface IColumnDefinition<out T>: IColumnDefinition
    {
        new T Convert(string value);
    }

}