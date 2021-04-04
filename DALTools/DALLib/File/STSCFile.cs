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
        /// <summary>
        /// The version of the script file (0x07 - Remake)
        /// </summary>
        public uint Version = 0x07;

        public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            // Check if file has a signature
            string sig = reader.ReadSignature();
            if (sig == "STSC")
            { // Newer script format
                uint headerSize = reader.ReadUInt32();
                Version = reader.ReadUInt32();
                switch (Version)
                {
                    case 4: // Date A Live: Twin Edition Rio Reincarnation (PSV)
                        ScriptID = reader.ReadUInt16();
                        break;
                    case 7: // Date A Live: Rio Reincarnation (PC)
                        ScriptName = reader.ReadNullTerminatedString();
                        reader.JumpAhead((uint)(0x20 - (ScriptName.Length + 1)));
                        reader.JumpAhead(12);
                        ScriptID = reader.ReadUInt32();
                        break;
                }
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
            writer.Write(Version);
            switch (Version)
            {
                case 4: // Date A Live: Twin Edition Rio Reincarnation (PSV)
                    writer.Write((ushort)ScriptID);
                    break;
                case 7: // Date A Live: Rio Reincarnation (PC)
                    writer.WriteSignature(ScriptName);
                    writer.WriteNulls((uint)(0x20 - ScriptName.Length)); // Pad Script Name
                    writer.Write(0x000507E3);
                    writer.Write((short)0x09);
                    writer.Write((short)0x0D);
                    writer.Write((short)0x19);
                    writer.Write((short)0x0D);
                    writer.Write(ScriptID);
                    break;
            }

            writer.FillInOffset("EntryPosition");
            foreach (var instruction in Instructions)
            {
                writer.Write((byte)STSCInstructions.DALRRInstructions.FindIndex(t => t?.Name == instruction.Name));
                instruction.Write(writer, ref ManualCount, strings);
            }
            // Write String Table
            Dictionary<string, uint> writtenStrings = new Dictionary<string, uint>();
            for (int i = 0; i < strings.Count; ++i)
            {
                if (!writer.HasOffset($"Strings_{i}"))
                    continue;
                if (writtenStrings.ContainsKey(strings[i]))
                {
                    writer.FillInOffset($"Strings_{i}", writtenStrings[strings[i]]);
                    continue;
                }
                writer.FillInOffset($"Strings_{i}");
                writtenStrings.Add(strings[i], (uint)writer.BaseStream.Position);
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
            if (Version == 4)
                tempAddress = 0x0E;
            for (int i = 0; i < Instructions.Count; ++i)
            {
                if (tempAddress >= address)
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
