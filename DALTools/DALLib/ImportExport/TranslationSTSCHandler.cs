using DALLib.File;
using DALLib.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DALLib.ImportExport.TranslationBase;

namespace DALLib.ImportExport
{
    public static class TranslationSTSCHandler
    {
        /// <summary>
        /// List of all the valid file types for importing and exporting translations
        /// </summary>
        public static List<TranslationBase> FileTypes = new List<TranslationBase>
        { new TranslationTSVFile(), new TranslationCSVFile(), new TranslationPOFile() };

        public static TranslationLine[] ExportTranslationLines(STSCFile script, STSCFileDatabase database, StringProcessor processor = null)
        {
            if (processor == null)
                processor = new StringProcessor();
            var lines = new List<TranslationLine>();
            // The ID or name of the title, usually who is speaking
            byte titleID = 0xFF;
            string titleName = null;
            // Loop through all the instructions
            foreach (var instruction in script.Instructions)
            {
                switch (instruction.Name)
                {
                    case "Name":
                        titleName = instruction.GetArgument<string>(0);
                        if (titleName.Length == 0)
                            titleName = null;
                        break;
                    case "MesTitle":
                        titleID = instruction.GetArgument<byte>(0);
                        break;
                    case "Message":
                    case "Mes":
                        // Get name of the character from 
                        string name = titleName ?? (titleID == 0xFF ? "None" : database?.Characters.FirstOrDefault(t => t.ID == titleID)?.FriendlyName);
                        // Add Entry to file
                        lines.Add(new TranslationLine("Message", name ?? $"Unknown [{titleID}]", instruction.GetArgument<string>(2)));
                        break;
                    case "Choice":
                    case "SetChoice":
                        lines.Add(new TranslationLine("Choice", "", instruction.GetArgument<string>(1)));
                        break;
                    case "MapPlace":
                        lines.Add(new TranslationLine("MapMarker", "", instruction.GetArgument<string>(1)));
                        break;
                    default:
                        continue;
                }
            }
            lines.ForEach(t =>
            {
                t.Comment = processor.Process(t.Comment);
                t.Key = processor.Process(t.Key);
            });
            return lines.ToArray();
        }

        public static TranslationLine[] ExportTranslationLines(STSC2File script, Dictionary<short, string> names, StringProcessor processor = null)
        {
            if (processor == null)
                processor = new StringProcessor();
            var lines = new List<TranslationLine>();
            
            short nameID = 0;
            
            // Loop through all the lines
            foreach (var line in script.Sequence.Lines)
            {
                switch (line.Name)
                {
                    case "Name":
                        //titleName = line.GetArgument<string>(0);
                        //if (titleName.Length == 0)
                        //    titleName = null;
                        //break;
                    case "Name / NameOff":
                        nameID = (short)(int)(line.arguments[0] as STSC2Node).Value;
                        break;
                    case "Mes":
                        // Add Entry to file
                        string name = names.ContainsKey(nameID) ? names[nameID] : $"Unknown 0x{nameID:X2}";
                        lines.Add(new TranslationLine("Message", name, line.arguments[3] as string));
                        break;
                    case "Choice":
                        lines.Add(new TranslationLine("Choice", "", line.arguments[1] as string));
                        break;
                    case "MapPlace":
                        //lines.Add(new TranslationLine("MapMarker", "", line.GetArgument<string>(1)));
                        break;
                    default:
                        continue;
                }
            }
            lines.ForEach(t =>
            {
                t.Comment = processor.Process(t.Comment);
                t.Key = processor.Process(t.Key);
            });
            return lines.ToArray();
        }

        public static string ExportTranslation(int fileTypeIndex, STSCFile script, STSCFileDatabase database, StringProcessor processor = null)
        {
            var fileType = FileTypes[fileTypeIndex];
            return fileType.ExportTranslation(ExportTranslationLines(script, database, processor));
        }

        public static string ExportTranslation(int fileTypeIndex, STSC2File script, Dictionary<short, string> names, StringProcessor processor = null)
        {
            var fileType = FileTypes[fileTypeIndex];
            return fileType.ExportTranslation(ExportTranslationLines(script, names, processor));
        }

        public static void ImportTranslation(int fileTypeIndex, STSCFile script, string data, bool useKey = true, StringProcessor processor = null)
        {
            var fileType = FileTypes[fileTypeIndex];
            var lines = fileType.ImportTranslation(data);
            ImportTranslation(lines, script, useKey, processor);
        }

        public static void ImportTranslation(int fileTypeIndex, STSC2File script, string data, bool useKey = true, StringProcessor processor = null)
        {
            var fileType = FileTypes[fileTypeIndex];
            var lines = fileType.ImportTranslation(data);
            ImportTranslation(lines, script, useKey, processor);
        }

        public static void ImportTranslation(TranslationLine[] lines, STSCFile script, bool useKey = true, StringProcessor processor = null)
        {
            if (processor == null)
                processor = new StringProcessor();

            foreach (var line in lines)
            {
                line.Comment = processor.ProcessReverse(line.Comment);
                line.Key = processor.ProcessReverse(line.Key);
                line.Translation = processor.ProcessReverse(line.Translation);
            }

            if (useKey)
            {
                for (int i = 0; i < lines.Length; ++i)
                {
                    // Skip untranslated lines
                    if (string.IsNullOrEmpty(lines[i].Translation))
                        continue;

                    for (int ii = 0; ii < script.Instructions.Count; ++ii)
                        SetSTSCLine(lines[i], script.Instructions, ii, false);
                }
            }
            else
            {
                int lastInstIndex = 0;
                for (int i = 0; i < lines.Length; ++i)
                {
                    for (int instIndex = lastInstIndex; instIndex < script.Instructions.Count; ++instIndex)
                    {
                        if (SetSTSCLine(lines[i], script.Instructions, instIndex, true))
                        {
                            lastInstIndex = instIndex + 1;
                            break;
                        }
                    }
                }
            }
        }

        public static void ImportTranslation(TranslationLine[] lines, STSC2File script, bool useKey = true, StringProcessor processor = null)
        {
            if (processor == null)
                processor = new StringProcessor();

            foreach (var line in lines)
            {
                line.Comment = processor.ProcessReverse(line.Comment);
                line.Key = processor.ProcessReverse(line.Key);
                line.Translation = processor.ProcessReverse(line.Translation);
            }

            if (useKey)
            {
                for (int i = 0; i < lines.Length; ++i)
                {
                    // Skip untranslated lines
                    if (string.IsNullOrEmpty(lines[i].Translation))
                        continue;

                    for (int ii = 0; ii < script.Sequence.Lines.Count; ++ii)
                        SetSTSCLine(lines[i], script.Sequence.Lines, ii, false);
                }
            }
            else
            {
                int lastInstIndex = 0;
                for (int i = 0; i < lines.Length; ++i)
                {
                    for (int instIndex = lastInstIndex; instIndex < script.Sequence.Lines.Count; ++instIndex)
                    {
                        if (SetSTSCLine(lines[i], script.Sequence.Lines, instIndex, true))
                        {
                            lastInstIndex = instIndex + 1;
                            break;
                        }
                    }
                }
            }
        }

        private static bool SetSTSCLine(TranslationLine line, List<STSCInstructions.Instruction> instructions, int index, bool ignoreKey)
        {
            var inst = instructions[index];
            switch (inst.Name)
            {
                case "Message":
                case "Mes":
                    // Check if the entry is a Message translation
                    if (!string.IsNullOrEmpty(line.Operator) && line.Operator != "Message")
                        break;
                    // Check if the key matches the current text
                    if (inst.GetArgument<string>(2) == line.Key || ignoreKey)
                    {
                        inst.Arguments[2] = line.Translation;

                        if (index <= 2) return true;
                        for (int i = index; i > index - 3; --i)
                            if (instructions[i].Name == "Name")
                            {
                                instructions[i].Arguments[0] = line.Comment;
                                return true;
                            }
                        return true;
                    }
                    break;
                case "Choice":
                case "SetChoice":
                    // Check if the entry is a Choice translation
                    if (!string.IsNullOrEmpty(line.Operator) && line.Operator != "Choice")
                        break;
                    // Check if the key matches the current text
                    if (inst.GetArgument<string>(1) == line.Key || ignoreKey)
                    {
                        inst.Arguments[1] = line.Translation;
                        return true;
                    }
                    break;
                case "MapPlace":
                    // Check if the entry is a MapPlace translation
                    if (!string.IsNullOrEmpty(line.Operator) && line.Operator != "MapMarker")
                        break;
                    // Check if the key matches the current text
                    if (inst.GetArgument<string>(1) == line.Key || ignoreKey)
                    {
                        inst.Arguments[1] = line.Translation;
                        return true;
                    }
                    break;
                default:
                    break;
            }
            return false;
        }

        private static bool SetSTSCLine(TranslationLine line, List<STSC2Commands.Command> lines, int index, bool ignoreKey)
        {
            var inst = lines[index];
            switch (inst.Name)
            {
                case "Mes":
                    // Check if the entry is a Message translation
                    if (!string.IsNullOrEmpty(line.Operator) && line.Operator != "Message")
                        break;
                    // Check if the key matches the current text
                    if (inst.arguments[3] as string == line.Key || ignoreKey)
                    {
                        inst.arguments[3] = line.Translation;

                        if (index <= 2) return true;
                        for (int i = index; i > index - 3; --i)
                            if (lines[i].Name == "Name") // TODO
                            {
                                lines[i].arguments[0] = line.Comment;
                                return true;
                            }
                        return true;
                    }
                    break;
                case "Choice":
                    // Check if the entry is a Choice line
                    if (!string.IsNullOrEmpty(line.Operator) && line.Operator != "Choice")
                        break;
                    // Check if the key matches the current text
                    if (inst.arguments[1] as string == line.Key || ignoreKey)
                    {
                        inst.arguments[1] = line.Translation;
                        return true;
                    }
                    break;
                case "MapPlace":
                    //// Check if the entry is a MapPlace translation
                    //if (!string.IsNullOrEmpty(line.Operator) && line.Operator != "MapMarker")
                    //    break;
                    //// Check if the key matches the current text
                    //if (inst.GetArgument<string>(1) == line.Key || ignoreKey)
                    //{
                    //    inst.Arguments[1] = line.Translation;
                    //    return true;
                    //}
                    break;
                default:
                    break;
            }
            return false;
        }

    }
}
