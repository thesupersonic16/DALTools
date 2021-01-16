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
        /// <summary>
        /// An internal reader for streaming files from the archive
        /// </summary>
        protected ExtendedBinaryReader _internalReader;

        // Options
        /// <summary>
        /// Toggle for using larger signatures (0x14) or smaller ones (0x08)
        /// DAL: RR uses larger signatures
        /// </summary>
        public bool UseSmallSig = false;

        public bool Compress = false;

        public List<FileEntry> FileEntries = new List<FileEntry>();

        public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            // Store the current reader
            _internalReader = reader;

            // Filename Section
            //  This section contains an array of addresses to each of the file's name and the strings
            //   itself right after, this section is only used for finding file indices from within game

            //  This is a workaround for reading different versions of PCK files as some files seem to
            //   have smaller padding (0x08 for Signatures, 0x04 for Padding)
            //   while DAL: RR has larger padding (0x14 for Signatures, 0x08 for Padding)
            //  This workaround works by checking the padding in the signature to determine the version
            int sigSize = reader.CheckDALSignature("Filename");
            if (sigSize < 0x14)
                UseSmallSig = true;

            // The length of the Filename section
            int fileNameSectionSize = reader.ReadInt32();
            // Address to the list of filenames
            int fileNameSectionAddress = (int)reader.BaseStream.Position;
            
            // Jump to the Pack section
            reader.JumpTo(fileNameSectionSize);

            // Makes sure the reader is aligned
            reader.FixPadding(UseSmallSig ? 0x04u : 0x08u);

            // Pack Section
            //  This section contains an array of file information and then all of it's data
            
            // Check Signature
            string packSig = reader.ReadDALSignature("Pack");
            if (packSig != "Pack" && packSig.Length <= 4)
                throw new SignatureMismatchException("Pack", packSig);

            // The length of the Pack section
            int packSectionSize = reader.ReadInt32();
            int fileCount = reader.ReadInt32();
            
            // Read file entries
            for (int i = 0; i < fileCount; ++i)
            {
                FileEntries.Add(new FileEntry());
                FileEntries[i].DataPosition = reader.ReadInt32();
                FileEntries[i].DataLength   = reader.ReadInt32();
            }

            // Jump back to the Filename section so we can get all of the file names
            reader.JumpTo(fileNameSectionAddress);
            
            // Reads all the file names
            for (int i = 0; i < fileCount; ++i)
            {
                int position = reader.ReadInt32() + (UseSmallSig ? 0xC : 0x18);
                FileEntries[i].FileName = reader.ReadStringElsewhere(position);
            }

            // Load all data into memory if the loader plans to close the stream
            if (!keepOpen)
                Preload();
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
            // Loads all files into memory if not already
            //  This is needed to ensure the reading stream is closed
            Preload();

            // ZLIB Compression
            Stream mainStream = null;
            if (Compress)
                mainStream = writer.StartDeflateEncapsulation();

            // Filename Section
            // Address of the Filename section
            long sectionPosition = writer.BaseStream.Position;
            writer.WriteDALSignature("Filename", UseSmallSig);
            writer.AddOffset("SectionSize");
            
            // Allocates space for all the file name pointers
            foreach (var entry in FileEntries)
                writer.AddOffset(entry.FileName);
            
            // Fills in all the file names
            foreach (var entry in FileEntries)
            {
                // Fill in the address and write the file name (including paths)
                writer.FillInOffset(entry.FileName, (uint)(writer.BaseStream.Position - (UseSmallSig ? 0xC : 0x18)));
                writer.WriteNullTerminatedString(entry.FileName);
            }
            // Fills in the size of the Filename section
            writer.FillInOffset("SectionSize");
            // Realigns the writer 
            writer.FixPadding(UseSmallSig ? 0x04u : 0x08u);

            // Pack Section
            // Address to the Pack section
            sectionPosition = writer.BaseStream.Position;
            writer.WriteDALSignature("Pack", UseSmallSig);
            writer.AddOffset("SectionSize");
            writer.Write(FileEntries.Count);

            // Writes file data entries
            for (int i = 0; i < FileEntries.Count; ++i)
            {
                // Allocates 4 bytes for the absolute address of the contents of the file
                writer.AddOffset($"DataPtr{i}");
                // Writes the length of the file
                writer.Write(FileEntries[i].Data.Length);
            }

            // Fills in the size of the Pack section
            writer.FillInOffset("SectionSize", (uint)(writer.BaseStream.Position - (UseSmallSig ? 0xC : 0x18)));
            // Realigns the writer
            writer.FixPadding(UseSmallSig ? 0x04u : 0x08u);

            // Data
            for (int i = 0; i < FileEntries.Count; ++i)
            {
                writer.FillInOffset($"DataPtr{i}");
                writer.Write(FileEntries[i].Data);
                // Realigns the writer 
                writer.FixPadding(UseSmallSig ? 0x04u : 0x08u);
            }

            // Finalise ZLIB Compression
            if (Compress)
                writer.EndDeflateEncapsulation(mainStream);
        }

        /// <summary>
        /// <para>Loads all files into memory then closes the stream</para>
        /// <para>This should be used if you would like to keep all the data in memory</para>
        /// </summary>
        public void Preload()
        {
            foreach (var entry in FileEntries)
            {
                // Check if file is already loaded into memory
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
        /// Reads the found file into memory
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns>a byte array containing the file</returns>
        public byte[] GetFileData(int index)
        {
            // Get File Entry
            var entry = FileEntries[index];
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
        /// Sets the position of the main stream to the found file
        /// </summary>
        /// <param name="index">File index</param>
        /// <returns>Stream to the file</returns>
        public Stream GetFileStream(int index)
        {
            // Get File Entry
            var entry = FileEntries[index];
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
                foreach (string filePath in Directory.EnumerateFiles(dirPath, "*")
                    .Select(x => x.Substring(path.Length + 1).Replace('\\', '/')).OrderBy(x => char.IsUpper(x[0]) ? -1 : 1))
                {
                    FileEntry entry = new FileEntry
                    {
                        FileName = filePath,
                        Data = System.IO.File.ReadAllBytes(Path.Combine(path, filePath))
                    };
                    entry.DataLength = entry.Data.Length;
                    FileEntries.Add(entry);
                }
            }
            SortFileList();
        }

        // TODO: Figure out how sorting works. 
        public void SortFileList()
        {
            if (FileEntries.Any(t => t.FileName == "face.mpb"))
            {
                List<string> order = new List<string>() { ".mpb", ".tex", "layername.bin", "screen.txt", ".exl", ".uca.bin", "Config.txt", ".amb", "" };
                FileEntries = FileEntries.OrderBy(t => order.FindIndex(tt => t.FileName.EndsWith(tt))).ToList();
            }
            if (FileEntries.Any(t => t.FileName.EndsWith(".MA")))
            {
                List<string> order = new List<string>() { ".MA", "" };
                FileEntries = FileEntries.OrderBy(t => order.FindIndex(tt => t.FileName.EndsWith(tt))).ToList();
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
