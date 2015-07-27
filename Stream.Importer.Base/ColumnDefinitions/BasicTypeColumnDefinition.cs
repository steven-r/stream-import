using System;
using System.Diagnostics;

namespace StreamImporter.Base.ColumnDefinitions
{
    public class StringColumnDefinition: ColumnDefinitionBase<String>
    {
        public StringColumnDefinition()
        {
            Type = typeof (String);
        }

        public override string Convert(String value)
        {
            return value;
        }
    }


    public class Int32ColumnDefinition : ColumnDefinitionBase<Int32>
    {
        public Int32ColumnDefinition()
        {
            Type = typeof(Int32);
        }

        public override Int32 Convert(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(Int32);
            }
            return System.Convert.ToInt32(value, CultureInfo);
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
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(Int16);
            }
            return System.Convert.ToInt16(value, CultureInfo);
        }
    }

    public class DecimalColumnDefinition : ColumnDefinitionBase<decimal>
    {
        public DecimalColumnDefinition()
        {
            Type = typeof(decimal);
        }

        public override decimal Convert(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(decimal);
            }
            try
            {
                return System.Convert.ToDecimal(value, CultureInfo);
            }
            catch (FormatException fe)
            {
                throw new FormatException($"Huh: Formatting {value} with {CultureInfo.DisplayName} failed (delimiter: '{CultureInfo.NumberFormat.NumberDecimalDigits}', separator: '{CultureInfo.NumberFormat.NumberGroupSeparator}'", fe);
            }
        }
    }


    public class SingleColumnDefinition : ColumnDefinitionBase<Single>
    {
        public SingleColumnDefinition()
        {
            Type = typeof(string);
        }

        public override Single Convert(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(Single);
            }
            return System.Convert.ToSingle(value, CultureInfo);
        }
    }

    public class DoubleColumnDefinition : ColumnDefinitionBase<Double>
    {
        public DoubleColumnDefinition()
        {
            Type = typeof(Double);
        }

        public override Double Convert(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(Double);
            }
            return System.Convert.ToDouble(value, CultureInfo);
        }
    }

    public class BooleanColumnDefinition : ColumnDefinitionBase<Boolean>
    {
        public BooleanColumnDefinition()
        {
            Type = typeof(Boolean);
        }

        public override Boolean Convert(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return default(Boolean);
            }
            return System.Convert.ToBoolean(value, CultureInfo);
        }
    }

}
