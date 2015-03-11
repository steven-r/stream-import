using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamImporter.Base.ColumnDefinitions
{
    public class IgnoredColumnDefinition: ColumnDefinitionBase<DBNull>
    {
        public IgnoredColumnDefinition()
        {
            Type = typeof (DBNull);
        }

        public override DBNull Convert(string value)
        {
            return DBNull.Value;
        }
    }
}
