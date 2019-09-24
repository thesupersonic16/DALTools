using HedgeLib.Exceptions;
using HedgeLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCKTool
{
    public class PCKStreamArchive : IDisposable
    {

        public List<FileEntry> FileEntries = new List<FileEntry>();

        protected ExtendedBinaryReader _reader;

        public void Load(string filePath)
        {
            var stream = File.OpenRead(filePath);
            Load(stream);
        }

        public void Load(Stream fileStream)
        {
            var reader = new ExtendedBinaryReader(fileStream, Encoding.UTF8);
            _reader = reader;
            int sigSize = CheckPCKSig(reader, "Filename");
            bool oldPCK = false;
            if (sigSize < 0x14)
                oldPCK = true;
            if (oldPCK)
                reader.JumpTo(0x08);
            int packAddress = reader.ReadInt32();
            int fileNameAddress = (int)reader.BaseStream.Position;

            reader.JumpTo(packAddress);

            // Pack
            reader.FixPadding(oldPCK ? 0x04u : 0x08u);
            sigSize = CheckPCKSig(reader, "Pack");

            int headerSize = reader.ReadInt32();
            int fileCount = reader.ReadInt32();

            for (int i = 0; i < fileCount; ++i)
            {
                FileEntries.Add(new FileEntry());
                FileEntries[i].dataPostion = reader.ReadInt32();
                FileEntries[i].dataLength = reader.ReadInt32();
            }

            reader.JumpTo(fileNameAddress);

            for (int i = 0; i < fileCount; ++i)
            {
                int position = reader.ReadInt32() + (oldPCK ? 0xC : 0x18);
                long oldPosition = reader.BaseStream.Position;
                reader.JumpTo(position);
                FileEntries[i].FileName = reader.ReadNullTerminatedString();
                reader.JumpTo(oldPosition);
            }

        }

        public byte[] GetFileData(string name)
        {
            // Get File Entry by filename
            var entry = FileEntries.FirstOrDefault(
                t => t.FileName.ToLowerInvariant() == name.ToLowerInvariant());
            if (entry == null)
                return null;
            _reader.JumpTo(entry.dataPostion);
            return _reader.ReadBytes(entry.dataLength);
        }

        /// <summary>
        /// Reads and Checks the Signature of the file.
        /// Messy as it supports multiple games.
        /// </summary>
        /// <param name="reader">StreamReader to read the signature from</param>
        /// <param name="expected">The expected signature</param>
        /// <returns>Length of the signature</returns>
        public int CheckPCKSig(ExtendedBinaryReader reader, string expected)
        {
            // Read and check the signature
            string sig = reader.ReadSignature(expected.Length);
            if (sig != expected)
                throw new InvalidSignatureException(expected, sig);
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

        public void Dispose()
        {
            if (_reader != null)
                _reader.Dispose();
        }

        public class FileEntry
        {
            public string FileName;
            public int dataPostion;
            public int dataLength;
        }


    }
}
