using DALLib.File;
using DALLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDialogueEditor
{
    // TODO: Generalise class
    public class CharaFile : FileBase
    {
        
        public List<CharaEntry> CharaEntries = new List<CharaEntry>();

        public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            int columnCount = reader.ReadInt32();
            int entryCount = reader.ReadInt32();
            int entryPosition = reader.ReadInt32();
            // Type, String
            
            // Skip
            reader.JumpTo(entryPosition);
            for (int i = 0; i < entryCount; i++)
                CharaEntries.Add(reader.ReadStruct<CharaEntry>());
        }

        public class CharaEntry
        {
            public int No { get; set; }
            public int ContNo { get; set; }
            public string Name { get; set; }
        }

    }
}
