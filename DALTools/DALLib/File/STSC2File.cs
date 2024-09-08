using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using DALLib.Exceptions;
using DALLib.IO;
using DALLib.Scripting;

namespace DALLib.File
{
    public class STSC2File : FileBase
    {
        /// <summary>
        /// The version of the tool used to build the script
        /// Date a Live: Ren Dystopia was built using 2.0.7
        /// </summary>
        public string Version = "2.0.7";

		public uint FileSize = 0;

		/// <summary>
		/// STSC2 Header Bit Flags.
		/// 0000 0001 - ScriptName
		/// 0000 0010 - Datestamp
		/// 0000 0100 - ScriptID
		/// 0000 1000 - Compression
		/// 0001 0000 - Security
		/// 0010 0000 - Filenames
		/// 0100 0000 - CmdLineInfo
		/// 
		/// Most Date a Live: Ren Dystopia scripts use 0x6D = 01101101
		/// </summary>
		public uint Flags = 0x6D;

        /// <summary>
        /// Name of the script, this is the name of the file
        /// </summary>
        public string ScriptName { get; set; }

        /// <summary>
        /// Time when the script was built
        /// </summary>
        public DateTime ScriptTimestamp { get; set; }

        /// <summary>
        /// The script ID is used to identify the script
        /// </summary>
        public ushort ScriptID = 0;

        /// <summary>
        /// Compression Data. Currently only ZLib is supported by DALLib
        /// </summary>
        public STSC2Compression Compression { get; set; }

        public List<string> Filenames = new List<string>();

        /// <summary>
        /// Unknown
        /// </summary>
        public bool CmdLineInfo { get; set; }

        public STSC2Sequence Sequence { get; set; } = new STSC2Sequence();

		public static readonly byte[] SecurityShuffleKey;

		public STSC2Security Security { get; set; }

		private byte[] SecurityDecode(byte[] buffer, int size, int offset)
		{
			switch (Security.SecurityType)
			{
				case STSC2SecurityType.OriginalKeyLv1:
					return SecurityDecodeOriginalKey(buffer, offset, Security.OriginalSize, 1);
				case STSC2SecurityType.OriginalKeyLv2:
					return SecurityDecodeOriginalKey(buffer, offset, Security.OriginalSize, 2);
			}
			int num = (int)(size - (long)((ulong)FileSize));
			byte[] array = new byte[num];
			Array.Copy(buffer, (long)((ulong)offset), array, 0L, num);
			return array;
		}

		private byte[] SecurityDecodeOriginalKey(byte[] buffer, int offset, int size, int lv)
		{
			byte[] array = new byte[size];
			Array.Copy(buffer, offset, array, 0L, size);
            int j;
            if (lv >= 2)
			{
				j = size / 2;
				for (int i = 0; i < j; i++)
				{
					byte b = array[i];
					array[i] = array[size - (i + 1)];
					array[size - (i + 1)] = b;
				}
			}
			j = size;
			for (int i = 0; i < size; i++)
			{
				byte b2 = SecurityShuffleKey[j & 255];
				array[i] = (byte)(array[i] - SecurityShuffleKey[b2] ^ b2);
				j++;
			}
			return array;
		}

		public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            // Check if file has the STSC signature
            string sig = reader.ReadSignature();
            if (sig != "STSC")
                throw new SignatureMismatchException("STSC", sig);

            Version = reader.ReadTrimmedString(0x0C);
			FileSize = reader.ReadUInt32();

            // Flags
            Flags = reader.ReadUInt32();
            if (CheckFlag(STSC2Flags.ScriptName))
                ScriptName = reader.ReadTrimmedString(0x20);
            if (CheckFlag(STSC2Flags.Datestamp))
                ScriptTimestamp = new DateTime(
                    reader.ReadUInt16(),  // Year
                    reader.ReadUInt16(),  // Month
                    reader.ReadUInt16(),  // Day
                    reader.ReadUInt16(),  // Hour
                    reader.ReadUInt16(),  // Minute
                    reader.ReadUInt16(),  // Second
                    reader.ReadUInt16()); // Milisecond
            if (CheckFlag(STSC2Flags.ScriptID))
                ScriptID = reader.ReadUInt16();
            if (CheckFlag(STSC2Flags.Compression))
            {
                Compression = reader.ReadStruct<STSC2Compression>();
                reader.JumpAhead(0x34); // Not sure what is meant to be here
            }
			if (CheckFlag(STSC2Flags.Security))
			{
				Security = reader.ReadStruct<STSC2Security>();
				reader.JumpAhead(0x30); // Not sure what is meant to be here
				reader.JumpAhead(0x32);
			}
			if (CheckFlag(STSC2Flags.Filenames))
            {
                int count = reader.ReadUInt16();
                for (int i = 0; i < count; i++)
                    Filenames.Add(reader.ReadNullTerminatedString());

            }
            CmdLineInfo = CheckFlag(STSC2Flags.CmdLineInfo);

            Stream scriptStream = null;
            if (Compression.CompressionType == STSC2CompressionType.Zlib)
            {
                var _data = reader.ReadBytes((int)Compression.CompressedSize);

				if (CheckFlag(STSC2Flags.Security))
					_data = SecurityDecode(_data, _data.Length, 0);

                // Decompress stream
                using (var deflate = new DeflateStream(new MemoryStream(_data, 2, _data.Length - 2), CompressionMode.Decompress, false))
                {
                    var memoryStream = deflate.CacheStream();
                    
                    // Test
                    //System.IO.File.WriteAllBytes("out.bin", memoryStream.ToArray());
                    //memoryStream.Position = 0;
                    
                    scriptStream = new VirtualStream(memoryStream, keepOpen);
                }
            }
            else
            {
                // Read to EOF
                scriptStream = new VirtualStream(reader.BaseStream, reader.GetPosition(), reader.GetLength() - reader.GetPosition(), keepOpen);
            }

            Sequence.ReadSequence(new ExtendedBinaryReader(scriptStream.CacheStream()), CmdLineInfo);
        }


        public override void Save(ExtendedBinaryWriter writer)
        {
            var sequence = new MemoryStream();
            Sequence.WriteSequence(new ExtendedBinaryWriter(sequence), CmdLineInfo);
            byte[] bytes = sequence.ToArray();

            writer.WriteSignature("STSC");
			writer.WriteNullTerminatedString(Version);
			writer.FixPadding(0x10);

			writer.AddOffset("FileSize");
			writer.Write(Flags);
            if (CheckFlag(STSC2Flags.ScriptName))
			{
				writer.WriteSignature(ScriptName);
				writer.WriteNulls((uint)(0x20 - ScriptName.Length));
			}
            if (CheckFlag(STSC2Flags.Datestamp))
			{
				writer.Write((ushort)ScriptTimestamp.Year);
                writer.Write((ushort)ScriptTimestamp.Month);
                writer.Write((ushort)ScriptTimestamp.Day);
                writer.Write((ushort)ScriptTimestamp.Hour);
                writer.Write((ushort)ScriptTimestamp.Minute);
                writer.Write((ushort)ScriptTimestamp.Second);
                writer.Write((ushort)ScriptTimestamp.Millisecond);
            }
            if (CheckFlag(STSC2Flags.ScriptID))
				writer.Write(ScriptID);
            if (CheckFlag(STSC2Flags.Compression))
            {
				//var compressed = new MemoryStream();
				//new MemoryStream(bytes).DeflateCompress(compressed);

				writer.Write((uint)STSC2CompressionType.None);
				writer.Write(bytes.Length);
                //bytes = compressed.ToArray();
                writer.Write(bytes.Length);
                writer.WriteNulls(0x34); // Not sure what is meant to be here
            }
            if (CheckFlag(STSC2Flags.Filenames))
            {
				writer.Write((ushort)Filenames.Count);
				foreach (string filename in Filenames)
					writer.WriteNullTerminatedString(filename);
            }

            writer.FillInOffset("FileSize");
            // Write ZLIB flags
            //writer.Write((byte)0x78);
            //writer.Write((byte)0xDA);

            // Test
            //System.IO.File.WriteAllBytes("out.bin", bytes);

            writer.Write(bytes);
			writer.FixPadding(0x10);
        }

        public override string ToString()
        {
            return ScriptName;
        }

        protected bool CheckFlag(STSC2Flags bit)
        {
            return (Flags & (uint)bit) != 0;
        }

        public enum STSC2Flags
        {
            ScriptName   = 0b0000_0001,
            Datestamp    = 0b0000_0010,
            ScriptID     = 0b0000_0100,
            Compression  = 0b0000_1000,
            Security     = 0b0001_0000,
            Filenames    = 0b0010_0000,
            CmdLineInfo  = 0b0100_0000
        }
        public enum STSC2CompressionType
        {
            None,
            Zlib,
            Snappy,
            Lz4
        }

		public enum STSC2SecurityType
		{
			None,
			Original,
			OriginalKeyLv1,
			OriginalKeyLv2
		}

		public struct STSC2Compression
        {
            public STSC2CompressionType CompressionType { get; set; }
            public int OriginalSize { get; set; }
            public int CompressedSize { get; set; }
        }

		public struct STSC2Security
		{
			public STSC2SecurityType SecurityType { get; set; }

			public int OriginalSize { get; set; }
			public int EncodedSize { get; set; }
			public uint Verify { get; set; }
		}
	}
}
