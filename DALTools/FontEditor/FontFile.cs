using DALLib.File;
using DALLib.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace FontEditor
{
    public class FontFile : FileBase
    {
        public int CharacterHeight = 0;
        public float WidthScale = 0;
        public float HeightScale = 0;
        public List<FontEntry> Characters = new List<FontEntry>();

        public override void Load(ExtendedBinaryReader reader)
        {
            CharacterHeight = reader.ReadInt32();
            int characterCount = reader.ReadInt32();
            WidthScale = reader.ReadSingle();
            HeightScale = reader.ReadSingle();

            for (int i = 0; i < characterCount; ++i)
            {
                var fontEntry = new FontEntry();
                fontEntry.Character = ReadReversedUTF8Character(reader);
                fontEntry.XScale = reader.ReadSingle();
                fontEntry.YScale = reader.ReadSingle();
                fontEntry.Kerning = reader.ReadInt32();
                fontEntry.Width = reader.ReadInt32();
                Characters.Add(fontEntry);
            }
        }

        public override void Save(Stream fileStream)
        {
            var writer = new ExtendedBinaryWriter(fileStream, Encoding.UTF8);
            writer.Write(CharacterHeight);
            writer.Write(Characters.Count);
            writer.Write(WidthScale);
            writer.Write(HeightScale);
            for (int i = 0; i < Characters.Count; ++i)
            {
                var fontEntry = Characters[i];
                // For whatever reason the UTF-8 bytes are reversed here
                //  While in the STSC it is not.
                writer.Write(Encoding.UTF8.GetBytes(fontEntry.Character.ToString()).Reverse().ToArray());
                // UTD-8 bytes needs to be 4 bytes, padded in the end
                writer.FixPadding();
                writer.Write(fontEntry.XScale);
                writer.Write(fontEntry.YScale);
                writer.Write(fontEntry.Kerning);
                writer.Write(fontEntry.Width);
            }
        }

        public char ReadReversedUTF8Character(ExtendedBinaryReader reader)
        {
            return Encoding.GetEncoding("utf-8").GetChars(reader.ReadBytes(4).Reverse().ToArray()).FirstOrDefault(t => t != (char)0);
        }

        public class FontEntry : INotifyPropertyChanged
        {

            public event PropertyChangedEventHandler PropertyChanged;

            // Workaround for updating the List UI as I don't know how to use WPF
            public string Display
            {
                get => Character.ToString();
                set
                {
                    Character = value[0];
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Display"));
                }
            }

            public char Character;
            public float XScale;
            public float YScale;
            public int Kerning;
            public int Width;

            public override string ToString()
            {
                return Character.ToString();
            }
        }
    }
}
