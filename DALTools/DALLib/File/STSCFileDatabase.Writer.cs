using DALLib.IO;
using DALLib.Scripting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.File
{
    // Writer class for STSCFileDatabase
    public partial class STSCFileDatabase
    {

        public override void Save(ExtendedBinaryWriter writer)
        {
            ManualCount = 0;
            var strings = new List<string>();
            writer.WriteSignature("STSC");
            writer.AddOffset("EntryPosition");
            writer.Write(0x07); // Version
            writer.WriteSignature(ScriptName);
            writer.WriteNulls((uint)(0x20 - ScriptName.Length)); // Pad Script Name
            writer.Write(0x000607E3);
            writer.Write((short)0x06);
            writer.Write((short)0x0A);
            writer.Write((short)0x06);
            writer.Write((short)0x17);
            writer.Write(ScriptID);
            writer.FillInOffset("EntryPosition");
            foreach (var instruction in Instructions)
            {
                writer.Write((byte)STSCInstructions.DALRRInstructions.FindIndex(t => t?.Name == instruction.Name));
                instruction.Write(writer, ref ManualCount, strings);
            }
            // Write String Table
            for (int i = 0; i < strings.Count; ++i)
            {
                writer.FillInOffset($"Strings_{i}");
                writer.WriteNullTerminatedString(strings[i]);
            }
            writer.FixPadding(0x04);
            WriteStage1(writer);
            WriteStage2(writer);
            WriteStage3(writer);
            WriteStage4(writer);
            WriteStage5(writer);
            WriteStage6(writer);
            WriteStage7(writer);
            WriteStage8(writer);
            WriteStage9(writer);
            WriteStage10(writer);
            WriteStage11(writer);
            writer.FixPadding(0x10);
        }

        /// <summary>
        /// System Text
        /// </summary>
        public void WriteStage1(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_0l", true, false);
            for (int i = 0; i < _systemText.Count; ++i)
                writer.AddOffset($"Stage1_{i}");
            writer.FillInOffset("Manual_Ptr_0h", true, false);
            for (int i = 0; i < _systemText.Count; ++i)
            {
                writer.FillInOffset($"Stage1_{i}");
                writer.WriteNullTerminatedString(_systemText.ElementAt(i));
            }
            writer.FixPadding(0x4);
        }

        /// <summary>
        /// CGs
        /// </summary>
        public void WriteStage2(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_1l", true, false);
            int i = 0;
            foreach (var entry in _CGs)
            {
                writer.AddOffset($"CGName{i}");
                writer.Write(entry.ID);
                writer.Write(entry.CGID);
                writer.Write(entry.CGID2);
                writer.Write(entry.Unknown5);
                writer.Write(entry.Unknown6);
                writer.Write(entry.TextureWidth);
                writer.Write(entry.TextureHeight);
                writer.Write(entry.Unknown7);
                writer.Write(entry.Unknown81);
                writer.Write(entry.Unknown82);
                writer.Write(entry.Unknown83);
                writer.Write(entry.Page);
                writer.Write(entry.FrameCount);
                writer.Write((byte)entry.GameID);
                writer.Write(entry.Unknown93);
                writer.Write(entry.Unknown94);
                writer.Write(entry.Unknown10);
                writer.Write(entry.Unknown11);
                writer.Write(entry.Unknown12);
                writer.Write(entry.Unknown13);
                writer.Write(entry.Unknown14);
                writer.Write(entry.Unknown15);
                writer.Write(entry.Unknown16);
                writer.Write(entry.Unknown17);
                writer.Write(entry.Unknown18);
                writer.Write(entry.Unknown19);
                writer.Write(entry.Unknown20);
                writer.Write(entry.Unknown21);
                writer.Write(entry.Unknown22);
                writer.Write(entry.Unknown23);
                writer.Write(entry.Unknown24);
                writer.Write(entry.Unknown25);
                writer.Write(entry.Unknown26);
                writer.Write(entry.Unknown27);
                ++i;
            }
            writer.FillInOffset("Manual_Ptr_1h", true, false);
            i = 0;
            foreach (var entry in _CGs)
            {
                writer.FillInOffset($"CGName{i}");
                writer.WriteNullTerminatedString(entry.Name);
                ++i;
            }
            writer.FixPadding(0x4);
        }

        /// <summary>
        /// Movies
        /// </summary>
        public void WriteStage3(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_2l", true, false);
            int i = 0;
            foreach (var entry in _movies)
            {
                writer.AddOffset($"Name{i}");
                writer.AddOffset($"Path{i}");
                writer.Write(entry.ID);
                writer.Write(entry.Unknown4);
                writer.Write((byte)entry.GameID);
                writer.Write(entry.Unknown5);
                ++i;
            }
            writer.FillInOffset("Manual_Ptr_2h", true, false);
            i = 0;
            foreach (var entry in _movies)
            {
                writer.FillInOffset($"Name{i}");
                writer.WriteNullTerminatedString(entry.FriendlyName);
                writer.FillInOffset($"Path{i}");
                writer.WriteNullTerminatedString(entry.FilePath);
                ++i;
            }
            writer.FixPadding(0x4);
        }

        /// <summary>
        /// Memories
        /// </summary>
        public void WriteStage4(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_3l", true, false);
            int i = 0;
            foreach (var entry in _memories)
            {
                writer.AddOffset($"MemName{i}");
                writer.AddOffset($"MemDesc{i}");
                writer.Write(entry.ID);
                writer.Write((byte)entry.GameID);
                writer.Write((byte)entry.Game);
                writer.Write((short)0); // Padding
                ++i;
            }
            writer.FillInOffset("Manual_Ptr_3h", true, false);
            i = 0;
            foreach (var entry in _memories)
            {
                writer.FillInOffset($"MemName{i}");
                writer.WriteNullTerminatedString(entry.Name);
                writer.FillInOffset($"MemDesc{i}");
                writer.WriteNullTerminatedString(entry.Description);
                ++i;
            }
            writer.FixPadding(0x4);
        }

        /// <summary>
        /// Characters
        /// </summary>
        public void WriteStage5(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_4l", true, false);
            int i = 0;
            foreach (var entry in _characters)
            {
                writer.AddOffset($"Name{i}");
                writer.Write(entry.ID);
                ++i;
            }
            writer.FillInOffset("Manual_Ptr_4h", true, false);
            i = 0;
            foreach (var entry in _characters)
            {
                writer.FillInOffset($"Name{i}");
                writer.WriteNullTerminatedString(entry.FriendlyName);
                ++i;
            }
            writer.FixPadding(0x4);
        }

        /// <summary>
        /// Unknown 2
        /// </summary>
        public void WriteStage6(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_5l", true, false);
            foreach (var entry in _unknown2)
            {
                writer.Write(entry.Unknown1);
                writer.Write(entry.Unknown2);
            }
            writer.FillInOffset("Manual_Ptr_5h", true, false);
            writer.FixPadding(0x4);
        }

        /// <summary>
        /// Unknown 3
        /// </summary>
        public void WriteStage7(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_6l", true, false);
            foreach (var entry in _unknown3)
            {
                writer.Write(entry.ID);
                writer.Write(entry.Unknown2);
            }
            writer.FillInOffset("Manual_Ptr_6h", true, false);
            writer.FixPadding(0x4);
        }

        /// <summary>
        /// Voices
        /// </summary>
        public void WriteStage8(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_7l", true, false);
            int i = 0;
            foreach (var entry in _voices)
            {
                writer.AddOffset($"UnknownName{i}");
                writer.AddOffset($"KnownName{i}");
                writer.AddOffset($"PreferedName{i}");
                writer.Write(entry.ID);
                ++i;
            }
            writer.FillInOffset("Manual_Ptr_7h", true, false);
            i = 0;
            foreach (var entry in _voices)
            {
                writer.FillInOffset($"UnknownName{i}");
                writer.WriteNullTerminatedString(entry.UnknownName);
                writer.FillInOffset($"KnownName{i}");
                writer.WriteNullTerminatedString(entry.KnownName);
                writer.FillInOffset($"PreferedName{i}");
                writer.WriteNullTerminatedString(entry.PreferedName);
                ++i;
            }
            writer.FixPadding(0x4);
        }

        /// <summary>
        /// Unknown 4
        /// </summary>
        public void WriteStage9(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_8l", true, false);
            foreach (var entry in _unknown4)
            {
                writer.Write(entry.Unknown1);
                writer.Write(entry.Unknown2);
            }
            writer.FillInOffset("Manual_Ptr_8h", true, false);
            writer.FixPadding(0x4);
        }

        /// <summary>
        /// ArtBook Pages
        /// </summary>
        public void WriteStage10(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_9l", true, false);
            int i = 0;
            foreach (var entry in _artBookPages)
            {
                writer.AddOffset($"PagePathThumbnail{i}");
                writer.AddOffset($"PagePathData{i}");
                writer.AddOffset($"Name{i}");
                writer.Write(entry.ID);
                writer.Write((short)entry.GameID);
                writer.Write(entry.Page);
                ++i;
            }
            writer.FillInOffset("Manual_Ptr_9h", true, false);
            i = 0;
            foreach (var entry in _artBookPages)
            {
                writer.FillInOffset($"PagePathThumbnail{i}");
                writer.WriteNullTerminatedString(entry.PagePathThumbnail);
                writer.FillInOffset($"PagePathData{i}");
                writer.WriteNullTerminatedString(entry.PagePathData);
                writer.FillInOffset($"Name{i}");
                writer.WriteNullTerminatedString(entry.Name);
                ++i;
            }
            writer.FixPadding(0x4);
        }

        /// <summary>
        /// Druma CD
        /// </summary>
        public void WriteStage11(ExtendedBinaryWriter writer)
        {
            writer.FillInOffset("Manual_Ptr_10l", true, false);
            int i = 0;
            foreach (var entry in _dramaCDs)
            {
                writer.AddOffset($"FileName{i}");
                writer.AddOffset($"FriendlyName{i}");
                writer.AddOffset($"SourceAlbum{i}");
                writer.AddOffset($"InternalName{i}");
                writer.Write(entry.ID);
                writer.Write((short)entry.Game);
                writer.Write(entry.Unknown7);
                writer.Write(entry.SourceTrackID);
                writer.Write(entry.Unknown9);
                ++i;
            }
            writer.FillInOffset("Manual_Ptr_10h", true, false);
            i = 0;
            foreach (var entry in _dramaCDs)
            {
                writer.FillInOffset($"FileName{i}");
                writer.WriteNullTerminatedString(entry.FileName);
                writer.FillInOffset($"FriendlyName{i}");
                writer.WriteNullTerminatedString(entry.FriendlyName);
                writer.FillInOffset($"SourceAlbum{i}");
                writer.WriteNullTerminatedString(entry.SourceAlbum);
                writer.FillInOffset($"InternalName{i}");
                writer.WriteNullTerminatedString(entry.InternalName);
                ++i;
            }
            writer.FixPadding(0x4);
        }

    }
}
