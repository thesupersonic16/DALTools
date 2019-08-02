using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeLib.Exceptions;
using HedgeLib.IO;

namespace STSCTool
{
    public class STSCFile : FileBase
    {
        public string ScriptName = "";
        public uint ScriptID = 0; // Used to store script information
        public List<STSCInstructions.Instruction> Instructions = new List<STSCInstructions.Instruction>();
        
        public override void Load(Stream fileStream)
        {
            var reader = new ExtendedBinaryReader(fileStream, Encoding.UTF8);
            string sig = reader.ReadSignature();
            if (sig != "STSC")
                throw new InvalidSignatureException("STSC", sig);
            uint headerSize = reader.ReadUInt32();
            uint unknown2 = reader.ReadUInt32();
            ScriptName = reader.ReadNullTerminatedString();
            reader.JumpAhead((uint)(0x20 - (ScriptName.Length + 1)));
            // TODO: Find a better way to find an end of the code segment
            reader.JumpAhead(12);
            ScriptID = reader.ReadUInt32();
            reader.JumpAhead(1);
            uint end = reader.ReadUInt32();
            reader.JumpBehind(5);
            int position = 0;
            while (reader.BaseStream.Position < end)
            {
                position = (int) reader.BaseStream.Position;
                byte opcode = reader.ReadByte();
                // Check if its a known instruction
                if (STSCInstructions.Instructions[opcode] == null)
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        Console.WriteLine(Instructions[Instructions.Count - 5 + i].Name);
                    }
                    Console.WriteLine("Error: Instruction {0:X2} at {1:X} is unknown, Dissembler must abort now!", opcode, position);
                    Console.ReadKey(true);
                    return;
                }

                Instructions.Add(STSCInstructions.Instructions[opcode].Read(reader));
            }
        }

        public override void Save(Stream fileStream)
        {
            ExtendedBinaryWriter writer = new ExtendedBinaryWriter(fileStream, Encoding.UTF8);
            var strings = new List<string>();
            writer.WriteSignature("STSC");
            writer.Write(0x3C);
            writer.Write(0x07);
            writer.WriteSignature(ScriptName);
            writer.WriteNulls((uint) (0x20 - ScriptName.Length));
            writer.Write(0x000507E3);
            writer.Write((short) 0x09);
            writer.Write((short) 0x0D);
            writer.Write((short) 0x19);
            writer.Write((short) 0x0D);
            writer.Write(ScriptID);
            foreach (var instruction in Instructions)
            {
                writer.Write((byte)STSCInstructions.Instructions.FindIndex(t => t?.Name == instruction.Name));
                instruction.Write(writer, strings);
            }
            // Write String Table
            for (int i = 0; i < strings.Count; ++i)
            {
                writer.FillInOffset($"Strings_{i}");
                writer.WriteNullTerminatedString(strings[i]);
            }
            writer.FixPadding(0x10);
        }
    }
}
