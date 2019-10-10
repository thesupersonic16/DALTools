using DALLib.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.Exceptions
{
    public class STSCDisassembleException : Exception
    {
        // Constructors
        public STSCDisassembleException(STSCFile script, string reason) :
            base(string.Format(
            "Failed to disassemble the script file {0}. {1}",
            script.ScriptName, reason))
        { }
    }
}
