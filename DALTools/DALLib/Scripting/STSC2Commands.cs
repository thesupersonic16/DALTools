using DALLib.IO;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static DALLib.Scripting.STSCInstructions;

namespace DALLib.Scripting
{
    public static partial class STSC2Commands
    {

        public class Command
        {
            public string Name { get; set; }
            public Type[] ArgumentTypes;
            public List<object> arguments = new List<object>();

            private byte[] m_CmdLineInfo = new byte[0];

            public Command(string name, Type[] arguments)
            {
                Name = name;
                ArgumentTypes = arguments;
            }

            public virtual Command Read(ExtendedBinaryReader reader, byte[] cmdLineInfo)
            {
                var line = new Command(Name, ArgumentTypes);
                line.m_CmdLineInfo = cmdLineInfo;

                if (ArgumentTypes != null)
                {
                    foreach (var arg in ArgumentTypes)
                    {
                        if (arg == typeof(STSC2Node))
                            line.arguments.Add(new STSC2Node().Read(reader));
                        else line.arguments.Add(reader.ReadByType(arg));
                    }
                }

                return line;
            }

            public virtual void Write(ExtendedBinaryWriter writer, List<string> stringTable, List<STSC2Node> nodes, List<Command> commands)
            {
                if (m_CmdLineInfo != null)
                    writer.Write(m_CmdLineInfo);

                // Opcode
                writer.Write((byte)commands.FindIndex(x => x.Name == Name));

                if (ArgumentTypes != null)
                {
                    foreach (var arg in arguments)
                    {
                        if (arg is STSC2Node node)
                        {
                            writer.AddOffset($"node{nodes.Count}");
                            nodes.Add(node);
                        }
                        else
                        {
                            if (arg is string str)
                            {
                                writer.AddOffset($"str{stringTable.Count}");
                                stringTable.Add(str);
                            }
                            else
                                writer.WriteByType(arg);
                        }
                    }
                }
            }


            public override string ToString()
            {
                return Name;
            }

        }

        public class CommandFuncCall : Command
        {
            public CommandFuncCall() : base("FuncCall", null)
            {

            }

            public override Command Read(ExtendedBinaryReader reader, byte[] cmdLineInfo)
            {
                // Workaround
                reader.JumpAhead(0x04);
                byte paramCount = reader.ReadByte();
                reader.JumpBehind(5);

                var args = new List<Type>() { typeof(uint), typeof(byte), typeof(byte), typeof(long) };  

                for (int i = 0; i < paramCount; i++)
                    args.AddRange(new Type[] { typeof(byte), typeof(STSC2Node) });

                args.Add(typeof(int));
                ArgumentTypes = args.ToArray();

                return base.Read(reader, cmdLineInfo);
            }

        }

        public class CommandReturn : Command
        {
            public CommandReturn() : base("Return", null)
            {

            }

            public override Command Read(ExtendedBinaryReader reader, byte[] cmdLineInfo)
            {
                // Workaround
                byte paramType = reader.ReadByte();
                reader.JumpBehind(1);

                Type readType = null;

                if (paramType == 0)
                    readType = typeof(int);
                else if (paramType == 1)
                    readType = typeof(float);
                else if (paramType == 2)
                    readType = typeof(bool);
                else if (paramType == 3)
                    readType = typeof(string);

                if (readType == null)
                    ArgumentTypes = new Type[] { typeof(byte) };
                else
                    ArgumentTypes = new Type[] { typeof(byte), readType }; 

                return base.Read(reader, cmdLineInfo);
            }

        }

        public class CommandJumpSwitch : Command
        {
            public CommandJumpSwitch() : base("JumpSwitch", null)
            {

            }

            public override Command Read(ExtendedBinaryReader reader, byte[] cmdLineInfo)
            {
                // Workaround
                reader.JumpAhead(0x04);
                int count = reader.ReadUInt16() & 0x7FF;
                reader.JumpBehind(6);

                var args = new List<Type>() { typeof(STSC2Node), typeof(ushort) };

                for (int i = 0; i < count; i++)
                    args.AddRange(new Type[] { typeof(long), typeof(int) });

                ArgumentTypes = args.ToArray();

                return base.Read(reader, cmdLineInfo);
            }
        }

        public class CommandSubStart : Command
        {
            public CommandSubStart() : base("SubStart", null)
            {

            }

            public override Command Read(ExtendedBinaryReader reader, byte[] cmdLineInfo)
            {
                // Workaround
                reader.JumpAhead(5);
                byte count = reader.ReadByte();
                reader.JumpBehind(6);

                var args = new List<Type>() { typeof(byte), typeof(int), typeof(byte), typeof(ulong), typeof(byte) };

                for (int i = 0; i < count; i++)
                    args.AddRange(new Type[] { typeof(byte), typeof(STSC2Node) });

                args.Add(typeof(uint));

                ArgumentTypes = args.ToArray();

                return base.Read(reader, cmdLineInfo);
            }
        }
    }
}
