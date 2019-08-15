using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeLib.Archives;
using HedgeLib.Exceptions;
using HedgeLib.IO;
using zlib;

namespace PCKTool
{
    public class PCKArchive : Archive
    {

        public List<FileEntry> FileEntries = new List<FileEntry>();

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
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
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
                reader.JumpAhead(12);
                reader = new ExtendedBinaryReader(new MemoryStream(DecompressData(reader.BaseStream)));
            }
            string sig = ReadPCKSig(reader);
            bool oldPCK = false;
            if (sig == "FilenameH")
                oldPCK = true;
            else if (sig != "Filename")
                throw new InvalidSignatureException("Filename", sig);
            if (oldPCK)
                reader.JumpTo(0x08);
            int packAddress = reader.ReadInt32();
            int fileNameAddress = (int)reader.BaseStream.Position;

            reader.JumpTo(packAddress);

            // Pack
            reader.FixPadding(0x8);
            sig = ReadPCKSig(reader, oldPCK ? 0x08 : 0x14);
            if (sig != "Pack")
                throw new InvalidSignatureException("Pack", sig);

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

            for (int i = 0; i < fileCount; ++i)
            {
                reader.JumpTo(FileEntries[i].dataPostion);
                Data.Add(new ArchiveFile()
                {
                    Name = FileEntries[i].FileName,
                    Data = reader.ReadBytes(FileEntries[i].dataLength)
                });
            }
            FixDirectories();

        }

        public override void Save(Stream fileStream)
        {
            var writer = new ExtendedBinaryWriter(fileStream);

            WritePCKSig(writer, "Filename");
            writer.AddOffset("HeaderSize");
            UnfixDirectories(null);
            var files = GetFiles(Data);
            for (int i = 0; i < files.Count; ++i)
                writer.AddOffset(files[i].Name);
            for (int i = 0; i < files.Count; ++i)
            {
                writer.FillInOffset(files[i].Name, (uint)writer.BaseStream.Position - 0x18);
                writer.WriteNullTerminatedString(files[i].Name);
            }
            // Pack
            writer.FillInOffset("HeaderSize");
            writer.FixPadding(0x8);
            long header = writer.BaseStream.Position;
            WritePCKSig(writer, "Pack");
            writer.AddOffset("HeaderSize");
            writer.Write(files.Count);

            foreach (var file in files)
            {
                writer.AddOffset(file.Name);
                writer.Write(file.Data.Length);
            }
            writer.FillInOffset("HeaderSize", (uint)(writer.BaseStream.Position - header));
            writer.FixPadding(0x8);

            // Data
            foreach (var file in files)
            {
                writer.FillInOffset(file.Name);
                writer.Write(file.Data);
                writer.FixPadding(0x8);
            }
        }

        public string ReadPCKSig(ExtendedBinaryReader reader, int size = 0x14)
        {
            string s = Encoding.ASCII.GetString(reader.ReadBytes(size));
            int trim = 0;
            if (s.Contains(' '))
                trim = s.IndexOf(" ", StringComparison.Ordinal);
            if (s.Contains('\0'))
                trim = s.IndexOf("\0", StringComparison.Ordinal);
            return s.Substring(0, trim);
        }

        public void WritePCKSig(ExtendedBinaryWriter writer, string sig)
        {
            writer.WriteSignature(sig + new string(' ', 0x14 - sig.Length));
        }

        public void FixDirectories()
        {
            for (int i = 0; i < Data.Count; ++i)
            {
                if (Data[i] is ArchiveDirectory)
                    continue;
                var file = Data[i] as ArchiveFile;
                if (file?.Name.IndexOf("/", StringComparison.Ordinal) != -1)
                {
                    string dir = file?.Name.Substring(0, file.Name.IndexOf('/'));
                    file.Name = file?.Name.Substring(file.Name.IndexOf('/'));
                    CreateDirectories(dir).Data.Add(file);
                    Data.RemoveAt(i--);
                }
            }
        }

        public void SortDirectories(ArchiveDirectory dir)
        {
            var data = dir != null ? dir.Data : Data;
            for (int i = 0; i < data.Count; ++i)
            {
                if (data[i] is ArchiveDirectory subDir)
                    SortDirectories(subDir);
                data.Sort((x, y) =>
                {
                    if (x is ArchiveDirectory && y is ArchiveDirectory)
                        return String.CompareOrdinal(x.Name, y.Name);
                    if (x is ArchiveDirectory)
                        return 0;
                    if (y is ArchiveDirectory)
                        return 1;
                    return string.Compare(x.Name, y.Name);
                });
            }
        }

        public void UnfixDirectories(ArchiveDirectory directory)
        {
            SortDirectories(null);
            List<ArchiveData> files = Data;
            if (directory != null)
                files = directory.Data;
            for (int i = 0; i < files.Count; ++i)
            {
                var data = files[i];
                if (data is ArchiveDirectory dir)
                    UnfixDirectories(dir);
                if (data is ArchiveFile file)
                {
                    if (directory != null)
                    {
                        file.Name = directory.Name + "/" + file.Name;
                        directory.Data.RemoveAt(i--);
                        if (directory.Parent != null)
                            directory.Parent.Data.Add(file);
                        else
                            Data.Add(file);
                    }
                }
            }
        }




        public class FileEntry
        {
            public string FileName;
            public int dataPostion;
            public int dataLength;
        }
    }
}
