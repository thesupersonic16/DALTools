using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.Exceptions
{
    public class InvalidTextureFormatException : Exception
    {
        // Constructors
        public InvalidTextureFormatException(int format) :
            base(string.Format(
            "The texture that is currently loading contains an invalid or unknown format! (0x{0:X8})", format))
        { }
    }
}
