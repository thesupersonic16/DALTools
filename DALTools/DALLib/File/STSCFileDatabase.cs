#pragma warning disable CS0067
using DALLib.IO;
using DALLib.Misc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.File
{
    // Main STSCFileDatabase class
    public partial class STSCFileDatabase : STSCFile
    {

        public ObservableCollection<string>             _systemText     = new ObservableCollection<string>();
        public ObservableCollection<CGEntry>            _CGs            = new ObservableCollection<CGEntry>();
        public ObservableCollection<MovieEntry>         _movies         = new ObservableCollection<MovieEntry>();
        public ObservableCollection<MemoryEntry>        _memories       = new ObservableCollection<MemoryEntry>();
        public ObservableCollection<CharacterEntry>     _characters     = new ObservableCollection<CharacterEntry>();
        public ObservableCollection<Unknown2Entry>      _unknown2       = new ObservableCollection<Unknown2Entry>();
        public ObservableCollection<Unknown3Entry>      _unknown3       = new ObservableCollection<Unknown3Entry>();
        public ObservableCollection<VoiceEntry>         _voices         = new ObservableCollection<VoiceEntry>();
        public ObservableCollection<Unknown4Entry>      _unknown4       = new ObservableCollection<Unknown4Entry>();
        public ObservableCollection<ArtBookPageEntry>   _artBookPages   = new ObservableCollection<ArtBookPageEntry>();
        public ObservableCollection<DramaCDEntry>       _dramaCDs       = new ObservableCollection<DramaCDEntry>();


        public ObservableCollection<string> SystemText              => _systemText;
        public ObservableCollection<CGEntry> CGs                    => _CGs;
        public ObservableCollection<MovieEntry> Movies              => _movies;
        public ObservableCollection<MemoryEntry> Memories           => _memories;
        public ObservableCollection<CharacterEntry> Characters      => _characters;
        public ObservableCollection<Unknown2Entry> Unknown2         => _unknown2;
        public ObservableCollection<Unknown3Entry> Unknown3         => _unknown3;
        public ObservableCollection<VoiceEntry> Voices              => _voices;
        public ObservableCollection<Unknown4Entry> Unknown4         => _unknown4;
        public ObservableCollection<ArtBookPageEntry> ArtBookPages  => _artBookPages;
        public ObservableCollection<DramaCDEntry> DramaCDs          => _dramaCDs;

        public void aLoad(Stream fileStream)
        {
            base.Load(fileStream);
            var reader = new ExtendedBinaryReader(fileStream);
            var array = Instructions[0].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 4; ++i)
            {
                int address             = BitConverter.ToInt32(array, i * 4);
                SystemText.Add(reader.ReadStringElsewhere(address));
            }
            array = Instructions[1].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 0x6C; ++i)
            {
                int nameAddress = BitConverter.ToInt32(array, i * 0x6C);
                var entry = new CGEntry();
                entry.Name              = reader.ReadStringElsewhere(nameAddress);
                entry.ID                = BitConverter.ToInt32(array, i * 0x6C + 0x0004);
                entry.CGID              = BitConverter.ToInt32(array, i * 0x6C + 0x0008);
                entry.CGID2             = BitConverter.ToInt32(array, i * 0x6C + 0x000C);
                entry.Unknown5          = BitConverter.ToUInt32(array, i * 0x6C + 0x0010);
                entry.Unknown6          = BitConverter.ToUInt16(array, i * 0x6C + 0x0014);
                entry.TextureWidth      = BitConverter.ToInt16(array, i * 0x6C + 0x0016);
                entry.TextureHeight     = BitConverter.ToInt16(array, i * 0x6C + 0x0018);
                entry.Unknown7          = BitConverter.ToUInt16(array, i * 0x6C + 0x001A);
                entry.Unknown81         = array[i * 0x6C + 0x001C];
                entry.Unknown82         = array[i * 0x6C + 0x001D];
                entry.Unknown83         = array[i * 0x6C + 0x001E];
                entry.Page              = array[i * 0x6C + 0x001F];
                entry.FrameCount        = array[i * 0x6C + 0x0020];
                entry.GameID            = (GameID)array[i * 0x6C + 0x0021];
                entry.Unknown93         = array[i * 0x6C + 0x0022];
                entry.Unknown94         = array[i * 0x6C + 0x0023];
                entry.Unknown10         = BitConverter.ToUInt32(array, i * 0x6C + 0x0024);
                entry.Unknown11         = BitConverter.ToUInt32(array, i * 0x6C + 0x0028);
                entry.Unknown12         = BitConverter.ToUInt32(array, i * 0x6C + 0x002C);
                entry.Unknown13         = BitConverter.ToUInt32(array, i * 0x6C + 0x0030);
                entry.Unknown14         = BitConverter.ToUInt32(array, i * 0x6C + 0x0034);
                entry.Unknown15         = BitConverter.ToUInt32(array, i * 0x6C + 0x0038);
                entry.Unknown16         = BitConverter.ToUInt32(array, i * 0x6C + 0x003C);
                entry.Unknown17         = BitConverter.ToUInt32(array, i * 0x6C + 0x0040);
                entry.Unknown18         = BitConverter.ToUInt32(array, i * 0x6C + 0x0044);
                entry.Unknown19         = BitConverter.ToUInt32(array, i * 0x6C + 0x0048);
                entry.Unknown20         = BitConverter.ToUInt32(array, i * 0x6C + 0x004C);
                entry.Unknown21         = BitConverter.ToUInt32(array, i * 0x6C + 0x0050);
                entry.Unknown22         = BitConverter.ToUInt32(array, i * 0x6C + 0x0054);
                entry.Unknown23         = BitConverter.ToUInt32(array, i * 0x6C + 0x0058);
                entry.Unknown24         = BitConverter.ToUInt32(array, i * 0x6C + 0x005C);
                entry.Unknown25         = BitConverter.ToUInt32(array, i * 0x6C + 0x0060);
                entry.Unknown26         = BitConverter.ToUInt32(array, i * 0x6C + 0x0064);
                entry.Unknown27         = BitConverter.ToUInt32(array, i * 0x6C + 0x0068);
                CGs.Add(entry);
            }

            array = Instructions[2].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 0x10; ++i)
            {
                var entry = new MovieEntry();
                entry.FriendlyName      = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x10 + 0));
                entry.FilePath          = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x10 + 4));
                entry.ID                = BitConverter.ToInt32(array, i * 0x10 + 8);
                entry.Unknown4          = array[i * 0x10 + 12];
                entry.GameID            = (GameID)array[i * 0x10 + 13];
                entry.Unknown5          = BitConverter.ToInt16(array, i * 0x10 + 14);
                Movies.Add(entry);
            }

            array = Instructions[3].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 0x10; ++i)
            {
                var entry = new MemoryEntry();
                entry.Name              = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x10 + 0));
                entry.Description       = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x10 + 4));
                entry.ID                = BitConverter.ToInt32(array, i * 0x10 + 8);
                entry.GameID            = (GameID)array[i * 0x10 + 12];
                entry.Game              = (MemoryEntry.MemoryGame)array[i * 0x10 + 13];
                Memories.Add(entry);
            }

            array = Instructions[4].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 0x08; ++i)
            {
                var entry = new CharacterEntry();
                entry.FriendlyName      = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x08 + 0));
                entry.ID                = BitConverter.ToInt32(array, i * 0x08 + 4);
                Characters.Add(entry);
            }
            array = Instructions[5].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 0x08; ++i)
            {
                var entry = new Unknown2Entry();
                entry.Unknown1          = BitConverter.ToInt32(array, i * 0x08 + 0);
                entry.Unknown2          = BitConverter.ToInt32(array, i * 0x08 + 4);
                Unknown2.Add(entry);
            }
            array = Instructions[6].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 0x06; ++i)
            {
                var entry = new Unknown3Entry();
                entry.ID                = BitConverter.ToInt16(array, i * 0x06 + 0);
                entry.Unknown2          = BitConverter.ToInt32(array, i * 0x06 + 2);
                Unknown3.Add(entry);
            }
            array = Instructions[7].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 0x10; ++i)
            {
                var entry = new VoiceEntry();
                entry.UnknownName       = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x10 + 0));
                entry.KnownName         = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x10 + 4));
                entry.PreferedName      = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x10 + 8));
                entry.ID = BitConverter.ToInt32(array, i * 0x10 + 12);
                Voices.Add(entry);
            }
            array = Instructions[8].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 0x06; ++i)
            {
                var entry = new Unknown4Entry();
                entry.Unknown1          = BitConverter.ToInt16(array, i * 0x06 + 0);
                entry.Unknown2          = BitConverter.ToInt32(array, i * 0x06 + 2);
                Unknown4.Add(entry);
            }
            array = Instructions[9].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 0x14; ++i)
            {
                var entry = new ArtBookPageEntry();
                entry.PagePathThumbnail = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x14 + 0));
                entry.PagePathData      = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x14 + 4));
                entry.Name              = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x14 + 8));
                entry.ID                = BitConverter.ToInt32(array, i * 0x14 + 12);
                entry.GameID            = (GameID)BitConverter.ToInt16(array, i * 0x14 + 16);
                entry.Page              = BitConverter.ToInt16(array, i * 0x14 + 18);
                ArtBookPages.Add(entry);
            }
            array = Instructions[10].GetArgument<byte[]>(1);
            for (int i = 0; i < array.Length / 0x1C; ++i)
            {
                var entry = new DramaCDEntry();
                entry.FileName          = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x1C + 0));
                entry.FriendlyName      = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x1C + 4));
                entry.SourceAlbum       = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x1C + 8));
                entry.InternalName      = reader.ReadStringElsewhere(BitConverter.ToInt32(array, i * 0x1C + 12));
                entry.ID                = BitConverter.ToInt16(array, i * 0x1C + 16);
                entry.Game              = (GameID)BitConverter.ToInt16(array, i * 0x1C + 18);
                entry.Unknown7          = BitConverter.ToInt16(array, i * 0x1C + 20);
                entry.SourceTrackID     = BitConverter.ToInt16(array, i * 0x1C + 22);
                entry.Unknown9          = BitConverter.ToInt16(array, i * 0x1C + 24);
                DramaCDs.Add(entry);
            }

            return;
        }

        public class CGEntry : INotifyPropertyChanged
        {
            /* 0x0000 */ public string Name         { get; set; }
            /* 0x0004 */ public int ID              { get; set; }
            /* 0x0008 */ public int CGID            { get; set; }
            /* 0x000C */ public int CGID2           { get; set; }
            /* 0x0010 */ public uint Unknown5;
            /* 0x0014 */ public ushort Unknown6;
            /* 0x0016 */ public short TextureWidth  { get; set; }
            /* 0x0016 */ public short TextureHeight { get; set; }
            /* 0x001A */ public ushort Unknown7;
            /* 0x001C */ public byte Unknown81;
            /* 0x001C */ public byte Unknown82;
            /* 0x001C */ public byte Unknown83;
            /* 0x001C */ public byte Page           { get; set; }
            /* 0x0020 */ public byte FrameCount     { get; set; }
            /* 0x0021 */ public GameID GameID       { get; set; }
            /* 0x0022 */ public byte Unknown93;
            /* 0x0023 */ public byte Unknown94;
            /* 0x0024 */ public uint Unknown10;
            /* 0x0028 */ public uint Unknown11;
            /* 0x002C */ public uint Unknown12;
            /* 0x0030 */ public uint Unknown13;
            /* 0x0034 */ public uint Unknown14;
            /* 0x0038 */ public uint Unknown15;
            /* 0x003C */ public uint Unknown16;
            /* 0x0040 */ public uint Unknown17;
            /* 0x0044 */ public uint Unknown18;
            /* 0x0048 */ public uint Unknown19;
            /* 0x004C */ public uint Unknown20;
            /* 0x0050 */ public uint Unknown21;
            /* 0x0054 */ public uint Unknown22;
            /* 0x0058 */ public uint Unknown23;
            /* 0x005C */ public uint Unknown24;
            /* 0x0060 */ public uint Unknown25;
            /* 0x0064 */ public uint Unknown26;
            /* 0x0068 */ public uint Unknown27;

            public event PropertyChangedEventHandler PropertyChanged;

            public override string ToString()
            {
                return Name;
            }
        }

        public class MovieEntry : INotifyPropertyChanged
        {
            public string FriendlyName              { get; set; }
            public string FilePath                  { get; set; }
            public int ID                           { get; set; }
            public byte Unknown4                    { get; set; }
            public GameID GameID                    { get; set; }
            public short Unknown5                   { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public override string ToString()
            {
                return $"Movie: {ID}, {FriendlyName}, {FilePath}";
            }
        }

        public class MemoryEntry : INotifyPropertyChanged
        {
            /// <summary>
            /// Friendly name of memory
            /// </summary>
            public string Name                      { get; set; }
            /// <summary>
            /// The Description of the memory displayed on the bottom
            /// </summary>
            public string Description               { get; set; }
            public int ID                           { get; set; }
            /// <summary>
            /// The ID of the game within the game files
            /// </summary>
            public GameID GameID                    { get; set; }
            /// <summary>
            /// The game category the memory belongs in
            /// </summary>
            public MemoryGame Game                  { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public override string ToString()
            {
                return $"Memory: {ID}, {Name}, {Description}, {Game}";
            }

            public enum MemoryGame
            {
                Rinne_Utopia,
                Arusu_Install,
                Rio_Reincarnation
            }
        }

        public class CharacterEntry : INotifyPropertyChanged
        {
            public string FriendlyName              { get; set; }
            public int ID                           { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public override string ToString()
            {
                return $"Character: {ID}, {FriendlyName}";
            }
        }

        public class VoiceEntry : INotifyPropertyChanged
        {
            /// <summary>
            /// Name used if you do not know the character
            /// </summary>
            public string UnknownName               { get; set; }
            /// <summary>
            /// The known name of the character
            /// </summary>
            public string KnownName                 { get; set; }
            /// <summary>
            /// The second name of the character, Used if they have another prefered name,
            /// if not, use KnownName
            /// </summary>
            public string PreferedName              { get; set; }
            /// <summary>
            /// The Voice ID of the character
            /// </summary>
            public int ID                           { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public override string ToString()
            {
                return $"Voice: {ID}, {KnownName}, {PreferedName}, {UnknownName}";
            }
        }

        public class Unknown2Entry
        {
            /* 0x0000 */ public int Unknown1;
            /* 0x0004 */ public int Unknown2;
        }

        public class Unknown3Entry
        {
            /* 0x0000 */ public short ID;
            /* 0x0002 */ public int Unknown2;
        }

        public class Unknown4Entry
        {
            /* 0x0000 */ public short Unknown1;
            /* 0x0002 */ public int Unknown2;
        }

        public class ArtBookPageEntry
        {
            /// <summary>
            /// Path relative of the NovThumb.pck archive to the texture without extension
            /// </summary>
            public string PagePathThumbnail         { get; set; }
            /// <summary>
            /// Path relative of the NovData.pck archive to the texture without extension
            /// </summary>
            public string PagePathData              { get; set; }
            /// <summary>
            /// Internal Name of the page, This isn't seen in game so its left in Japanese
            /// </summary>
            public string Name                      { get; set; }
            /// <summary>
            /// ID of the page
            /// </summary>
            public int ID                           { get; set; }
            /// <summary>
            /// ID to show which game the page belongs in
            /// </summary>
            public GameID GameID                    { get; set; }
            /// <summary>
            /// The page number
            /// </summary>
            public short Page                       { get; set; }

            public override string ToString()
            {
                return $"ArtBookPage: {PagePathThumbnail}, {PagePathData}, {Name}, {ID}, {GameID}, {Page}";
            }

        }

        public class DramaCDEntry : INotifyPropertyChanged
        {
            /* 0x0000 */ public string FileName     { get; set; }
            /* 0x0004 */ public string FriendlyName { get; set; }
            /* 0x0008 */ public string SourceAlbum  { get; set; }
            /* 0x000C */ public string InternalName { get; set; }
            /* 0x0010 */ public short ID            { get; set; }
            /* 0x0012 */ public GameID Game         { get; set; }
            /* 0x0014 */ public short Unknown7      { get; set; }
            /* 0x0016 */ public short SourceTrackID { get; set; }
            /* 0x0018 */ public int Unknown9        { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            public override string ToString()
            {
                return $"DramaCDEntry: {FileName}, {FriendlyName}, {SourceAlbum}, {InternalName}, {ID}, {Game}, {Unknown7}, {SourceTrackID}, {Unknown9}";
            }

        }
    }
}
