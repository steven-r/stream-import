using System.Data;

namespace StreamImporter.Base
{
    public class ColumnDefinition
    {
        public string Name { get; set; }

        public DbType Type { get; set; }

        public int Ordinal { get; set; }
    }
}