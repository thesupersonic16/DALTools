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
using Scarlet.Drawing;
using Scarlet.IO;

namespace TEXTool
{
    [Serializable]
    public class TEXFile : FileBase
    {

        public enum Compression
        {
            None  = 0x00000000,
            DXT1  = 0x00000001,
            DXT5  = 0x00000002,
            Store = 0x00004000
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

        public short SheetWidth = 0;
        public short SheetHeight = 0;
        public List<Frame> Frames = new List<Frame>();
        [XmlIgnore] public byte[] SheetData = null;

        public override void Load(Stream fileStream)
        {
            var reader = new ExtendedBinaryReader(fileStream);
            bool hasHeader = ReadPCKSig(reader) == "Texture";
            int textureSectionSize = reader.ReadInt32();
            if (!hasHeader)
                reader.JumpTo(0);
            Compression compression = (Compression)reader.ReadInt32();
            int version = reader.ReadInt32();
            if (version != 0x8100000 && version != 0x1100000 && version != 0x2100000)
            {
                Console.WriteLine("Error: File Not Supported Yet! Expected: {0:X4} or {1:X4} or {2:X4} Got {3:X4}", 0x8100000, 0x1100000, 0x2100000, version);
                Console.ReadKey(true);
            }
            int dataLength = reader.ReadInt32();
            SheetWidth = reader.ReadInt16();
            SheetHeight = reader.ReadInt16();
            SheetData = reader.ReadBytes(dataLength);

            // Decompression
            ImageBinary image;
            switch (compression)
            {
                case Compression.DXT1:
                    image = new ImageBinary(SheetWidth, SheetHeight, PixelDataFormat.FormatDXT1Rgba,
                        Endian.LittleEndian, PixelDataFormat.FormatAbgr8888, Endian.LittleEndian, SheetData);
                    SheetData = image.GetOutputPixelData(0);
                    break;
                case Compression.DXT5:
                    image = new ImageBinary(SheetWidth, SheetHeight, PixelDataFormat.FormatDXT5,
                        Endian.LittleEndian, PixelDataFormat.FormatAbgr8888, Endian.LittleEndian, SheetData);
                    SheetData = image.GetOutputPixelData(0);
                    break;
                case Compression.Store:
                    break;
                default:
                    Console.WriteLine("Unknown Compression {0:X4}", compression);
                    Console.ReadKey(true);
                    break;
            }


            // Parts
            string sig = ReadPCKSig(reader);
            if (sig != "Parts")
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
            sig = ReadPCKSig(reader);
            if (sig != "Anime")
                throw new InvalidSignatureException("Anime", sig);
            int animeSectionSize = reader.ReadInt32();
            
        }

        // May not work, Only tested with title.tex
        public override void Save(Stream fileStream)
        {
            var writer = new ExtendedBinaryWriter(fileStream);
            
            WritePCKSig(writer, "Texture");
            writer.AddOffset("HeaderSize");
            writer.Write(0x4000);
            writer.Write(0x8100000);
            writer.AddOffset("DataLength");
            writer.Write(SheetWidth);
            writer.Write(SheetHeight);
            writer.Write(SheetData);
            writer.FillInOffset("DataLength", (uint)writer.BaseStream.Position - 0x28);
            
            // Parts
            writer.FillInOffset("HeaderSize");
            long header = writer.BaseStream.Position;
            WritePCKSig(writer, "Parts");
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
            WritePCKSig(writer, "Anime");
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
            byte[] buffer = new byte[4];
            for (int i = 0; i < SheetData.Length; i += 4)
            {
                buffer[0] = SheetData[i + 0];
                buffer[1] = SheetData[i + 1];
                buffer[2] = SheetData[i + 2];
                buffer[3] = SheetData[i + 3];
                SheetData[i + 0] = buffer[2];
                SheetData[i + 1] = buffer[1];
                SheetData[i + 2] = buffer[0];
                SheetData[i + 3] = buffer[3];
            }

        }

        public string ReadPCKSig(ExtendedBinaryReader reader)
        {
            try
            {
                string s = Encoding.ASCII.GetString(reader.ReadBytes(0x14));
                return s.Substring(0, s.IndexOf(" ", StringComparison.Ordinal));
            }
            catch
            {
                return "";
            }
        }

        public void WritePCKSig(ExtendedBinaryWriter writer, string sig)
        {
            writer.WriteSignature(sig + new string(' ', 0x14 - sig.Length));
        }
    }
}
