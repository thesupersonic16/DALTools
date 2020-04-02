using DALLib.Exceptions;
using DALLib.IO;
using DALLib.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.File
{
    public class STSCFile : FileBase
    {
        /// <summary>
        /// Name of the script, This is usually just the name of the file
        /// </summary>
        public string ScriptName { get; set; }
        /// <summary>
        /// The script ID is used to identify the script
        /// </summary>
        public uint ScriptID = 0;
        /// <summary>
        /// List of instructions inside the script
        /// </summary>
        public List<STSCInstructions.Instruction> Instructions = new List<STSCInstructions.Instruction>();
        /// <summary>
        /// Count of manual offsets ready
        /// </summary>
        public int ManualCount = 0;
        /// <summary>
        /// If the script should use the smaller headers
        /// </summary>
        public bool UseSmallHeader = false;

        public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            // Check if file has a signature
            string sig = reader.ReadSignature();
            if (sig == "STSC")
            { // Newer script format
                uint headerSize = reader.ReadUInt32();
                uint version = reader.ReadUInt32();
                ScriptName = reader.ReadNullTerminatedString();
                reader.JumpAhead((uint)(0x20 - (ScriptName.Length + 1)));
                reader.JumpAhead(12);
                ScriptID = reader.ReadUInt32();
            }
            else
            { // Older script format
                // Jump back as older scripts have much smaller headers
                reader.JumpBehind(4);
                ScriptID = reader.ReadUInt16();
            }
            // Stupid workaround to find the end of the code segment
            reader.Offset = (uint)reader.BaseStream.Length;
            while (true)
            {
                byte opcode = reader.ReadByte();

                if (opcode >= 0x94)
                {
                    throw new STSCDisassembleException(this, 
                        $"Got opcode 0x{opcode:X2} at 0x{reader.BaseStream.Position - 1:X8} in \"{ScriptName}\"" +
                        " There is no opcodes larger than 0x93!");
                }

                // Check if its a known instruction
                if (STSCInstructions.DALRRInstructions[opcode] == null)
                {
                    throw new STSCDisassembleException(this,
                        $"Got opcode 0x{opcode:X2} at 0x{reader.BaseStream.Position - 1:X8} in \"{ScriptName}\"" +
                        " This opcode is unknown!");
                }

                var instruction = STSCInstructions.DALRRInstructions[opcode].Read(reader);
                Instructions.Add(instruction);
                if (reader.BaseStream.Position >= reader.Offset)
                    break;
            }
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
            var strings = new List<string>();
            writer.WriteSignature("STSC");
            writer.AddOffset("EntryPosition");
            writer.Write(0x07); // Version
            writer.WriteSignature(ScriptName);
            writer.WriteNulls((uint) (0x20 - ScriptName.Length)); // Pad Script Name
            writer.Write(0x000507E3);
            writer.Write((short) 0x09);
            writer.Write((short) 0x0D);
            writer.Write((short) 0x19);
            writer.Write((short) 0x0D);
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
                if (!writer.HasOffset($"Strings_{i}"))
                    continue;
                writer.FillInOffset($"Strings_{i}");
                writer.WriteNullTerminatedString(strings[i]);
            }
            writer.FixPadding(0x10);
        }

        public int FindAddress(int index)
        {
            int address = 0;
            for (int i = 0; i < index; ++i)
                address += Instructions[i].GetInstructionSize();
            return address;
        }

        public int FindIndex(int address)
        {
            int tempAddress = 0x3C;
            for (int i = 0; i < Instructions.Count; ++i)
            {
                if (tempAddress == address)
                    return i;
                tempAddress += Instructions[i].GetInstructionSize();
            }
            return 0;
        }

        public override string ToString()
        {
            return ScriptName;
        }
    }
}
