using DALLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.File
{
    // Roughly written to allow exporting of static CGs with multiple frames, Needs rewriting
    public class MAFile : FileBase
    {

        public List<MATexture> Textures = new List<MATexture>();
        public List<MALayer> Layers = new List<MALayer>();

        public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            string header = reader.ReadNullTerminatedString();
            reader.FixPadding(16);
            uint textureOffset = reader.ReadUInt32();
            uint textureCount = reader.ReadUInt32();
            int layerOffset = reader.ReadInt32();
            int layerCount = reader.ReadInt32();
            int unknownOffset = reader.ReadInt32();
            int unknownCount = reader.ReadInt32();
            uint unknown28 = reader.ReadUInt32();
            uint unknown2C = reader.ReadUInt32();
            string projectDir = reader.ReadStringElsewhere();
            uint unknown34 = reader.ReadUInt32();
            uint unknown38 = reader.ReadUInt32();
            uint unknown3C = reader.ReadUInt32();

            reader.JumpTo(textureOffset);
            for (int i = 0; i < textureCount; ++i)
                Textures.Add(new MATexture(reader));

            reader.JumpTo(layerOffset);
            for (int i = 0; i < layerCount; ++i)
                Layers.Add(new MALayer(reader));
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
        }

        public class MALayer
        {
            public string LayerName = "";
            public int TextureID = 0;
            public float LayerOffX = 0;
            public float LayerOffY = 0;
            public float LayerWidth = 0;
            public float LayerHeight = 0;
            public List<MAVert> Verts = new List<MAVert>();

            public MALayer() { }

            public MALayer(ExtendedBinaryReader reader)
            {
                Load(reader);
            }

            public void Load(ExtendedBinaryReader reader)
            {
                LayerName = reader.ReadSignature(0x20);
                uint unknown20 = reader.ReadUInt32();
                TextureID = reader.ReadInt32();
                uint Offset = reader.ReadUInt32();
                uint unknown2C = reader.ReadUInt32();

                //
                uint oldpos = (uint)reader.GetPosition();
                reader.JumpTo(Offset);

                reader.JumpAhead(0x164);
                LayerOffX = reader.ReadSingle();
                LayerOffY = reader.ReadSingle();
                reader.JumpAhead(4);
                LayerWidth = reader.ReadSingle();
                LayerHeight = reader.ReadSingle();

                reader.JumpAhead(0x88); // Unknown

                /*
                 * Top Left
                 * Top Right
                 * Bottom Left
                 * Bottom Right
                 */

                for (int i = 0; i < 4; ++i)
                    Verts.Add(new MAVert(reader));

                reader.JumpTo(oldpos);

                reader.JumpAhead(0x84); // Unknown
            }
        }

        public class MAVert
        {
            public float SourceX = 0;
            public float SourceY = 0;
            public float DestinX = 0;
            public float DestinY = 0;

            public MAVert() { }

            public MAVert(ExtendedBinaryReader reader)
            {
                Load(reader);
            }

            public void Load(ExtendedBinaryReader reader)
            {
                SourceX = reader.ReadSingle();
                SourceY = reader.ReadSingle();
                DestinX = reader.ReadSingle();
                DestinY = reader.ReadSingle();
                reader.JumpAhead(0x30); // Unknown
            }
        }

        public class MATexture
        {
            public string TextureFullPath = "";
            public string TextureFileName = "";
            public int TextureWidth = 0;
            public int TextureHeight = 0;

            public MATexture() { }

            public MATexture(ExtendedBinaryReader reader)
            {
                Load(reader);
            }

            public void Load(ExtendedBinaryReader reader)
            {
                TextureFullPath = reader.ReadSignature(0x200).Trim('\0');
                TextureFileName = reader.ReadSignature(0x080).Trim('\0');
                TextureWidth    = reader.ReadInt32();
                TextureHeight   = reader.ReadInt32();
                reader.JumpAhead(0x84); // Unknown
            }
        }
    }
}
