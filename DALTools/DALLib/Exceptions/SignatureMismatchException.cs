using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.Exceptions
{
    public class SignatureMismatchException : Exception
    {
        // Constructors
        public SignatureMismatchException(string expectedSig, string readSig) : 
            base(string.Format(
            "The read signature does not match the expected signature! (Expected {0} got {1}.)",
            expectedSig, readSig))
        { }
    }
}
