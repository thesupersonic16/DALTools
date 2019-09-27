using DALLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zlib;

namespace DALLib.File
{
    public class PCKFile : FileBase, IDisposable
    {
        protected ExtendedBinaryReader _reader;

        // Options
        /// <summary>
        /// Toggle for using larger signatures (0x14) or smaller ones (0x08)
        /// DAL: RR uses larger signatures
        /// </summary>
        public bool UseSmallSig = false;

        public List<FileEntry> FileEntries = new List<FileEntry>();

        public override void Load(ExtendedBinaryReader reader)
        {
            // Decompress Zlib stream
            if (reader.PeekSignature() == "ZLIB")
            {
                // Skip Zlib Header
                // 0x00 - "ZLIB"
                // 0x04 - UncompressedSize
                // 0x08 - CompressedSize
                // 0x0C - Zlib Data
                reader.JumpAhead(12);
                // Set stream to ZLIB
                reader.SetStream(new ZOutputStream(reader.BaseStream));
            }

            _reader = reader;
            int sigSize = reader.CheckDALSignature("Filename");
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
            sigSize = reader.CheckDALSignature("Pack");

            int headerSize = reader.ReadInt32();
            int fileCount = reader.ReadInt32();

            for (int i = 0; i < fileCount; ++i)
            {
                FileEntries.Add(new FileEntry());
                FileEntries[i].DataPosition = reader.ReadInt32();
                FileEntries[i].DataLength = reader.ReadInt32();
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

        public override void Save(ExtendedBinaryWriter writer)
        {
            // Preload all files, This is needed as we can not edit archives while writing streams safely yet
            Preload();

            writer.WriteDALSignature("Filename", UseSmallSig);
            writer.AddOffset("HeaderSize");
            foreach (var entry in FileEntries)
                writer.AddOffset(entry.FileName);
            foreach (var entry in FileEntries)
            {
                // TODO: Check if - 0x18 is correct for other .pck(s)
                writer.FillInOffset(entry.FileName, (uint)writer.BaseStream.Position - 0x18);
                writer.WriteNullTerminatedString(entry.FileName);
            }

            // Pack
            writer.FillInOffset("HeaderSize");
            writer.FixPadding(0x8);
            long headerPosition = writer.BaseStream.Position;
            writer.WriteDALSignature("Pack", UseSmallSig);
            writer.AddOffset("HeaderSize");
            writer.Write(FileEntries.Count);

            // File sizes
            foreach (var entry in FileEntries)
            {
                writer.AddOffset(entry.FileName);
                writer.Write(entry.Data.Length);
            }

            writer.FillInOffset("HeaderSize", (uint)(writer.BaseStream.Position - headerPosition));
            writer.FixPadding(0x8);

            // Data
            foreach (var entry in FileEntries)
            {
                writer.FillInOffset(entry.FileName);
                writer.Write(entry.Data);
                writer.FixPadding(0x8);
            }
        }

        /// <summary>
        /// Preloads all files into memory
        /// </summary>
        public void Preload()
        {
            foreach (var entry in FileEntries)
            {
                // Check if file is already preloaded
                if (entry.Data != null)
                    continue;
                _reader.JumpTo(entry.DataPosition);
                entry.Data = _reader.ReadBytes(entry.DataLength);
            }
        }

        /// <summary>
        /// Reads the found file into memory
        /// </summary>
        /// <param name="name">Name of the file</param>
        /// <returns>a byte array containing the file</returns>
        public byte[] GetFileData(string name)
        {
            // Get File Entry by filename
            var entry = FileEntries.FirstOrDefault(
                t => t.FileName.ToLowerInvariant() == name.ToLowerInvariant());
            // Check if file is found
            if (entry == null)
                return null;
            // Check if preloaded
            if (entry.Data != null)
                return entry.Data;
            _reader.JumpTo(entry.DataPosition);
            return _reader.ReadBytes(entry.DataLength);
        }

        // TODO: add Virtual Stream
        /// <summary>
        /// Sets the position of the main stream to the found file
        /// </summary>
        /// <param name="name">Name of the file</param>
        /// <returns>Stream to the file or Null if not found</returns>
        public Stream GetFileStream(string name)
        {
            // Get File Entry by filename
            var entry = FileEntries.FirstOrDefault(
                t => t.FileName.ToLowerInvariant() == name.ToLowerInvariant());
            if (entry == null)
                return null;
            _reader.JumpTo(entry.DataPosition);
            return _reader.BaseStream;
        }

        /// <summary>
        /// Extracts all files onto disk
        /// </summary>
        /// <param name="path">Path to write all the files to</param>
        public void ExtractAllFiles(string path)
        {
            foreach (var entry in FileEntries)
            {
                // Set Location
                _reader.JumpTo(entry.DataPosition);
                // Create Directories
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(path, entry.FileName)));
                // Write Data
                System.IO.File.WriteAllBytes(Path.Combine(path, entry.FileName),
                    _reader.ReadBytes(entry.DataLength));
            }
        }

        public void AddAllFiles(string path)
        {
            foreach (string filePath in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
            {
                FileEntry entry = new FileEntry();
                // Set and Remove starting folder name
                entry.FileName = filePath.Substring(path.Length + 1);
                entry.Data = System.IO.File.ReadAllBytes(filePath);
                entry.DataLength = entry.Data.Length;
                FileEntries.Add(entry);
            }
        }

        /// <summary>
        /// Adds a new file to the archive
        /// </summary>
        /// <param name="filename">Name of the file including its path</param>
        /// <param name="data">Array of bytes in which the file contains</param>
        public void AddFile(string filename, byte[] data)
        {
            FileEntry entry = new FileEntry();
            entry.FileName = filename;
            entry.Data = data;
            entry.DataLength = data.Length;
            FileEntries.Add(entry);
        }

        /// <summary>
        /// Replaces a new file to the archive
        /// </summary>
        /// <param name="filename">Name of the file including its path</param>
        /// <param name="data">Array of bytes in which the file contains</param>
        public void ReplaceFile(string filename, byte[] data)
        {
            // Get File Entry by filename
            var entry = FileEntries.FirstOrDefault(
                t => t.FileName.ToLowerInvariant() == filename.ToLowerInvariant());
            // If file is not found, Create a new one
            if (entry == null)
            {
                AddFile(filename, data);
            }
            else
            {
                entry.Data = data;
                entry.DataLength = data.Length;
            }
        }

        /// <summary>
        /// Searches for a file containing the string "containsString"
        /// </summary>
        /// <param name="containsString">Pattern</param>
        /// <returns>Path to the found file, or null if not found</returns>
        public string SearchForFile(string containsString)
        {
            return FileEntries.FirstOrDefault(t =>
            t.FileName.ToLowerInvariant().Contains(containsString.ToLowerInvariant()))?.FileName;
        }

        public void Dispose()
        {
            if (_reader != null)
                _reader.Dispose();
            FileEntries.Clear();
        }

        public class FileEntry
        {
            public string FileName;
            public int DataPosition;
            public int DataLength;
            public byte[] Data;
        }


    }
}
