﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DALLib.Compression;
using DALLib.Imaging;
using DALLib.IO;
using Scarlet.Drawing;
using Scarlet.IO;

namespace DALLib.File
{
    [Serializable]
    public class TEXFile : FileBase, IDisposable
    {

        /// <summary>
        /// List of formats decribing how pixel data is stored
        /// </summary>
        public enum Format
        {
            None       = 0x0000_0000,
            DXT1       = 0x0000_0001,
            DXT5       = 0x0000_0002,
            Luminance8 = 0x0000_0080,
            Luminance4 = 0x0000_0100,
            Raster     = 0x0000_0200,
            DXT12      = 0x0000_1000, // Seems to just be DXT1
            Large      = 0x0000_2000,
            BGRA       = 0x0000_4000,
            PNG        = 0x0001_0000  // TODO: Find out which file uses this
        }

        /// <summary>
        /// Unknown. Used to flag loading PNGs
        /// </summary>
        public enum LoaderType
        {
            Default = 0x0000_0000,
            PNG     = 0x0000_0001,
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
        /// Toggle for using not including a signature, It is unknown what needs it or now
        /// Default is false (include signature) as most files needs it
        /// </summary>
        public bool Sigless = false;
        /// <summary>
        /// A flag for if the texture should be ZLIB compressed or not
        /// Default is false as DAL: RR does not support ZLIB compressed textures
        /// </summary>
        public bool Compress = false;

        /// <summary>
        /// Should the image data be encoded as a PNG file
        /// DAL: RR does not support PNG textures, however DAL: RD does and may require it
        /// </summary>
        public bool UsePNG = false;

        /// <summary>
        /// Loads and parses file from stream into memory
        /// </summary>
        /// <param name="reader">Reader of the file</param>
        public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            // Read Signature to guess the type of TEX
            int sigSize = reader.CheckDALSignature("Texture");
            Sigless = sigSize < 4;
            UseSmallSig = sigSize <= 8;

            int textureSectionSize = reader.ReadInt32();
            if (Sigless)
                reader.JumpBehind(11);
            uint buf = reader.ReadUInt32();
            Format format = (Format)(buf & 0xFFFFFF);
            LoaderType loader = LoaderType.Default;
            byte unknown = (byte)(buf >> 24);
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
                loader = (LoaderType)reader.ReadInt16();
                short version = reader.ReadInt16();
                dataLength = reader.ReadInt32();
                SheetWidth = reader.ReadInt16();
                SheetHeight = reader.ReadInt16();
            }

            string dataSig = reader.PeekSignature();
            bool useLZ77 = dataSig == "LZ77";

            // Read Image Data
            SheetData = reader.ReadBytes(dataLength);
            
            // Decompress LZ77 Image
            if (useLZ77)
                SheetData = SheetData.DecompressLZ77();

            if (loader == LoaderType.PNG)
                UsePNG = true;

            // Decompress/Process Image based on format
            TEXConverter.Decode(this, format, loader, reader);

            // Fix Alignment
            reader.FixPadding(UseSmallSig ? 0x04u : 0x08u);

            // Parts
            sigSize = reader.CheckDALSignature("Parts");
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
            reader.FixPadding(UseSmallSig ? 0x04u : 0x08u);
            sigSize = reader.CheckDALSignature("Anime");
            if (sigSize < 4)
                return;
            int animeSectionSize = reader.ReadInt32();
            
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
            // ZLIB Compression
            Stream mainStream = null;
            if (Compress)
                mainStream = writer.StartDeflateEncapsulation();

            writer.SetEndian(UseBigEndian);

            if (!Sigless)
            {
                writer.WriteDALSignature("Texture", UseSmallSig);
                writer.AddOffset("HeaderSize");
            }

            // Format
            var format = Format.BGRA;
            var loader = UsePNG ? LoaderType.PNG : LoaderType.Default;

            if (UseSmallSig)
            {
                // TODO: Check if this is correct. This works for the PSV version of DAL: RR
                writer.Write((int)format | (0x81 << 24));
                writer.AddOffset("DataLength");
                writer.Write(SheetWidth);
                writer.Write(SheetHeight);
            }
            else
            {
                writer.Write((int)format);
                writer.Write(0x8100000 | (uint)loader);
                writer.AddOffset("DataLength");
                writer.Write((short)SheetWidth);
                writer.Write((short)SheetHeight);
            }

            byte[] data = TEXConverter.Encode(this, format, loader);
            writer.Write(data);
            writer.FillInOffset("DataLength", (uint)data.Length);

            // Sigless files do not have Parts or Anime
            if (Sigless)
                return;
            
            // Parts
            writer.FillInOffset("HeaderSize");
            writer.FixPadding(UseSmallSig ? 0x04u : 0x08u);
            writer.WriteDALSignature("Parts", UseSmallSig);
            long header = writer.BaseStream.Position;
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
            writer.FixPadding(UseSmallSig ? 0x04u : 0x08u);

            // Anime
            header = writer.BaseStream.Position;
            writer.WriteDALSignature("Anime", UseSmallSig);
            writer.AddOffset("HeaderSize");
            writer.WriteNulls(4);
            writer.FillInOffset("HeaderSize", (uint)(writer.BaseStream.Position - header));
            writer.FixPadding(0x10);

            // Finalise ZLIB Compression
            if (Compress)
                writer.EndDeflateEncapsulation(mainStream);
        }

        public void SaveSheetImage(string path)
        {
            ImageTools.SaveImage(path, SheetWidth, SheetHeight, SheetData);
        }

        public void LoadSheetImage(string path)
        {
            ImageTools.LoadImage(path, ref SheetWidth, ref SheetHeight, ref SheetData);
        }

        public void Dispose()
        {
            Frames.Clear();
            Frames = null;
            SheetData = null;
        }
    }
}
