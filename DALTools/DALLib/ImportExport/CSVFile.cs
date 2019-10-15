using DALLib.Exceptions;
using DALLib.File;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.ImportExport
{
    public class STSCCSVFile : STSCImportExportBase
    {

        protected string _buffer = "";

        public override string TypeName => "Comma-Separated Values";

        public override string TypeExtension => ".csv";

        public override string Export(STSCFile script, STSCFileDatabase database)
        {
            // Header
            _buffer += " Operator , Title , Key , Translation \n";
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
                        string name = titleID == 0xFF ? "None" : database.Characters.FirstOrDefault(t => t.ID == titleID)?.FriendlyName;
                        // Add Entry to file
                        AddEntry("Message", name, instruction.GetArgument<string>(4));
                        break;
                    case "SetChoice":
                        AddEntry("Choice", "", instruction.GetArgument<string>(1));
                        break;
                    case "MapPlace":
                        AddEntry("MapMarker", "", instruction.GetArgument<string>(1));
                        break;
                    default:
                        continue;
                }
            }

            // Move the buffer onto the stack and clear the main one for next use
            string buffer = _buffer;
            _buffer = null;

            return buffer;
        }

        public override void Import(STSCFile script, STSCFileDatabase database, string file)
        {
            foreach (var line in file.Replace("\r", "").Split('\n'))
            {
                bool escape = false;
                // Split line by tabs
                string[] split = CSVSplit(line, ',');
                // Ignore blank lines
                if (line.Length == 0)
                    continue;
                // Check if all the columns exist
                if (split.Length < 3)
                    throw new InvalidFileFormatException(
                        "The TSV file being imported is missing columns. The expected columns are (Operator, Title, Key and Translation)");
                // Check if the entry has been translated
                if (string.IsNullOrEmpty(split[3]))
                    continue;
                foreach (var instruction in script.Instructions)
                {
                    switch (instruction.Name)
                    {
                        case "Mes":
                            // Check if the entry is a Message translation
                            if (split[0] != "Message")
                                break;
                            // Check if the key matches the current text
                            if (instruction.GetArgument<string>(4) == split[2])
                                instruction.Arguments[4] = split[3];
                            break;
                        case "SetChoice":
                            // Check if the entry is a Choice translation
                            if (split[0] != "Choice")
                                break;
                            // Check if the key matches the current text
                            if (instruction.GetArgument<string>(1) == split[2])
                                instruction.Arguments[1] = split[3];
                            break;
                        case "MapPlace":
                            // Check if the entry is a MapPlace translation
                            if (split[0] != "MapMarker")
                                break;
                            // Check if the key matches the current text
                            if (instruction.GetArgument<string>(1) == split[2])
                                instruction.Arguments[1] = split[3];
                            break;
                        default:
                            continue;
                    }
                }
            }
        }

        public string[] CSVSplit(string src, char delimit)
        {
            var splits = new List<string>();
            string buffer = "";
            bool escape = false;
            for (int i = 0; i < src.Length; ++i)
            {
                // Excel escaping
                if (i + 1 < src.Length && 
                    src[i + 0] == '"' && 
                    src[i + 1] == '"')
                {
                    buffer += '"';
                    ++i;
                    continue;
                }
                // Quote escaping
                if (src[i] == '"')
                {
                    escape = !escape;
                    continue;
                }
                // Next row
                if (!escape && src[i] == ',')
                {
                    splits.Add(buffer);
                    buffer = "";
                    continue;
                }
                // Add char
                buffer += src[i];
            }
            // Add buffer if there is any data, usually is the last column
            if (!string.IsNullOrEmpty(buffer))
                splits.Add(buffer);
            return splits.ToArray();
        }

        public void AddEntry(string op, string title, string key)
        {
            _buffer += $"\"{op}\",\"{title.Replace("\"", "\\\"")}\",\"{key.Replace("\"", "\\\"")}\",\n";
        }
    }
}
