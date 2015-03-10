using System;

namespace StreamImporter.Base
{
    public class ColumnMapping
    {
        public ColumnMapping(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }

        public string OldName { get; set; }

        public string NewName { get; set; }
    }
}
