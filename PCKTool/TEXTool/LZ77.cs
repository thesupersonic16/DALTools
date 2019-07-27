using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeLib.IO;

namespace TEXTool
{
    public static class LZ77
    {
        public static int Position(this ExtendedBinaryReader reader)
        {
            return (int)reader.BaseStream.Position;
        }

        public static byte[] Lz77Decompress(this byte[] compressed)
        {
            var reader = new ExtendedBinaryReader(new MemoryStream(compressed));
            byte[] buffer = null;
            int position = 0;
            int flagposition = 0;
            if (reader.ReadSignature() == "LZ77")
            {
                int uncompressedSize = reader.ReadInt32();
                int lz77Step = reader.ReadInt32();
                int offset = reader.ReadInt32();
                flagposition = reader.Position();
                reader.JumpTo(offset);
                buffer = new byte[uncompressedSize];
            }

            int flagCount = 0;
            int flag = 0;
            while (true)
            {
                if (flagCount == 0)
                {
                    if (flagposition >= compressed.Length)
                        break;
                    if (flagposition == reader.Position())
                        reader.JumpAhead(1);
                    flag = compressed[flagposition++];
                    flagCount = 8;
                }
                if ((flag & 0x80) != 0)
                {
                    if (reader.Position() + 2 > compressed.Length)
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
