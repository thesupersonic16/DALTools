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
        /// List of all the valid file types for importing anmd exporting translations
        /// </summary>
        public static TranslationBase[] FileTypes = new TranslationBase[] 
        { new TranslationTSVFile(), new TranslationCSVFile(), new TranslationPOFile() };

        public static string ExportTranslation(int fileTypeIndex, STSCFile script, STSCFileDatabase database)
        {
            var lines = new List<TranslationLine>();
            var fileType = FileTypes[fileTypeIndex];
            // The ID of who is currently speaking
            byte titleID = 0xFF;
            // Loop through all the instructions
            foreach (var instruction in script.Instructions)
            {
                switch (instruction.Name)
                {
                    case "MesTitle":
                        titleID = instruction.GetArgument<byte>(0);
                        break;
                    case "Mes":
                        // Get name of the character from 
                        string name = titleID == 0xFF ? "None" : database?.Characters.FirstOrDefault(t => t.ID == titleID)?.FriendlyName;
                        // Add Entry to file
                        lines.Add(new TranslationLine("Message", name ?? $"Unknown [{titleID}]", instruction.GetArgument<string>(2), ""));
                        break;
                    case "SetChoice":
                        lines.Add(new TranslationLine("Choice", "", instruction.GetArgument<string>(1), ""));
                        break;
                    case "MapPlace":
                        lines.Add(new TranslationLine("MapMarker", "", instruction.GetArgument<string>(1), ""));
                        break;
                    default:
                        continue;
                }
            }
            return fileType.ExportTranslation(lines.ToArray());
        }

        public static void ImportTranslation(int fileTypeIndex, STSCFile script, string data, bool useKey = true)
        {
            var fileType = FileTypes[fileTypeIndex];
            var lines = fileType.ImportTranslation(data);
            if (useKey)
            {
                for (int i = 0; i < lines.Length; ++i)
                {
                    // Skip untranslated lines
                    if (string.IsNullOrEmpty(lines[i].Translation))
                        continue;

                    foreach (var instruction in script.Instructions)
                        setSTSCLine(lines[i], instruction, false);
                }
            }
            else
            {
                int lastInstIndex = 0;
                for (int i = 0; i < lines.Length; ++i)
                {
                    for (int instIndex = lastInstIndex; instIndex < script.Instructions.Count; ++instIndex)
                    {
                        if (setSTSCLine(lines[i], script.Instructions[instIndex], true))
                        {
                            lastInstIndex = instIndex + 1;
                            break;
                        }
                    }
                }
            }
        }

        private static bool setSTSCLine(TranslationLine line, STSCInstructions.Instruction inst, bool ignoreKey)
        {
            switch (inst.Name)
            {
                case "Mes":
                    // Check if the entry is a Message translation
                    if (!string.IsNullOrEmpty(line.Operator) && line.Operator != "Message")
                        break;
                    // Check if the key matches the current text
                    if (inst.GetArgument<string>(2) == line.Key || ignoreKey)
                    {
                        inst.Arguments[2] = line.Translation;
                        return true;
                    }
                    break;
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
    }
}
