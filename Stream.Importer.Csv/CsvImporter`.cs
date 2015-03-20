using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using StreamImporter.Base;
using StreamImporter.Base.ColumnDefinitions;

namespace StreamImporter.Csv
{
    public class CsvStreamImporter<T>: CsvStreamImporter
        where T: class
    {
        #region Fields

        private readonly Type _t = typeof (T);
        private PropertyInfo[] _propertyInfos;

        public T Value;

        #endregion

        #region Handlers

        /// <summary>
        /// Mapping function to map csv headers to property names. 
        /// </summary>
        /// <remarks>
        /// Only used in case <see cref="P:CsvImporter.HasHeaders"/> is set.
        /// </remarks>
        /// <returns>
        /// A property name for the given header field. If a csv header cannot be mapped, <c>null</c> needs to be returned.
        /// In this case this column will be ignored.
        /// </returns>
        public Func<string, string> MapField = str => str;

        private Dictionary<string, PropertyInfo> _propertyMap;

        #endregion

        #region Constructors

        public CsvStreamImporter(Stream stream)
            :base(stream)
        {
            Value = (T)Activator.CreateInstance(_t);
        }

        public CsvStreamImporter(Stream stream, bool hasHeader)
            : base(stream, hasHeader)
        {
        }

        public CsvStreamImporter(Stream stream, bool hasHeader, char delimiter)
            : base(stream, hasHeader, delimiter)
        {
        }

        #endregion

        #region CsvImporterOverwrites

        public override void SetupColumns()
        {
            _propertyInfos =
                _t.GetProperties(BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.Public |
                                 BindingFlags.GetProperty);
            _propertyMap = _propertyInfos.ToDictionary(x => x.Name, y => y);

            if (_propertyInfos.Length == 0)
            {
                throw new InvalidCastException(string.Format("The type {0} does not contain any public property", _t.FullName));
            }
            List<string> csvHeaders = new List<string>();
            if (HasHeader)
            {
                csvHeaders = ReadHeaderLine();
                foreach (string header in csvHeaders)
                {
                    string propertyName = MapField(header);
                    if (string.IsNullOrEmpty(propertyName))
                    {
                        // ignore this column as it cannot be mapped to a target property.
                        AddColumnDefinition<IgnoredColumnDefinition>(header);
                    }
                    else
                    {
                        PropertyInfo info = _propertyInfos.SingleOrDefault(x => x.Name == propertyName);
                        if (info == null)
                        {
                            throw new InvalidCastException("Cannot find property " + propertyName);
                        }
                        IColumnDefinition def = AddColumnDefinition(info.Name, info.PropertyType);
                        def.InputColumn = header;
                    }

                }
            }
            else
            {
                foreach (PropertyInfo info in _propertyInfos)
                {
                    IColumnDefinition def = AddColumnDefinition(info.Name, info.PropertyType);
                    def.InputColumn = MapField(def.Name);
                }
            }
        }

        public override object GetValue(int i)
        {
            PropertyInfo info = _propertyInfos[i];
            if (info == null)
            {
                throw new InvalidOperationException("Uh... Cannot find property " + i);
            }
            if (Value == null)
            {
                return Value;
            }
            return info.GetValue(Value);
        }

        public override void SetData(string[] data)
        {
            var dataList = ColumnDefinitions.ToDictionary(x => x.Ordinal, y => y);
            for (int i = 0; i < data.Length; i++)
            {
                IColumnDefinition definition = dataList[i];
                if (definition.Type == typeof (DBNull))
                {
                    continue; // ignored
                }
                PropertyInfo info = _propertyMap[definition.Name];
                info.SetValue(Value, definition.Convert(data[i]));
            }   
        }

        #endregion
    }
}
