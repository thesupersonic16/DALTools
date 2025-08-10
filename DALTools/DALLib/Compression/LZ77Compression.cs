using DALLib.IO;
using System;
using System.IO;

namespace DALLib.Compression
{
    /// <summary>
    /// LZ77 compression used for files that were directly ported from the PlayStation
    /// <para/>
    /// Decompressor is based off ps_lz77 from QuickBMS
    /// </summary>
    public static class LZ77Compression
    {
        public const int LZ77_MAX_WINDOW_SIZE = 0x50;

        public static byte[] CompressLZ77(this byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            using (var dataMemoryStream = new MemoryStream())
            using (var writer = new ExtendedBinaryWriter(memoryStream))
            using (var dataWriter = new ExtendedBinaryWriter(dataMemoryStream))
            {
                writer.WriteSignature("LZ77");
                writer.Write(bytes.Length);
                // This field is unknown and required by the games
                writer.Write(bytes.Length / 4);
                writer.AddOffset("Offset");

                int dataPointer = 0;
                int flagPosition = 0;
                int currentFlag = 0;
                while (dataPointer < bytes.Length)
                {
                    (int bestOffset, int bestLength) = FindLongestMatch(bytes, dataPointer, 40);
                    if (bestOffset < 0 || bestLength < 3)
                    {
                        // No match
                        dataWriter.Write(bytes[dataPointer++]);
                    }
                    else
                    {
                        // Write match
                        currentFlag |= 1 << (7 - flagPosition);
                        dataWriter.Write((byte)bestOffset); // Back step
                        dataWriter.Write((byte)(bestLength - 3)); // Amount
                        dataPointer += bestLength;
                    }
                    flagPosition++;
                    if (flagPosition == 8)
                    {
                        writer.Write((byte)currentFlag);
                        currentFlag = 0;
                        flagPosition = 0;
                    }
                }

                writer.FillInOffset("Offset");
                writer.Write(dataMemoryStream.ToArray());

                return memoryStream.ToArray();
            }
        }

        public static byte[] DecompressLZ77(this byte[] compressed)
        {
            byte[] buffer = null;
            using (var reader = new ExtendedBinaryReader(new MemoryStream(compressed)))
            {
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
            }
            return buffer;
        }

        public static (int, int) FindLongestMatch(byte[] data, int position, int limit)
        {
            int bestOffset = -1;
            int bestLength = -1;
            int maxOffset = Math.Min(data.Length - position, limit);

            for (int offsetSize = 2; offsetSize < maxOffset; offsetSize++)
            {
                int searchStartPosition = Math.Max(position - LZ77_MAX_WINDOW_SIZE, 0);
                byte[] pattern = new byte[offsetSize];
                Array.Copy(data, position, pattern, 0, offsetSize);

                for (int searchPosition = searchStartPosition; searchPosition < position; searchPosition++)
                {
                    if (data[searchPosition] != pattern[0])
                        continue;

                    int matchLength = 1;
                    while (matchLength < offsetSize && searchPosition + matchLength < position &&
                           data[searchPosition + matchLength] == pattern[matchLength % offsetSize])
                        matchLength++;
                    if (matchLength > bestLength)
                    {
                        bestOffset = position - searchPosition;
                        bestLength = matchLength;
                    }
                }
            }

            return (bestOffset, bestLength);
        }
    }
}
