using DALLib.Compression;
using DALLib.Exceptions;
using DALLib.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.File
{
    public class PCKFile : FileBase, IDisposable
    {
        protected ExtendedBinaryReader _internalReader;

        // Options
        /// <summary>
        /// Toggle for using larger signatures (0x14) or smaller ones (0x08)
        /// DAL: RR uses larger signatures
        /// </summary>
        public bool UseSmallSig = false;

        public bool Compress = false;

        public List<FileEntry> FileEntries = new List<FileEntry>();

        public override void Load(ExtendedBinaryReader reader)
        {
            // Store the current reader so we can stream files
            _internalReader = reader;

            // Filename Section
            //  This section contains an array of addresses to each of the file's name and the strings itself
            //   this section is only used for finding file indices from within game
            //
            //  Workaround for reading different versions of PCK files
            //  Older PCK files seem to have smaller padding (0x08 for Signatures, 0x04 for Padding)
            //  While DAL: RR has larger padding (0x14 for Signatures, 0x08 for Padding)
            //  This workaround works by checking the padding in the signature to determine the version
            int sigSize = reader.CheckDALSignature("Filename");
            bool oldPCK = false;
            if (sigSize < 0x14)
                oldPCK = true;

            // The length of the Filename section
            int filenameSectionSize = reader.ReadInt32();

            // The address to the Filename section
            int fileNameSectionAddress = (int)reader.BaseStream.Position;
            // Jump to the next section, which should be Pack
            reader.JumpTo(filenameSectionSize);

            // Pack Section
            //  This section contains an array of all the file's data
            reader.FixPadding(oldPCK ? 0x04u : 0x08u);
            // Check Signature
            string packSig = reader.ReadDALSignature("Pack");
            // Check if signature is valid
            if (packSig != "Pack" && packSig.Length <= 4)
                throw new SignatureMismatchException("Pack", packSig);

            // The length of the Pack section
            int packSectionSize = reader.ReadInt32();
            // The amount of files that is currently stored in the archive
            int fileCount = reader.ReadInt32();
            // Read file entries
            for (int i = 0; i < fileCount; ++i)
            {
                FileEntries.Add(new FileEntry());
                FileEntries[i].DataPosition = reader.ReadInt32();
                FileEntries[i].DataLength = reader.ReadInt32();
            }

            // Jump back to the Filename section so we can name all the files
            reader.JumpTo(fileNameSectionAddress);
            // Reads all the file names
            for (int i = 0; i < fileCount; ++i)
            {
                int position = reader.ReadInt32() + (oldPCK ? 0xC : 0x18);
                FileEntries[i].FileName = reader.ReadStringElsewhere(position);
            }
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
            // Preload all files, This is needed as we can not edit archives while writing streams safely yet
            Preload();

            // ZLIB Compression
            Stream mainStream = null;
            if (Compress)
                mainStream = writer.StartDeflateEncapsulation();

            // Filename Section
            // Address to the start of the Filename section, This is used as a base address
            long sectionPosition = writer.BaseStream.Position;
            // Writes the signature for the section
            writer.WriteDALSignature("Filename", UseSmallSig);
            // Allocates 4 bytes for the section size
            writer.AddOffset("SectionSize");
            // Allocates space for all the file name pointers
            foreach (var entry in FileEntries)
                writer.AddOffset(entry.FileName);
            // Fills in all the file names
            foreach (var entry in FileEntries)
            {
                // Writes an absolute address to the array of file name pointers
                writer.FillInOffset(entry.FileName, (uint)(writer.BaseStream.Position - sectionPosition));
                // Writes the file name and adds a null byte to terminate the string
                writer.WriteNullTerminatedString(entry.FileName);
            }
            // Finalises the Filename section by writing the length of the section
            writer.FillInOffset("SectionSize");
            // Pads the file to a divisible of 0x08 for DAL: RR or 0x04 for others 
            writer.FixPadding(UseSmallSig ? 0x04u : 0x08u);

            // Pack Section
            // Address to the start of the Pack section, This is used as a base address
            sectionPosition = writer.BaseStream.Position;
            // Writes the signature for the section
            writer.WriteDALSignature("Pack", UseSmallSig);
            // Allocates 4 bytes for the section size
            writer.AddOffset("SectionSize");
            // Writes the file count
            writer.Write(FileEntries.Count);

            // Writes file data entries
            for (int i = 0; i < FileEntries.Count; ++i)
            {
                // Allocates 4 bytes for the absolute address of the contents of the file
                writer.AddOffset($"DataPtr{i}");
                // Writes the length of the file
                writer.Write(FileEntries[i].Data.Length);
            }

            // Finalises the Pack section by writing the length of the section
            writer.FillInOffset("SectionSize", (uint)(writer.BaseStream.Position - sectionPosition));
            // Pads the file to a divisible of 0x08 for DAL: RR or 0x04 for others 
            writer.FixPadding(UseSmallSig ? 0x04u : 0x08u);

            // Data
            for (int i = 0; i < FileEntries.Count; ++i)
            {
                // Writes the absolute address of where we are currently at which will contain the file contents
                writer.FillInOffset($"DataPtr{i}");
                // Write the file contents
                writer.Write(FileEntries[i].Data);
                // Pads the file to a divisible of 0x08 for DAL: RR or 0x04 for others 
                writer.FixPadding(UseSmallSig ? 0x04u : 0x08u);
            }

            // Finalise ZLIB Compression
            if (Compress)
                writer.EndDeflateEncapsulation(mainStream);
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
                _internalReader.JumpTo(entry.DataPosition);
                entry.Data = _internalReader.ReadBytes(entry.DataLength);
            }
            _internalReader?.Dispose();
            _internalReader = null;
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
            _internalReader.JumpTo(entry.DataPosition);
            return _internalReader.ReadBytes(entry.DataLength);
        }

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
            // Return a memory stream if the reader is closed, assuming all files are loaded into memory
            if (_internalReader == null)
                return new MemoryStream(entry.Data);
            _internalReader.JumpTo(entry.DataPosition);
            return new VirtualStream(_internalReader.BaseStream, entry.DataPosition, entry.DataLength, true);
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
                _internalReader.JumpTo(entry.DataPosition);
                // Create Directories
                Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(path, entry.FileName)));
                // Write Data
                System.IO.File.WriteAllBytes(Path.Combine(path, entry.FileName),
                    _internalReader.ReadBytes(entry.DataLength));
            }
        }

        /// <summary>
        /// Enumerates all files from the directory pointed by path and adds them to the archive
        /// <para/>
        /// Note: This function is recursive and also includes files from the root directory
        /// </summary>
        /// <param name="path">The Directory to add files from</param>
        public void AddAllFiles(string path)
        {
            foreach (string dirPath in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories).Concat(new [] { path }))
            {
                foreach (string filePath in Directory.EnumerateFiles(dirPath, "*"))
                {
                    FileEntry entry = new FileEntry
                    {
                        // Set and Remove starting folder name
                        FileName = filePath.Substring(path.Length + 1).Replace('\\', '/'),
                        Data = System.IO.File.ReadAllBytes(filePath),
                    };
                    entry.DataLength = entry.Data.Length;
                    FileEntries.Add(entry);
                }
            }
        }

        /// <summary>
        /// Adds a new file to the archive
        /// </summary>
        /// <param name="filename">Name of the file including its path</param>
        /// <param name="data">Array of bytes in which the file contains</param>
        public void AddFile(string filename, byte[] data)
        {
            FileEntry entry = new FileEntry
            {
                FileName = filename,
                Data = data,
                DataLength = data.Length
            };
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
            if (_internalReader != null)
                _internalReader.Dispose();
            FileEntries.Clear();
        }

        public class FileEntry
        {
            public string FileName  { get; set; }
            public int DataPosition { get; set; }
            public int DataLength   { get; set; }
            public byte[] Data      { get; set; }

            public override string ToString()
            {
                return FileName;
            }
        }


    }
}
