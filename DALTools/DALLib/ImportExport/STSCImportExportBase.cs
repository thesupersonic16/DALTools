using DALLib.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.ImportExport
{
    public abstract class STSCImportExportBase
    {

        public abstract string TypeName { get; }
        // Formated with a fullstop following with the extension e.g. ".bin"
        public abstract string TypeExtension { get; }

        public abstract void   Import(STSCFile script, STSCFileDatabase database, string file);
        public abstract string Export(STSCFile script, STSCFileDatabase database);

    }
}
