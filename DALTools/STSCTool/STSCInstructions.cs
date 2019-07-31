using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HedgeLib.IO;
using static STSCTool.STSCInstructions.ArgumentType;

namespace STSCTool
{
    public static class STSCInstructions
    {

        public static List<Instruction> Instructions = new List<Instruction>()
        {
            new Instruction("NOP", null),
            new Instruction("Exit", null),
            null, // Continue
            null, // Endv
            null, // InfinitWait
            new Instruction("Wait", new []{ AT_Int32 }),
            new Instruction("Goto", new []{ AT_Pointer }),
            null, // Return
            null, // ReturnPush
            null, // ReturnPop
            null, // Call_iv
            null, // SubStart
            new Instruction("SubEnd", new []{ AT_Int16 }),
            new Instruction("RandJump", new []{ AT_PointerArray }),
            new Instruction("Printf", new []{ AT_String }),
            new Instruction("FileJump", new []{ AT_String }),

            null, // FlgOnJump
            null, // FlgOffJump
            new Instruction("FlagSet", new []{ AT_Int16, AT_Byte }),
            null, // PrmTrueJump
            null, // PrmFalseJump
            new Instruction("PrmSet", new []{ AT_Int32, AT_Int32 }), 
            new Instruction("PrmCopy", new []{ AT_Int32, AT_Int32 }), 
            new Instruction("PrmAdd", new []{ AT_Int32, AT_Int32 }), 
            null, // PrmAddWk
            null, // PrmBranch
            null, // Call
            null, // CallReturn
            null, // SubEndWait
            new InstructionIf(),
            new Instruction("switch", new []{ AT_Byte }), // switch TODO
            null, // PrmRand

            null, // DataBaseParam
            null, // NewGameOpen
            null, // EventStartMes
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // SetEventNumber

            new Instruction("PlayMovie", new []{ AT_String, AT_Int16, AT_Byte}),
            new Instruction("BgmWait", new []{ AT_Byte, AT_Int16}),
            null, // BgmVolume
            new Instruction("SePlay", new []{ AT_Byte, AT_Byte, AT_Byte}),
            new Instruction("SeStop", new []{ AT_Byte, AT_Byte}),
            null, // SeWait 
            null, // SeVolume
            new Instruction("SeAllStop", null),
            new Instruction("BgmDummy", new []{ AT_Int32, AT_Int16}), // Data is discarded
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy

            null, // NowLoading
            new Instruction("Fade", new []{ AT_Byte, AT_Int16, AT_Int16}),
            new Instruction("PatternFade", new []{ AT_Int16, AT_Int16, AT_Int16}),
            new Instruction("Quake", new []{ AT_Byte, AT_Int16}),
            new Instruction("CrossFade", new []{ AT_Int16}),
            new Instruction("PatternCrossFade", new []{ AT_Int16, AT_Int16 }),
            null, // DispTone
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Dummy
            null, // Wait2

            new Instruction("Mes", new []{ AT_Byte, AT_Byte, AT_Byte, AT_Byte, AT_String, AT_Int16}),
            new Instruction("MesWait", null),
            new Instruction("MesTitle", new []{ AT_Byte}),
            new Instruction("SetChoice", new []{ AT_Pointer, AT_String, AT_Int16 }),
            new Instruction("ShowChoices", new []{ AT_Bool}),
            new Instruction("SetFontSize", new []{ AT_Byte}),
            null, // MapPlace
            null, // MapChara
            null, // MapBg
            null, // MapCoord
            null, // MapStart
            null, // MapInit
            new Instruction("MesWinClose", null),
            new Instruction("BgOpen", new []{ AT_Int32, AT_Int32}), // MesWinOpen?
            new Instruction("BgClose", new []{ AT_Byte}),
            new Instruction("MaAnime", new []{ AT_Byte}),

            new Instruction("BgMove", new []{ AT_Byte, AT_Int32, AT_Int16}),
            null, // BgScale
            new Instruction("BustOpen", new []{ AT_Byte, AT_Int32, AT_Int32}), // TODO
            new Instruction("BustClose", new []{ AT_Byte, AT_Int16}), // TODO
            null, // BustMove
            null, // BustMoveAdd
            null, // BustScale
            new Instruction("BustPriority", new []{ AT_Byte, AT_Byte}), // TODO
            new Instruction("PlayVoice", new []{ AT_Byte, AT_Int32, AT_String }), 
            new Instruction("VoiceCharaDraw", new []{ AT_Int16 }),
            new Instruction("DateSet", new []{ AT_Byte, AT_Byte, AT_Byte }), // TODO
            null, // TellOpen
            null, // TellClose
            new Instruction("Trophy", new []{ AT_Byte }), // TODO
            new Instruction("SetVibration", new []{ AT_Byte, AT_Float }), // NOTE: It's "Vibraiton" in-game, did they misspell vibration?
            null, // BustQuake

            null, // BustFade
            null, // BustCrossMove
            null, // BustTone
            new Instruction("BustAnime", new []{ AT_Byte, AT_Byte}), // TODO
            null, // CameraMoveXY
            null, // CameraMoveZ
            null, // CameraMoveXYZ
            null, // ScaleMode
            new Instruction("GetBgNo", new []{ AT_Int32}), // TODO
            null, // GetFadeState
            new Instruction("SetAmbiguous", new []{ AT_Float, AT_Byte, AT_Bool }),
            null, // AmbiguousPowerFade
            null, // Blur+ On/Off
            null, // BlurPowerFade
            null, // Monologue+ On/Off
            null, // Mirage+ On/Off

            null, // MiragePowerFade
            new Instruction("MessageVoiceWait", new [] { AT_Byte }), // TODO
            new Instruction("RasterScroll", new [] { AT_Int32, AT_Int32, AT_Int16 }), // TODO
            null, // RasterScrollPowerFade
            new Instruction("MesDel", null),
            new Instruction("MemoryOn", new [] { AT_Int16 }), // TODO
            new Instruction("SaveDateSet", new []{ AT_Byte, AT_String }),
            new Instruction("ExiPlay", new []{ AT_Int16, AT_Byte, AT_Byte, AT_Byte, AT_Byte, AT_Byte, AT_Int32, AT_Byte }), // TODO
            new Instruction("ExiStop", new []{ AT_Int16, AT_Byte }), // TODO
            new Instruction("GalleryFlg", new []{ AT_Int16, AT_Byte }),
            null, // DateChange
            null, // BustSpeed
            null, // DateRestNumber
            null, // MapTutorial
            null, // Ending
            null, // Set/Del +FixAuto
            null, // ExiLoopStop
            null, // ExiEndWait
            null // Set/Del +EventKeyNg
        };

        public enum ArgumentType
        {
            AT_Bool, AT_Byte, AT_Int16, AT_Int32, AT_Float, AT_String, AT_Pointer, AT_PointerArray
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

            public virtual void Write(ExtendedBinaryWriter writer, List<string> strings = null)
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
                        WriteByType(writer, ArgTypes[i], Arguments[i], strings);
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
                        case AT_Pointer:
                        case AT_Float:
                        case AT_String:
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
                    case AT_Pointer:
                        return reader.ReadInt32();
                    default:
                        return null;
                }
            }

            public static void WriteByType(ExtendedBinaryWriter writer, ArgumentType type, object value, List<string> strings = null)
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
                        writer.AddOffset($"Strings_{strings.Count}");
                        strings.Add((string)value);
                        break;
                    case AT_Pointer:
                        writer.Write((int)value);
                        break;
                }
            }
        }

        public static string ReadStringElsewhere(this ExtendedBinaryReader reader, int position = 0)
        {
            long oldPos = reader.BaseStream.Position;
            try
            {
                if (position == 0)
                    reader.JumpTo(reader.ReadInt32());
                string s = reader.ReadNullTerminatedString();
                if (position == 0)
                    reader.JumpTo(oldPos + 4);
                else
                    reader.JumpTo(oldPos);
                return s;
            }
            catch
            {
                reader.JumpTo(oldPos);
                return null;
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

            public InstructionIf() : base("if", new [] {AT_Pointer})
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

            public override void Write(ExtendedBinaryWriter writer, List<string> strings = null)
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

    }
}
