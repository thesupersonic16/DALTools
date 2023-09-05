using DALLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.Scripting
{
    public class STSC2Sequence
    {

        public List<STSC2Commands.Command> Lines = new List<STSC2Commands.Command>();

        public STSC2Sequence() { }

        public void ReadSequence(ExtendedBinaryReader reader, bool incudeCmdLineInfo)
        {
            reader.Offset = (uint)reader.BaseStream.Length;
            while (true)
            {
                // TODO: Maybe move half of this into the Read function
                // TODO: Figure out how this works
                byte[] CmdLineInfo = null;
                if (incudeCmdLineInfo)
                    CmdLineInfo = reader.ReadBytes(6);
                byte opcode = reader.ReadByte();

                // Out of bounds check
                if (opcode >= STSC2Commands.DALRDCommands.Count)
                    throw new Exception($"Cmd read error. Read 0x{opcode:X2} at {(reader.GetPosition() - 1):X4}.");

                var line = STSC2Commands.DALRDCommands[opcode].Read(reader, CmdLineInfo);
                Lines.Add(line);
                if (reader.BaseStream.Position >= reader.Offset)
                    break;
            }
        }

        public void WriteSequence(ExtendedBinaryWriter writer, bool incudeCmdLineInfo)
        {
            List<string> stringTable = new List<string>();
            Dictionary<string, uint> writtenStrings = new Dictionary<string, uint>();
            Dictionary<STSC2Node, uint> nodeStrOffsets = new Dictionary<STSC2Node, uint>();
            List<STSC2Node> nodes = new List<STSC2Node>();
            
            // Lines
            foreach (var line in Lines)
                line.Write(writer, stringTable, nodes, STSC2Commands.DALRDCommands);

            // TODO: Handle trees
            // Node strings
            foreach (var node in nodes)
            {
                if (node.Value is string str)
                {
                    nodeStrOffsets[node] = (uint)writer.BaseStream.Position;
                    writer.WriteNullTerminatedString(str);
                }
            }

            // Strings
            for (int i = 0; i < stringTable.Count; i++)
            {
                if (writtenStrings.ContainsKey(stringTable[i]))
                    writer.FillInOffset($"str{i}", writtenStrings[stringTable[i]]);
                else
                {
                    writtenStrings[stringTable[i]] = (uint)writer.BaseStream.Position;
                    writer.FillInOffset($"str{i}", true);
                    writer.WriteNullTerminatedString(stringTable[i]);
                }
            }

            // Nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                writer.FillInOffset($"node{i}");
                if (nodes[i].Value is string)
                    nodes[i].Write(writer, nodeStrOffsets[nodes[i]]);
                else
                    nodes[i].Write(writer, 0);
            }

            // Padding
            writer.FixPadding(0x10);
        }
    }
}
