using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HedgeLib.IO;
using static STSCTool.STSCInstructions.InstructionIf.Comparison;

namespace STSCTool
{
    public class STSCTextHandler
    {

        public static int FindAddress(STSCFile file, int index)
        {
            int address = 0;
            for (int i = 0; i < index; ++i)
                address += file.Instructions[i].GetInstructionSize();
            return address;
        }

        public static int FindIndex(STSCFile file, int address)
        {
            int tempAddress = 0x3C;
            for (int i = 0; i < file.Instructions.Count; ++i)
            {
                if (tempAddress == address)
                    return i;
                tempAddress += file.Instructions[i].GetInstructionSize();
            }
            return 0;
        }

        public static void PlaceLine(STSCFile file, List<string> strings, List<int> lines, int address, string s)
        {
            int index = FindIndex(file, address);
            strings.Insert(lines[index], s);
            for (int i = index; i < lines.Count; ++i)
                ++lines[i];
        }

        public static void ConvertSingleInstructionToText(STSCFile file, int instructionPointer,
            Dictionary<string, int> labels, List<int> scopeEnds, List<int> lines, List<string> strings, ref int address, ref int currentIndent)
        {
            var instruction = file.Instructions[instructionPointer];
            if (scopeEnds.Count != 0 && scopeEnds.Last() == address)
            {
                --currentIndent;
                strings.Add($"{new string(' ', currentIndent * 4)}}}");
                scopeEnds.RemoveAt(scopeEnds.Count - 1);
            }
            // TODO
            int address2 = address;
            if (labels.Any(t => t.Value == address2))
                strings.Add($"#label {labels.FirstOrDefault(t => t.Value == address2).Key}");
            if (instruction.Name == "if")
            {
                lines.Add(strings.Count);
                var ifInstruction = instruction as STSCInstructions.InstructionIf;
                string[] comps = ifInstruction.Comparisons
                    .Select((comparison, index) => ConvertComparisonToString(comparison)).ToArray();
                strings.Add($"{new string(' ', currentIndent * 4)}if ({string.Join(" && ", comps)})");
                strings.Add($"{new string(' ', currentIndent * 4)}{{");
                ++currentIndent;
                scopeEnds.Add(ifInstruction.GetArgument<int>(0));
            }
            else
            {
                string[] argString = instruction.Arguments.Select((arg, index) =>
                {
                    if (instruction.ArgTypes[index] == STSCInstructions.ArgumentType.AT_Pointer)
                    {
                        int jumpAddress = instruction.GetArgument<int>(index);
                        string labelName = $"LABEL_{jumpAddress:X4}";
                        // Change Label name if its been used as a function
                        if (instruction.Name == STSCInstructions.Instructions[0x1A].Name)
                            labelName = $"SUB_{jumpAddress:X4}";

                        if (!labels.ContainsKey(labelName))
                        {
                            labels.Add(labelName, jumpAddress);
                            if (FindIndex(file, jumpAddress) < instructionPointer)
                                PlaceLine(file, strings, lines, jumpAddress, $"#label {labelName}");
                        }
                        return labelName;
                    }
                    return ConvertArgumentToString(instruction, index);
                }).ToArray();
                // Macros
                if (instruction.Name == STSCInstructions.Instructions[0x52].Name)
                    argString[0] = STSCMacros.CharacterNames[int.Parse(argString[0])] ?? argString[0];
                lines.Add(strings.Count);
                strings.Add($"{new string(' ', currentIndent * 4)}{instruction.Name}({string.Join(", ", argString)})");
            }
        }

        public static string[] ConvertToText(STSCFile file)
        {
            var labels = new Dictionary<string, int>();
            var strings = new List<string>();
            var scopeEnds = new List<int>();
            var lines = new List<int>();
            int currentIndent = 0;
            int address = 0x3C;
            strings.Add($"#scriptID 0x{file.ScriptID:X8}");
            strings.Add($"#scriptName {file.ScriptName}");
            for (int i = 0; i < file.Instructions.Count; ++i)
            {
                var instruction = file.Instructions[i];
                ConvertSingleInstructionToText(file, i, labels, scopeEnds, lines, strings, ref address, ref currentIndent);
                address += instruction.GetInstructionSize();
            }
            // Return Scope back to the start
            while (currentIndent != 0)
            {
                --currentIndent;
                strings.Add($"{new string(' ', currentIndent * 4)}}}");
                scopeEnds.RemoveAt(scopeEnds.Count - 1);
            }
            return strings.ToArray();
        }

        public static void ConvertToObject(STSCFile file, string[] text)
        {
            var labels = new Dictionary<string, int>();
            var scopes = new List<int>();
            int scopeID = 0;
            int address = 0x3C;
            for (int i = 0; i < text.Length; ++i)
            {
                string line = text[i];
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line[0] == '#')
                {
                    if (line.StartsWith("#label"))
                        labels.Add(line.Substring(7), address);
                    if (line.StartsWith("#scriptID"))
                        file.ScriptID = uint.Parse(ProcessLiterals(line.Substring(10)));
                    if (line.StartsWith("#scriptName"))
                        file.ScriptName = line.Substring(12);
                    continue;
                }
                if (line[0] == '{')
                    continue;
                if (line[0] == '}')
                {
                    labels.Add(scopes.Last().ToString(), address);
                    scopes.RemoveAt(scopes.Count - 1);
                    continue;
                }
                var code = ParseCodeLine(line);
                var baseInstruction = STSCInstructions.Instructions.FirstOrDefault(t => t?.Name == code[0]);
                if (baseInstruction == null)
                {
                    Console.WriteLine("Error: Could not find any instructions for \"{0}\"! Please check line {1} in the source file.", code[0], i);
                    Console.ReadKey(true);
                    return;
                }

                if (baseInstruction.Name == "if")
                {
                    var baseIf = baseInstruction as STSCInstructions.InstructionIf;
                    var instruction = new STSCInstructions.InstructionIf();
                    instruction.Arguments.Add(scopeID.ToString());
                    scopes.Add(scopeID++);
                    var comStrs = code[1].Split(new [] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in comStrs)
                    {
                        foreach (string cmpstr in ComparisonOperatorStrings)
                            if (s.Contains(cmpstr))
                            {
                                var split = s.Split(new[] { cmpstr }, StringSplitOptions.RemoveEmptyEntries);
                                var cmp = new STSCInstructions.InstructionIf.Comparison(
                                    uint.Parse(ProcessLiterals(split[0])),
                                    (ComparisonOperators) Array.IndexOf(ComparisonOperatorStrings, cmpstr),
                                    uint.Parse(ProcessLiterals(split[1])));
                                instruction.Comparisons.Add(cmp);
                                break;
                            }
                    }
                    file.Instructions.Add(instruction);
                    address += instruction.GetInstructionSize();
                }
                else if (baseInstruction.Name == "switch")
                {
                    var baseSwitch = baseInstruction as STSCInstructions.InstructionSwitch;
                    var instruction = new STSCInstructions.InstructionSwitch(baseSwitch.Name, null);
                    uint unknown = (uint)int.Parse(code[1]);
                    ushort amount = ushort.Parse(code[2]);
                    bool endFlag = bool.Parse(code[3]);
                    
                    instruction.ArgTypes = new STSCInstructions.ArgumentType[amount * 2 + 3];
                    instruction.ArgTypes[0] = STSCInstructions.ArgumentType.AT_DataReference;
                    instruction.Arguments.Add(unknown);
                    instruction.ArgTypes[1] = STSCInstructions.ArgumentType.AT_Int16;
                    instruction.Arguments.Add(amount);
                    instruction.ArgTypes[2] = STSCInstructions.ArgumentType.AT_Bool;
                    instruction.Arguments.Add(endFlag);
                    for (int ii = 0; ii < amount; ++ii)
                    {
                        // case
                        instruction.ArgTypes[ii * 2 + 3 + 0] = STSCInstructions.ArgumentType.AT_Int32;
                        instruction.Arguments.Add(int.Parse(ProcessLiterals(code[ii * 2 + 4 + 0])));
                        // location
                        instruction.ArgTypes[ii * 2 + 3 + 1] = STSCInstructions.ArgumentType.AT_Pointer;
                        instruction.Arguments.Add(code[ii * 2 + 4 + 1]);
                    }
                    file.Instructions.Add(instruction);
                    address += instruction.GetInstructionSize();
                }
                else
                {
                    var instruction = (STSCInstructions.Instruction)Activator.CreateInstance(baseInstruction.GetType(), baseInstruction.Name,
                        baseInstruction.ArgTypes);
                    if (baseInstruction.ArgTypes != null)
                        for (int ii = 0; ii < instruction.ArgTypes.Length; ++ii)
                            AddArgument(instruction, instruction.ArgTypes[ii], code[ii + 1]);
                    file.Instructions.Add(instruction);
                    address += instruction.GetInstructionSize();

                }
            }

            file.Instructions.ForEach(t => 
            {
                if (t.ArgTypes != null)
                    for (int i = 0; i < t.ArgTypes.Length; ++i)
                        if (t.ArgTypes[i] == STSCInstructions.ArgumentType.AT_Pointer && t.Arguments[i].GetType() == typeof(string))
                            t.Arguments[i] = labels[t.Arguments[i] as string];
            });
        }

        public static string ConvertArgumentToString(STSCInstructions.Instruction instruction, int index)
        {
            switch (instruction.ArgTypes[index])
            {
                case STSCInstructions.ArgumentType.AT_Bool:
                    return instruction.GetArgument<bool>(index) ? "true" : "false";
                case STSCInstructions.ArgumentType.AT_Byte:
                case STSCInstructions.ArgumentType.AT_Int16:
                case STSCInstructions.ArgumentType.AT_Int32:
                case STSCInstructions.ArgumentType.AT_Pointer:
                    int num = instruction.GetArgument<int>(index);
                    if (num > 2048 || num < -2048)
                        return $"0x{num:X}";
                    else
                        return num.ToString();
                case STSCInstructions.ArgumentType.AT_DataReference:
                    return $"0x{instruction.GetArgument<uint>(index):X8}";
                case STSCInstructions.ArgumentType.AT_Float:
                    return $"{instruction.GetArgument<float>(index)}f";
                case STSCInstructions.ArgumentType.AT_String:
                    return $"\"{instruction.GetArgument<string>(index).Replace("\"", "\\\"")}\"";
            }
            return "";
        }
        public static void AddArgument(STSCInstructions.Instruction instruction, STSCInstructions.ArgumentType type, string value)
        {
            switch (type)
            {
                case STSCInstructions.ArgumentType.AT_Bool:
                    instruction.Arguments.Add(value != "false");
                    return;
                case STSCInstructions.ArgumentType.AT_Byte:
                    instruction.Arguments.Add(byte.Parse(value));
                    return;
                case STSCInstructions.ArgumentType.AT_Int16:
                    instruction.Arguments.Add(short.Parse(value));
                    return;
                case STSCInstructions.ArgumentType.AT_Int32:
                    instruction.Arguments.Add(int.Parse(value));
                    return;
                case STSCInstructions.ArgumentType.AT_Pointer:
                    instruction.Arguments.Add(value);
                    return;
                case STSCInstructions.ArgumentType.AT_DataReference:
                    instruction.Arguments.Add(uint.Parse(value));
                    return;
                case STSCInstructions.ArgumentType.AT_Float:
                    instruction.Arguments.Add(float.Parse(value));
                    return;
                case STSCInstructions.ArgumentType.AT_String:
                    instruction.Arguments.Add(value);
                    return;
            }
        }

        public static string ConvertComparisonToString(STSCInstructions.InstructionIf.Comparison comparison)
        {
            string left = comparison.Left.ToString();
            string right = comparison.Right.ToString();
            if (comparison.Left > 2048)
                left = $"0x{comparison.Left:X}";
            if (comparison.Right > 2048)
                right = $"0x{comparison.Right:X}";
            if (comparison.Operator != ComparisonOperators.NotZero && comparison.Operator != ComparisonOperators.Zero)
                return $"{left} {ComparisonOperatorStrings[(int) comparison.Operator]} {right}";
            return $"{left} {ComparisonOperatorStrings[(int) comparison.Operator]}";
        }

        public static List<string> ParseCodeLine(string codeLine)
        {
            bool insideString = false;
            bool escaped = false;
            string buffer = "";
            string bufferString = "";
            List<string> strings = new List<string>();
            for (int i = 0; i < codeLine.Length; ++i)
            {
                if (codeLine[i] == '\\')
                    escaped = true;
                if (escaped)
                {
                    if (codeLine[i + 1] != '"')
                        bufferString += codeLine[i];
                    bufferString += codeLine[++i];
                    escaped = false;
                    continue;
                }

                if (!insideString && codeLine[i] == ' ')
                    continue;

                if (!insideString && codeLine[i] == '(')
                {
                    if (buffer.Length > 0)
                        strings.Add(ProcessLiterals(buffer));
                    buffer = "";
                    continue;
                }
                if (!insideString && codeLine[i] == ')')
                {
                    if (buffer.Length > 0)
                        strings.Add(ProcessLiterals(buffer));
                    buffer = "";
                    ++i;
                    continue;
                }

                if (!insideString && codeLine[i] == ',')
                {
                    if (buffer.Length > 0)
                        strings.Add(ProcessLiterals(buffer));
                    buffer = "";
                    continue;
                }

                if (codeLine[i] == '"')
                {
                    if (insideString)
                    {
                        insideString = false;
                        buffer += bufferString;
                    }
                    else
                        insideString = true;
                    continue;
                }
                if (insideString)
                    bufferString += codeLine[i];
                else
                    buffer += codeLine[i];
            }
            if (!string.IsNullOrEmpty(buffer))
                strings.Add(buffer);
            return strings;

        }

        public static string ProcessLiterals(string s)
        {
            int number = 0;
            if ((number = Array.IndexOf(STSCMacros.CharacterNames, s)) != -1)
                return number.ToString();
            if (s.StartsWith("0x") && int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out number))
                return number.ToString();
            if (s.EndsWith("f") && float.TryParse(s.Substring(0, s.Length - 1), out float floatResult))
                return floatResult.ToString(CultureInfo.InvariantCulture);
            return s;
        }


        public static string[] ComparisonOperatorStrings = new[] {"==", "!=", "<=", "<", ">=", ">", "!= 0", "== 0"};
    }
}
