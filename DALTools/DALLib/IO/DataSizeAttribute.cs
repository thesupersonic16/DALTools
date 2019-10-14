using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.IO
{
    public class DataSizeAttribute : Attribute
    {
        public int Size = 4;

        public DataSizeAttribute(int size)
        {
            Size = size;
        }
    }
}
