using DALLib.Exceptions;
using DALLib.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.File
{
    public class FNTFile : FileBase
    {

        public FontFile FontCode { get; set; }
        public TEXFile FontTexture { get; set; }

        public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            string tableSig = reader.ReadDALSignature("Table");
            if (tableSig != "Table" && tableSig.Length <= 4)
                throw new SignatureMismatchException("Table", tableSig);

            int textureOffset = reader.ReadInt32();

            FontCode = new FontFile();
            FontTexture = new TEXFile();

            FontCode.WidthScale = -1;
            FontCode.HeightScale = -1;
            FontCode.MonospaceOnly = true;
            FontCode.Load(reader, true);

            // Load texture
            reader.JumpTo(textureOffset);
            reader.FixPadding(0x08);
            FontTexture.Load(reader, true);

            // Set scale size
            FontCode.WidthScale = (float)FontCode.CharacterHeight / FontTexture.SheetWidth;
            FontCode.HeightScale = (float)FontCode.CharacterHeight / FontTexture.SheetHeight;
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
            writer.WriteDALSignature("Table", false);

            writer.AddOffset("texture");

            // Code
            FontCode.WidthScale = -1;
            FontCode.HeightScale = -1;
            FontCode.Save(writer);

            // Texture
            writer.FixPadding(0x08);
            writer.FillInOffset("texture");
            FontTexture.Save(writer);
        }
    }
}
