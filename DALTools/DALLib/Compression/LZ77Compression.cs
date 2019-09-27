using DALLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.Compression
{
    /// <summary>
    /// LZ77 Decompressor used for decompressing files that were directly ported from the PlayStation
    /// <para/>
    /// Based off ps_lz77 from QuickBMS
    /// </summary>
    public static class LZ77Compression
    {

        public static byte[] DecompressLZ77(this byte[] compressed)
        {
            var reader = new ExtendedBinaryReader(new MemoryStream(compressed));
            byte[] buffer = null;
            int position = 0;
            long flagPosition = 0;
            if (reader.ReadSignature() == "LZ77")
            {
                int uncompressedSize = reader.ReadInt32();
                int lz77Step = reader.ReadInt32();
                int offset = reader.ReadInt32();
                flagPosition = reader.GetPosition();
                reader.JumpTo(offset);
                buffer = new byte[uncompressedSize];
            }

            int flagCount = 0;
            int flag = 0;
            while (true)
            {
                if (flagCount == 0)
                {
                    if (flagPosition >= compressed.Length)
                        break;
                    if (flagPosition == reader.GetPosition())
                        reader.JumpAhead(1);
                    flag = compressed[flagPosition++];
                    flagCount = 8;
                }
                if ((flag & 0x80) != 0)
                {
                    if (reader.GetPosition() + 2 > compressed.Length)
                        break;
                    int backStep = reader.ReadByte();
                    int amount = reader.ReadByte();
                    for (amount += 3; (amount--) != 0; position++)
                    {
                        if (position >= buffer.Length)
                            break;
                        buffer[position] = buffer[position - backStep];
                    }
                }
                else
                {
                    if (position >= buffer.Length)
                        break;
                    buffer[position++] = reader.ReadByte();
                }
                flag <<= 1;
                flagCount--;
            }
            return buffer;
        }
    }
}
