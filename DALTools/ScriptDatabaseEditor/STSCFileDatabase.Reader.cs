using HedgeLib.IO;
using STSCTool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDatabaseEditor
{
    // Reader class for STSCFileDatabase
    public partial class STSCFileDatabase
    {

        public override void Load(Stream fileStream)
        {
            // Read STSC
            base.Load(fileStream);

            // Used to read resources like strings, another reader is used for reading structs
            var fileReader = new ExtendedBinaryReader(fileStream, Encoding.UTF8);

            // System text
            using (var reader = CreateReader(0))
            {
                while (!EOF(reader))
                {
                    SystemText.Add(fileReader.ReadStringElsewhere(reader.ReadInt32(), false));
                }
            }

            // CGs
            using (var reader = CreateReader(1))
            {
                while (!EOF(reader))
                {
                    var entry = new CGEntry();
                    entry.Name          = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.ID            = reader.ReadInt32();
                    entry.CGID          = reader.ReadInt32();
                    entry.CGID2         = reader.ReadInt32();
                    entry.Unknown5      = reader.ReadUInt32();
                    entry.Unknown6      = reader.ReadUInt16();
                    entry.TextureWidth  = reader.ReadInt16();
                    entry.TextureHeight = reader.ReadInt16();
                    entry.Unknown7      = reader.ReadUInt16();
                    entry.Unknown81     = reader.ReadByte();
                    entry.Unknown82     = reader.ReadByte();
                    entry.Unknown83     = reader.ReadByte();
                    entry.Page          = reader.ReadByte();
                    entry.FrameCount    = reader.ReadByte();
                    entry.GameID        = (GameID)reader.ReadByte();
                    entry.Unknown93     = reader.ReadByte();
                    entry.Unknown94     = reader.ReadByte();
                    entry.Unknown10     = reader.ReadUInt32();
                    entry.Unknown11     = reader.ReadUInt32();
                    entry.Unknown12     = reader.ReadUInt32();
                    entry.Unknown13     = reader.ReadUInt32();
                    entry.Unknown14     = reader.ReadUInt32();
                    entry.Unknown15     = reader.ReadUInt32();
                    entry.Unknown16     = reader.ReadUInt32();
                    entry.Unknown17     = reader.ReadUInt32();
                    entry.Unknown18     = reader.ReadUInt32();
                    entry.Unknown19     = reader.ReadUInt32();
                    entry.Unknown20     = reader.ReadUInt32();
                    entry.Unknown21     = reader.ReadUInt32();
                    entry.Unknown22     = reader.ReadUInt32();
                    entry.Unknown23     = reader.ReadUInt32();
                    entry.Unknown24     = reader.ReadUInt32();
                    entry.Unknown25     = reader.ReadUInt32();
                    entry.Unknown26     = reader.ReadUInt32();
                    entry.Unknown27     = reader.ReadUInt32();
                    CGs.Add(entry);
                }
            }

            // Movies
            using (var reader = CreateReader(2))
            {
                while (!EOF(reader))
                {
                    var entry = new MovieEntry();
                    entry.FriendlyName  = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.FilePath      = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.ID            = reader.ReadInt32();
                    entry.Unknown4      = reader.ReadByte();
                    entry.GameID        = (GameID)reader.ReadByte();
                    entry.Unknown5      = reader.ReadInt16();
                    Movies.Add(entry);
                }
            }

            // Memories
            using (var reader = CreateReader(3))
            {
                while (!EOF(reader))
                {
                    var entry = new MemoryEntry();
                    entry.Name          = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.Description   = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.ID            = reader.ReadInt32();
                    entry.GameID        = (GameID)reader.ReadByte();
                    entry.Game          = (MemoryEntry.MemoryGame)reader.ReadByte();
                    Memories.Add(entry);
                    reader.JumpAhead(2);
                }
            }

            // Characters
            using (var reader = CreateReader(4))
            {
                while (!EOF(reader))
                {
                    var entry = new CharacterEntry();
                    entry.FriendlyName  = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.ID            = reader.ReadInt32();
                    Characters.Add(entry);
                }
            }

            // Unknown2
            using (var reader = CreateReader(5))
            {
                while (!EOF(reader))
                {
                    var entry = new Unknown2Entry();
                    entry.Unknown1      = reader.ReadInt32();
                    entry.Unknown2      = reader.ReadInt32();
                    Unknown2.Add(entry);
                }
            }

            // Unknown3
            using (var reader = CreateReader(6))
            {
                while (!EOF(reader))
                {
                    var entry = new Unknown3Entry();
                    entry.ID            = reader.ReadInt16();
                    entry.Unknown2      = reader.ReadInt32();
                    Unknown3.Add(entry);
                }
            }

            // Voices
            using (var reader = CreateReader(7))
            {
                while (!EOF(reader))
                {
                    var entry = new VoiceEntry();
                    entry.UnknownName   = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.KnownName     = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.PreferedName  = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.ID            = reader.ReadInt32();
                    Voices.Add(entry);
                }
            }

            // Unknown4
            using (var reader = CreateReader(8))
            {
                while (!EOF(reader))
                {
                    var entry = new Unknown4Entry();
                    entry.Unknown1      = reader.ReadInt16();
                    entry.Unknown2      = reader.ReadInt32();
                    Unknown4.Add(entry);
                }
            }

            // Art Book Page
            using (var reader = CreateReader(9))
            {
                while (!EOF(reader))
                {
                    var entry = new ArtBookPageEntry();
                    entry.PagePathThumbnail = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.PagePathData  = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.Name          = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.ID            = reader.ReadInt32();
                    entry.GameID        = (GameID)reader.ReadInt16();
                    entry.Page          = reader.ReadInt16();
                    ArtBookPages.Add(entry);
                }
            }

            // DramaCDs
            using (var reader = CreateReader(10))
            {
                while (!EOF(reader))
                {
                    var entry = new DramaCDEntry();
                    entry.FileName      = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.FriendlyName  = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.SourceAlbum   = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.InternalName  = fileReader.ReadStringElsewhere(reader.ReadInt32(), false);
                    entry.ID            = reader.ReadInt16();
                    entry.Game          = (GameID)reader.ReadInt16();
                    entry.Unknown7      = reader.ReadInt16();
                    entry.SourceTrackID = reader.ReadInt16();
                    entry.Unknown9      = reader.ReadInt16();
                    DramaCDs.Add(entry);
                    reader.JumpAhead(2);
                }
            }
        }

        /// <summary>
        /// Creates a Reader for a database param
        /// </summary>
        /// <param name="index">Database Param Index</param>
        /// <returns>a reader attached to the array of thae param</returns>
        public ExtendedBinaryReader CreateReader(int index)
        {
            var stream = new MemoryStream(Instructions[index].GetArgument<byte[]>(1));
            var reader = new ExtendedBinaryReader(stream, Encoding.UTF8);
            return reader;
        }

        /// <summary>
        /// Helper for checking if we are at the end of the stream
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public bool EOF(BinaryReader reader)
        {
            return reader.BaseStream.Position >= reader.BaseStream.Length;
        }

    }
}
