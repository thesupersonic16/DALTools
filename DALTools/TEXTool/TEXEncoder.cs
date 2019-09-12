using HedgeLib.IO;
using Scarlet.Drawing;
using Scarlet.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TEXTool.TEXFile;
using static Scarlet.Drawing.PixelDataFormat;

namespace TEXTool
{
    public static class TEXEncoder
    {

        public static byte[] Encode(this TEXFile file, Format format, ExtendedBinaryWriter writer)
        {
            switch (format)
            {
                case Format.RGBA: // Native
                    return file.SheetData;
                default:
                    Console.WriteLine("Unknown Format!");
                    Console.ReadKey();
                    return null;
            }
        }

    }
}
