using DALLib.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.File
{
    public class FontFile : FileBase
    {
        /// <summary>
        /// Height of each character in pixels
        /// </summary>
        public int CharacterHeight = 0;
        /// <summary>
        /// Unknown
        /// </summary>
        public float WidthScale = 0;
        /// <summary>
        /// Unknown
        /// </summary>
        public float HeightScale = 0;
        /// <summary>
        /// List of Characters defined in the font code
        /// </summary>
        public List<FontEntry> Characters = new List<FontEntry>();

        public override void Load(ExtendedBinaryReader reader)
        {
            // Height of the character in pixels
            CharacterHeight     = reader.ReadInt32();
            // The amount of characters defined
            int characterCount  = reader.ReadInt32();
            // Unknown
            WidthScale          = reader.ReadSingle();
            HeightScale         = reader.ReadSingle();

            for (int i = 0; i < characterCount; ++i)
            {
                var fontEntry = new FontEntry
                {
                    Character = ReadReversedUTF8Char(reader),   // The read UTF8 character
                    XScale = reader.ReadSingle(),               // X position on the texture (Multiply by the textures width to get pixels)
                    YScale = reader.ReadSingle(),               // Y position on the texture (Multiply by the textures height to get pixels)
                    Kerning = reader.ReadInt32(),               // -X offset for positioning when rendering
                    Width = reader.ReadInt32()                  // The width of the character in pixels
                };
                Characters.Add(fontEntry);
            }
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
            // Height of the character in pixels
            writer.Write(CharacterHeight);
            // The amount of characters defined
            writer.Write(Characters.Count);
            // Unknown
            writer.Write(WidthScale);
            writer.Write(HeightScale);
            foreach (var entry in Characters)
            {
                // Writes the character in UTF-8 in reverse byte order, Not sure why its like this
                writer.Write(Encoding.UTF8.GetBytes(new[] { entry.Character }).Reverse().ToArray());
                // Pads the character to be 4 bytes long
                writer.FixPadding();
                // X position on the texture
                writer.Write(entry.XScale);
                // Y position on the texture
                writer.Write(entry.YScale);
                // -X render offset
                writer.Write(entry.Kerning);
                // Width of the character in pixels
                writer.Write(entry.Width);
            }
        }

        /// <summary>
        /// Reads a single 32-bit UTF8 character in reverse bit order
        /// </summary>
        /// <param name="reader">Reader used to read the character from</param>
        /// <returns>The read character</returns>
        public char ReadReversedUTF8Char(ExtendedBinaryReader reader)
        {
            return Encoding.UTF8.GetChars(reader.ReadBytes(4).Reverse().ToArray()).FirstOrDefault(t => t != (char)0);
        }

        public class FontEntry : INotifyPropertyChanged
        {
            public char Character { get; set; }
            /// <summary>
            /// The X position in the texture where the character is located
            /// <para/>
            /// Note: This is not in pixels, its range is from 0 to 1, you can convert to pixels by multiplying by the texture width
            /// </summary>
            public float XScale { get; set; }
            /// <summary>
            /// The Y position in the texture where the character is located
            /// <para/>
            /// Note: This is not in pixels, its range is from 0 to 1, you can convert to pixels by multiplying by the texture height
            /// </summary>
            public float YScale { get; set; }
            /// <summary>
            /// -X offset in pixels, Offsets the character to the left when being rendered
            /// </summary>
            public int Kerning { get; set; }
            /// <summary>
            /// The width of the character in pixels
            /// </summary>
            public int Width { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}
