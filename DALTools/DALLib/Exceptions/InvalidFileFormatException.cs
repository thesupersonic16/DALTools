using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.Exceptions
{
    public class InvalidFileFormatException : Exception
    {
        // Constructors
        public InvalidFileFormatException(string message) : base(message) { }
    }
}
