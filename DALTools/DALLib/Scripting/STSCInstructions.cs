using DALLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DALLib.Scripting.STSCInstructions.ArgumentType;

namespace DALLib.Scripting
{
    public static partial class STSCInstructions
    {

        public enum ArgumentType
        {
            AT_Bool, AT_Byte, AT_Int16, AT_Int32, AT_Float, AT_String, AT_StringPtr, AT_CodePointer, AT_DataReference, AT_PointerArray, AT_DataBlock
        }

        public class Instruction
        {
            public string Name;
            public ArgumentType[] ArgTypes;
            public List<object> Arguments = new List<object>();

            public Instruction(string name, ArgumentType[] arguments)
            {
                Name = name;
                ArgTypes = arguments;
            }

            public virtual T GetArgument<T>(int index)
            {
                return (T) Convert.ChangeType(Arguments[index], typeof(T));
            }

            public virtual Instruction Read(ExtendedBinaryReader reader)
            {
                var instruction = new Instruction(Name, ArgTypes);
                if (ArgTypes == null)
                    return instruction;
                for (int i = 0; i < ArgTypes.Length; ++i)
                {
                    if (ArgTypes[i] == AT_PointerArray)
                    {
                        int[] pointers = new int[reader.ReadByte()];
                        for (int i2 = 0; i2 < pointers.Length; ++i2)
                            pointers[i] = reader.ReadInt32();
                        Arguments.Add(pointers);
                    }
                    else
                        instruction.Arguments.Add(ReadByType(reader, ArgTypes[i]));
                }

                return instruction;
            }

            public virtual void Write(ExtendedBinaryWriter writer, ref int manualCount, List<string> strings = null)
            {
                if (ArgTypes == null)
                    return;
                for (int i = 0; i < ArgTypes.Length; ++i)
                {
                    if (ArgTypes[i] == AT_PointerArray)
                    {
                        var pointers = GetArgument<int[]>(i);
                        writer.Write((byte)pointers.Length);
                        foreach (var pointer in pointers)
                            writer.Write(pointer);
                    }
                    else
                        WriteByType(writer, ArgTypes[i], Arguments[i], ref manualCount, strings);
                }
            }

            public virtual int GetInstructionSize()
            {
                int size = 1;
                if (ArgTypes == null)
                    return size;
                foreach (var arg in ArgTypes)
                    switch (arg)
                    {
                        case AT_Bool:
                        case AT_Byte:
                            size += 1;
                            break;
                        case AT_Int16:
                            size += 2;
                            break;
                        case AT_Int32:
                        case AT_CodePointer:
                        case AT_DataReference:
                        case AT_Float:
                        case AT_String:
                        case AT_DataBlock:
                            size += 4;
                            break;
                        case AT_PointerArray:
                            size += GetArgument<int[]>(0).Length * 4 + 1;
                            break;
                    }

                return size;
            }

            public static object ReadByType(ExtendedBinaryReader reader, ArgumentType type)
            {
                switch (type)
                {
                    case AT_Bool:
                        return reader.ReadBoolean();
                    case AT_Byte:
                        return reader.ReadByte();
                    case AT_Int16:
                        return reader.ReadInt16();
                    case AT_Int32:
                        return reader.ReadInt32();
                    case AT_Float:
                        return reader.ReadSingle();
                    case AT_String:
                        return reader.ReadStringElsewhere();
                    case AT_StringPtr:
                        return reader.ReadNullTerminatedStringPointer();
                    case AT_CodePointer:
                        return reader.ReadInt32();
                    case AT_DataReference:
                        return reader.ReadUInt32();
                    case AT_DataBlock:
                        long position = reader.ReadUInt32();
                        long length = reader.ReadUInt32();
                        if (reader.Offset > position)
                            reader.Offset = (uint)position;
                        return new StreamBlock(position, length);
                    default:
                        return null;
                }
            }

            public static void WriteByType(ExtendedBinaryWriter writer, ArgumentType type, object value, ref int manualCount, List<string> strings = null)
            {
                switch (type)
                {
                    case AT_Bool:
                        writer.Write((bool)value);
                        break;
                    case AT_Byte:
                        writer.Write((byte)value);
                        break;
                    case AT_Int16:
                        writer.Write((short)value);
                        break;
                    case AT_Int32:
                        writer.Write((int)value);
                        break;
                    case AT_Float:
                        writer.Write((float)value);
                        break;
                    case AT_String:
                        if (strings == null)
                            break;
                        if (value == null)
                        {
                            writer.Write(0);
                            break;
                        }
                        writer.AddOffset($"Strings_{strings.Count}");
                        strings.Add((string)value);
                        break;
                    case AT_StringPtr:
                        if (strings == null)
                            break;
                        writer.AddOffset($"StringsPtr_{strings.Count}");
                        strings.Add((string)value);
                        break;
                    case AT_CodePointer:
                        writer.Write((int)value);
                        break;
                    case AT_DataReference:
                        writer.Write((int)value);
                        break;
                    case AT_DataBlock:
                        writer.AddOffset($"Manual_Ptr_{manualCount}l");
                        writer.AddOffset($"Manual_Ptr_{manualCount}h");
                        ++manualCount;
                        break;
                }
            }
        }

        public class InstructionIf : Instruction
        {
            public class Comparison
            {
                public enum ComparisonOperators
                {
                    Equals, NotEquals, GreaterThenOrEqual, GreaterThan, LessThenOrEqual, LessThen, NotZero, Zero
                }

                public Comparison(uint left, ComparisonOperators oper, uint right)
                {
                    Left = left;
                    Operator = oper;
                    Right = right;
                }

                public ComparisonOperators Operator;
                public uint Left;
                public uint Right;
            }
            public List<Comparison> Comparisons = new List<Comparison>();

            public InstructionIf() : base("if", new [] {AT_CodePointer})
            {

            }

            public override Instruction Read(ExtendedBinaryReader reader)
            {
                var instruction = new InstructionIf();
                short amount = reader.ReadInt16();
                instruction.Arguments.Add(reader.ReadInt32());
                for (int i = 0; i < amount; ++i)
                {
                    uint left = reader.ReadUInt32();
                    uint right = reader.ReadUInt32();
                    var compareOp = (Comparison.ComparisonOperators)reader.ReadByte();
                    instruction.Comparisons.Add(new Comparison(left, compareOp, right));
                }
                return instruction;
            }

            public override void Write(ExtendedBinaryWriter writer, ref int manualCount, List<string> strings = null)
            {
                writer.Write((short)Comparisons.Count);
                writer.Write(GetArgument<int>(0));
                foreach (var comparison in Comparisons)
                {
                    writer.Write((uint)comparison.Left);
                    writer.Write((uint)comparison.Right);
                    writer.Write((byte)comparison.Operator);
                }
            }

            public override int GetInstructionSize()
            {
                int size = 1;
                size += 2;
                size += 4;
                size += Comparisons.Count * 9;

                return size;
            }
        }
        public class InstructionSwitch : Instruction
        {
            public InstructionSwitch(string name, ArgumentType[] arguments) : base(name, arguments)
            {
            }

            public override Instruction Read(ExtendedBinaryReader reader)
            {
                var instruction = new InstructionSwitch(Name, ArgTypes);
                uint unknown = reader.ReadUInt32();
                ushort amount = reader.ReadUInt16();
                bool endFlag = amount >> 15 == 1; 
                if (endFlag)
                    amount &= 0x7FF;

                instruction.ArgTypes = new ArgumentType[amount * 2 + 3];
                instruction.ArgTypes[0] = AT_DataReference;
                instruction.Arguments.Add(unknown);
                instruction.ArgTypes[1] = AT_Int16;
                instruction.Arguments.Add(amount);
                instruction.ArgTypes[2] = AT_Bool;
                instruction.Arguments.Add(endFlag);
                for (int i = 0; i < amount; ++i)
                {
                    // case
                    instruction.ArgTypes[i * 2 + 3 + 0] = AT_Int32;
                    instruction.Arguments.Add(reader.ReadInt32());
                    // location
                    instruction.ArgTypes[i * 2 + 3 + 1] = AT_CodePointer;
                    instruction.Arguments.Add(reader.ReadInt32());
                }
                return instruction;
            }

            public override void Write(ExtendedBinaryWriter writer, ref int manualCount, List<string> strings = null)
            {
                writer.Write(GetArgument<uint>(0));
                writer.Write((ushort)(GetArgument<ushort>(1) | GetArgument<ushort>(2) << 15));
                for (int i = 0; i < GetArgument<ushort>(1); ++i)
                {
                    // case
                    writer.Write(GetArgument<int>(i * 2 + 3 + 0));
                    // location
                    writer.Write(GetArgument<int>(i * 2 + 3 + 1));
                }
            }

            public override int GetInstructionSize()
            {
                // Cheap way to ignore the flag
                return base.GetInstructionSize() - 1;
            }
        }
    }
}
