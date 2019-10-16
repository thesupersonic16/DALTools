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

        /// <summary>
        /// By RFC 4180 Standards, All line breaks should contain a CRLF which is common with Microsoft's applications
        /// </summary>
        private readonly string LINEBREAK = "\r\n";

        protected string _buffer = "";

        protected int _bufferIndex = 0;

        public override string TypeName => "Comma-Separated Values";

        public override string TypeExtension => ".csv";

        public override string Export(STSCFile script, STSCFileDatabase database)
        {
            // Header
            AddRow("Operator", "Title", "Key", "Translation");
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
                        AddRow("Message", name, instruction.GetArgument<string>(4), "");
                        break;
                    case "SetChoice":
                        AddRow("Choice", "", instruction.GetArgument<string>(1), "");
                        break;
                    case "MapPlace":
                        AddRow("MapMarker", "", instruction.GetArgument<string>(1), "");
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
            _buffer = file;
            _bufferIndex = 0;
            while (true)
            {
                // Split line by tabs
                string[] split = ReadCSVRow();
                // End of file
                if (split == null)
                    break;
                // Check if all the columns exist
                if (split.Length < 4)
                    throw new InvalidFileFormatException(
                        "The CSV file being imported is missing columns. The expected columns are (Operator, Title, Key and Translation)");
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

        public string[] ReadCSVRow()
        {
            // Check if we are at the end of the file
            if (_bufferIndex >= _buffer.Length)
                return null;
            var splits = new List<string>();
            string buffer = "";
            bool escape = false;
            for (; _bufferIndex < _buffer.Length; ++_bufferIndex)
            {
                // Excel escaping
                if (_bufferIndex + 1 < _buffer.Length && 
                    _buffer[_bufferIndex + 0] == '"' && 
                    _buffer[_bufferIndex + 1] == '"')
                {
                    buffer += '"';
                    ++_bufferIndex;
                    continue;
                }
                // Quote escaping
                if (_buffer[_bufferIndex] == '"')
                {
                    escape = !escape;
                    continue;
                }
                // Next column
                if (!escape && _buffer[_bufferIndex] == ',')
                {
                    splits.Add(buffer.Replace("\n", "\\n"));
                    buffer = "";
                    continue;
                }
                // Ignore carriage returns
                if (!escape && _buffer[_bufferIndex] == '\r')
                    continue;
                // Next row
                if (!escape && _buffer[_bufferIndex] == '\n')
                {
                    _bufferIndex = _bufferIndex + 1;
                    // Escape the LF and add the last column
                    splits.Add(buffer.Replace("\n", "\\n"));
                    break;
                }
                // Add char
                buffer += _buffer[_bufferIndex];
            }
            return splits.ToArray();
        }

        public void AddRow(params string[] fields)
        {
            for (int i = 0; i < fields.Length; ++i)
            {
                // Add double-quotes if field contains an escaped line feed
                if (fields[i].Length > 0 && fields[i][0] != '"' && fields[i].Contains("\\n"))
                    fields[i] = $"\"{fields[i].Replace("\\n", LINEBREAK)}\"";
            }
            _buffer += string.Join(",", fields) + LINEBREAK;
        }

    }
}
