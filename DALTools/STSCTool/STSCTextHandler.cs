using System;
using System.Collections.Generic;
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
        public static string[] ConvertToText(STSCFile file)
        {
            var labels = new Dictionary<string, int>();
            var strings = new List<string>();
            var scopeEnds = new List<int>();
            int currentIndent = 0;
            int address = 0x3C;
            strings.Add($"#scriptID 0x{file.ScriptID:X8}");
            strings.Add($"#scriptName {file.ScriptName}");
            for (int i = 0; i < file.Instructions.Count; ++i)
            {
                var instruction = file.Instructions[i];
                if (scopeEnds.Count != 0 && scopeEnds.Last() == address)
                {
                    --currentIndent;
                    strings.Add($"{new string(' ', currentIndent * 4)}}}");
                    scopeEnds.RemoveAt(scopeEnds.Count - 1);
                }
                if (labels.Any(t =>  t.Value == address))
                    strings.Add($"#label {labels.FirstOrDefault(t => t.Value == address).Key}");
                if (instruction.Name != "if")
                {
                    string[] argString = instruction.Arguments.Select((arg, index) =>
                    {
                        if (instruction.ArgTypes[index] == STSCInstructions.ArgumentType.AT_Pointer)
                        {
                            int jumpAddress = instruction.GetArgument<int>(index);
                            string labelName = $"LABEL_{jumpAddress:X4}";
                            if (!labels.ContainsKey(labelName))
                                labels.Add(labelName, jumpAddress);
                            return labelName;
                        }
                        return ConvertArgumentToString(instruction, index);
                    }).ToArray();
                    strings.Add($"{new string(' ', currentIndent * 4)}{instruction.Name}({string.Join(", ", argString)})");
                }
                else
                {
                    var ifInstruction = instruction as STSCInstructions.InstructionIf;
                    string[] comps = ifInstruction.Comparisons
                        .Select((comparison, index) => ConvertComparisonToString(comparison)).ToArray();
                    strings.Add($"{new string(' ', currentIndent * 4)}if ({string.Join(" && ", comps)})");
                    strings.Add($"{new string(' ', currentIndent * 4)}{{");
                    ++currentIndent;
                    scopeEnds.Add(ifInstruction.GetArgument<int>(0));
                }
                address += instruction.GetInstructionSize();
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

                if (baseInstruction.Name != "if")
                {
                    var instruction = (STSCInstructions.Instruction) Activator.CreateInstance(baseInstruction.GetType(), baseInstruction.Name,
                        baseInstruction.ArgTypes);
                    if (baseInstruction.ArgTypes != null)
                        for (int ii = 0; ii < instruction.ArgTypes.Length; ++ii)
                            AddArgument(instruction, instruction.ArgTypes[ii], code[ii + 1]);
                    file.Instructions.Add(instruction);
                    address += instruction.GetInstructionSize();
                }
                else
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
            }

            file.Instructions.ForEach(t => 
            {
                if (t.ArgTypes != null)
                    for (int i = 0; i < t.ArgTypes.Length; ++i)
                        if (t.ArgTypes[i] == STSCInstructions.ArgumentType.AT_Pointer && t.Arguments[i].GetType() == typeof(string))
                            t.Arguments[i] = labels[t.Arguments[i] as string];
            });
        }

        public static string[] PreprocessLabels(string[] text)
        {
            var labels = new List<string>();
            for (int i = 0; i < text.Length; ++i)
            {
                if (string.IsNullOrEmpty(text[i]))
                    continue;
                if (text[i][0] != '#')
                    continue;
                string line = text[i].Substring(1);
                if (line.StartsWith("label"))
                {
                    labels.Add(line.Substring(6));
                }
            }

            return labels.ToArray();
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
                case STSCInstructions.ArgumentType.AT_Float:
                    return $"{instruction.GetArgument<float>(index)}f";
                case STSCInstructions.ArgumentType.AT_String:
                    return $"\"{instruction.GetArgument<string>(index)}\"";
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
            if (s.StartsWith("0x") && int.TryParse(s.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int number))
                return number.ToString();
            if (s.EndsWith("f") && float.TryParse(s.Substring(0, s.Length - 1), out float floatResult))
                return floatResult.ToString(CultureInfo.InvariantCulture);
            return s;
        }


        public static string[] ComparisonOperatorStrings = new[] {"==", "!=", "<=", "<", ">=", ">", "!= 0", "== 0"};
    }
}
