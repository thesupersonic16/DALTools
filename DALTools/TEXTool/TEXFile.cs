using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HedgeLib.Exceptions;
using HedgeLib.IO;
using HedgeLib.Misc;
using Scarlet.Drawing;
using Scarlet.IO;
using zlib;

namespace TEXTool
{
    [Serializable]
    public class TEXFile : FileBase, IDisposable
    {

        public enum Format
        {
            None       = 0x0000_0000,
            DXT1       = 0x0000_0001,
            DXT5       = 0x0000_0002,
            Luminance8 = 0x0000_0080,
            Raster     = 0x0000_0200,
            Unknown    = 0x0000_1000,
            Large      = 0x0000_2000,
            RGBA       = 0x0000_4000,
            PNG        = 0x0001_0000
        }

        [Serializable]
        public class Frame
        {
            public float FrameWidth { get; set; }
            public float FrameHeight { get; set; }
            public float LeftScale { get; set; }
            public float TopScale { get; set; }
            public float RightScale { get; set; }
            public float BottomScale { get; set; }
        }

        public int SheetWidth;
        public int SheetHeight;
        public List<Frame> Frames = new List<Frame>();
        [XmlIgnore] public byte[] SheetData = null;

        // Options
        /// <summary>
        /// Toggle for using larger signatures (0x14) or smaller ones (0x08)
        /// DAL: RR uses larger signatures
        /// </summary>
        public bool UseSmallSig = false;
        /// <summary>
        /// Toggle for using Not including a signature, It is unknown what needs it or now
        /// Default is false (include signature) as most files needs it
        /// </summary>
        public bool Sigless = false;


        public static byte[] DecompressData(Stream inputStream, bool closeStream = true)
        {
            byte[] buffer = null;
            using (MemoryStream outMemoryStream = new MemoryStream())
            using (ZOutputStream outZStream = new ZOutputStream(outMemoryStream))
            {
                CopyStream(inputStream, outZStream);
                outZStream.finish();
                buffer = outMemoryStream.ToArray();
                if (closeStream)
                    inputStream.Close();
            }
            return buffer;
        }

        public static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[32];
            int len;
            while ((len = input.Read(buffer, 0, 32)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public override void Load(Stream fileStream)
        {
            var reader = new ExtendedBinaryReader(fileStream);

            // Decompress Zlib stream
            if (reader.PeekChar() == 'Z')
            {
                // Skip Zlib Header
                // 0x00 - "ZLIB"
                // 0x04 - UncompressedSize
                // 0x08 - CompressedSize
                // 0x0C - Zlib Data
                reader.JumpAhead(12);
                reader = new ExtendedBinaryReader(new MemoryStream(DecompressData(reader.BaseStream)));
            }

            // Read Signature to guess the type of TEX
            int sigSize = CheckPCKSig(reader, "Texture");
            Sigless = sigSize < 4;
            UseSmallSig = sigSize <= 8;

            int textureSectionSize = reader.ReadInt32();
            if (Sigless)
                reader.JumpTo(0);
            Format format = (Format)(reader.ReadUInt16() | (reader.ReadByte() << 16));
            byte unknown = reader.ReadByte();
            int dataLength;

            // Read Image Information
            if ((unknown & 0b00000001) != 0)
            {
                dataLength = reader.ReadInt32();
                SheetWidth = reader.ReadInt32();
                SheetHeight = reader.ReadInt32();
            }
            else
            {
                int version = reader.ReadInt32();
                dataLength = reader.ReadInt32();
                SheetWidth = reader.ReadInt16();
                SheetHeight = reader.ReadInt16();
            }
            
            // Read Image Data
            SheetData = reader.ReadBytes(dataLength);
            
            // LZ77 Decompression
            if (BitConverter.ToInt32(SheetData, 0) == 0x37375A4C)
                SheetData = SheetData.Lz77Decompress();

            // Decompress/Process Image based on format
            TEXDecoder.Decode(this, format, reader);


            // Parts
            sigSize = CheckPCKSig(reader, "Parts");
            if (sigSize < 4)
                return;
            int partsSectionSize = reader.ReadInt32();
            int partCount = reader.ReadInt32();
            for (int i = 0; i < partCount; ++i)
            {
                reader.JumpAhead(8); // Unknown
                float frameWidth = reader.ReadSingle();
                float frameHeight = reader.ReadSingle();
                float frameXScale = reader.ReadSingle();
                float frameYScale = reader.ReadSingle();
                float frameWidthScale = reader.ReadSingle();
                float frameHeightScale = reader.ReadSingle();
                Frames.Add(new Frame
                {
                    FrameWidth = frameWidth,
                    FrameHeight = frameHeight,
                    LeftScale = frameXScale,
                    TopScale = frameYScale,
                    RightScale = frameWidthScale,
                    BottomScale = frameHeightScale
                });
            }

            // Anime
            reader.FixPadding(0x8);
            sigSize = CheckPCKSig(reader, "Anime");
            if (sigSize < 4)
                return;
            int animeSectionSize = reader.ReadInt32();
            
        }

        // May not work, Only tested with title.tex
        public override void Save(Stream fileStream)
        {
            var writer = new ExtendedBinaryWriter(fileStream);

            if (!Sigless)
            {
                WritePCKSig(writer, "Texture", UseSmallSig);
                writer.AddOffset("HeaderSize");
            }

            // Format
            var format = Format.RGBA;
            bool flag1 = false;

            writer.Write((short)format);
            writer.Write((byte)((int)format >> 16));
            writer.Write((byte)0);
            if (!flag1)
            {
                writer.Write(0x8100000);
                writer.AddOffset("DataLength");
                writer.Write((short)SheetWidth);
                writer.Write((short)SheetHeight);
            }
            writer.Write(TEXEncoder.Encode(this, format, writer));
            writer.FillInOffset("DataLength", (uint)writer.BaseStream.Position - (Sigless ? 0x10u : 0x28u));

            // Sigless files do not have Parts or Anime
            if (Sigless)
                return;
            
            // Parts
            writer.FillInOffset("HeaderSize");
            long header = writer.BaseStream.Position;
            WritePCKSig(writer, "Parts", UseSmallSig);
            writer.AddOffset("HeaderSize");
            writer.Write(Frames.Count);

            foreach (var frame in Frames)
            {
                writer.WriteNulls(8);
                writer.Write(frame.FrameWidth);
                writer.Write(frame.FrameHeight);
                writer.Write(frame.LeftScale);
                writer.Write(frame.TopScale);
                writer.Write(frame.RightScale);
                writer.Write(frame.BottomScale);
            }
            writer.FillInOffset("HeaderSize", (uint)(writer.BaseStream.Position - header));
            writer.FixPadding(0x8);

            // Anime
            header = writer.BaseStream.Position;
            WritePCKSig(writer, "Anime", UseSmallSig);
            writer.AddOffset("HeaderSize");
            writer.WriteNulls(4);
            writer.FillInOffset("HeaderSize", (uint)(writer.BaseStream.Position - header));
            writer.FixPadding(0x10);

        }

        public void SaveImage(string path)
        {
            var Image = new Bitmap(SheetWidth, SheetHeight, PixelFormat.Format32bppArgb);

            // Copy Data into Bitmap
            var bitmap = Image.LockBits(new Rectangle(0, 0, SheetWidth, SheetHeight), ImageLockMode.ReadWrite, Image.PixelFormat);
            FlipColors();
            Marshal.Copy(SheetData, 0, bitmap.Scan0, SheetWidth * SheetHeight * 4);
            Image.UnlockBits(bitmap);
            Image.Save(path);
        }

        public void LoadImage(string path)
        {
            var image = new Bitmap(path);
            SheetWidth = (short) image.Width;
            SheetHeight = (short) image.Height;
            SheetData = new byte[SheetWidth * SheetHeight * 4];
            var bitmap = image.LockBits(new Rectangle(0, 0, SheetWidth, SheetHeight), ImageLockMode.ReadWrite,
                image.PixelFormat);
            Marshal.Copy(bitmap.Scan0, SheetData, 0, SheetWidth * SheetHeight * 4);
            image.UnlockBits(bitmap);
            FlipColors();
        }

        public void FlipColors()
        {
            byte buffer = 0;
            for (int i = 0; i < SheetData.Length; i += 4)
            {
                buffer = SheetData[i + 0];
                SheetData[i + 0] = SheetData[i + 2];
                SheetData[i + 2] = buffer;
            }
        }

        public static byte[] FlipColorsNew(byte[] array)
        {
            byte[] newArray = new byte[array.Length];
            Array.Copy(array, newArray, array.Length);
            for (int i = 0; i < array.Length; i += 4)
            {
                newArray[i + 0] = array[i + 2];
                newArray[i + 2] = array[i + 0];
            }
            return newArray;
        }

        public int CheckPCKSig(ExtendedBinaryReader reader, string expected)
        {
            // Read and check the signature
            string sig = reader.ReadSignature(expected.Length);
            if (sig != expected)
                return 0;
            // Calculate Size of Padding
            int padding = 0;
            while (true)
            {
                if (reader.ReadByte() != 0x20)
                {
                    if (expected.Length + padding >= 0x14)
                        reader.JumpBehind((expected.Length + padding) - 0x14 + 1);
                    else if (expected.Length + padding >= 0x08)
                        reader.JumpBehind((expected.Length + padding) - 0x08 + 1);
                    break;
                }
                ++padding;
            }
            // Total length of the signature
            return expected.Length + padding;
        }

        public void WritePCKSig(ExtendedBinaryWriter writer, string sig, bool smallSig)
        {
            writer.WriteSignature(sig + new string(' ', (smallSig ? 0x08 : 0x14) - sig.Length));
        }

        public void Dispose()
        {
            Frames.Clear();
            Frames = null;
            SheetData = null;
        }
    }
}
