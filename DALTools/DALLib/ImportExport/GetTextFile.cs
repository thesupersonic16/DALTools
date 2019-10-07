using DALLib.Exceptions;
using DALLib.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.ImportExport
{
    public class GetTextFile : STSCImportExportBase
    {
        // Used to store used text to prevent duplicates
        private List<string> _usedText = new List<string>();

        protected string _buffer = "";

        public override string TypeName => "GNU gettext";

        public override string TypeExtension => ".po";

        public override string Export(STSCFile script, STSCFileDatabase database)
        {
            // Header
            WriteHeader();
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
                        AddEntry($"Message, {name}", instruction.GetArgument<string>(4));
                        break;
                    case "SetChoice":
                        AddEntry("Choice", instruction.GetArgument<string>(1));
                        break;
                    default:
                        continue;
                }
            }

            // Move the buffer onto the stack and clear the main one for next use
            string buffer = _buffer;
            _buffer = null;
            // Clear the list of used text
            _usedText.Clear();

            return buffer;
        }

        public override void Import(STSCFile script, STSCFileDatabase database, string file)
        {
            string id = "";
            foreach (var line in file.Replace("\r", "").Split('\n'))
            {
                // Ignore blank lines
                if (line.Length == 0)
                    continue;
                if (line.StartsWith("msgid"))
                    id = line.Substring(7, line.Length - 8).Replace("\\\"", "\"");
                if (line.StartsWith("msgstr"))
                {
                    string str = line.Substring(8, line.Length - 9).Replace("\\\"", "\"");
                    // Skip missing translation
                    if (string.IsNullOrEmpty(str))
                        continue;
                    foreach (var instruction in script.Instructions)
                    {
                        switch (instruction.Name)
                        {
                            case "Mes":
                                // Check if the key matches the current text
                                if (instruction.GetArgument<string>(4) == id)
                                    instruction.Arguments[4] = str;
                                break;
                            case "SetChoice":
                                // Check if the key matches the current text
                                if (instruction.GetArgument<string>(1) == id)
                                    instruction.Arguments[1] = str;
                                break;
                            default:
                                continue;
                        }
                    }
                }
            }
        }

        public void WriteHeader()
        {
            AddEntry("", "");
            AddHeaderParam("Project-Id-Version", "");
            AddHeaderParam("POT-Creation-Date", "");
            AddHeaderParam("PO-Revision-Date", "");
            AddHeaderParam("Last-Translator", "");
            AddHeaderParam("Language-Team", "");
            AddHeaderParam("MIME-Version", "1.0");
            AddHeaderParam("Content-Type", "text/plain; charset=UTF-8");
            AddHeaderParam("Content-Transfer-Encoding", "8bit");
            AddHeaderParam("Language", "en");
            _buffer += "\r\n";

            void AddHeaderParam(string key, string value)
            {
                _buffer += $"\"{key}: {value}\\n\"\r\n";
            }
        }

        public void AddEntry(string comment, string key)
        {
            if (!_usedText.Contains(key))
            {
                _usedText.Add(key);
                if (!string.IsNullOrEmpty(comment))
                    _buffer += $"#. {comment}\r\n";
                _buffer += $"msgid \"{key.Replace("\"", "\\\"")}\"\r\n";
                _buffer += $"msgstr \"\"\r\n";
                _buffer += $"\r\n";
            }
        }
    }
}
