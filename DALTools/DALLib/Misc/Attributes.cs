using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.Misc
{
    public sealed class DataSizeAttribute : Attribute
    {
        public int Size = 4;

        public DataSizeAttribute(int size)
        {
            Size = size;
        }
    }

    public sealed class PropertyNameAttribute : Attribute
    {
        public string PropertyName { get; set; }

        public PropertyNameAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}
