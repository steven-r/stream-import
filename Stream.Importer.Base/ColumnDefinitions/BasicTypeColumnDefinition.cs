using System;
using System.Globalization;

namespace StreamImporter.Base.ColumnDefinitions
{
    public class StringColumnDefinition: ColumnDefinitionBase<string>
    {
        public StringColumnDefinition()
        {
            Type = typeof (string);
        }

        public override string Convert(string value)
        {
            return value;
        }
    }


    public class Int32ColumnDefinition : ColumnDefinitionBase<Int32>
    {
        public Int32ColumnDefinition()
        {
            Type = typeof(string);
        }

        public override Int32 Convert(string value)
        {
            return System.Convert.ToInt32(value, CultureInfo.CurrentCulture);
        }
    }

    public class Int16ColumnDefinition : ColumnDefinitionBase<Int16>
    {
        public Int16ColumnDefinition()
        {
            Type = typeof(string);
        }

        public override Int16 Convert(string value)
        {
            return System.Convert.ToInt16(value, CultureInfo.CurrentCulture);
        }
    }

    public class DecimalColumnDefinition : ColumnDefinitionBase<Decimal>
    {
        public DecimalColumnDefinition()
        {
            Type = typeof(string);
        }

        public override Decimal Convert(string value)
        {
            return System.Convert.ToDecimal(value, CultureInfo.CurrentCulture);
        }
    }

}
