using System;

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
            Type = typeof(string);
        }

        public override Int32 Convert(string value)
        {
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
            return System.Convert.ToInt16(value, CultureInfo);
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
            return System.Convert.ToDecimal(value, CultureInfo);
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
            return System.Convert.ToSingle(value, CultureInfo);
        }
    }

    public class DoubleColumnDefinition : ColumnDefinitionBase<Double>
    {
        public DoubleColumnDefinition()
        {
            Type = typeof(string);
        }

        public override Double Convert(string value)
        {
            return System.Convert.ToDouble(value, CultureInfo);
        }
    }

    public class BooleanColumnDefinition : ColumnDefinitionBase<Boolean>
    {
        public BooleanColumnDefinition()
        {
            Type = typeof(string);
        }

        public override Boolean Convert(string value)
        {
            return System.Convert.ToBoolean(value, CultureInfo);
        }
    }

}
