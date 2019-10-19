using DALLib.Exceptions;
using DALLib.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.ImportExport
{
    public class TranslationPOFile : TranslationBase
    {
        // Used to store used text to prevent duplicates
        private List<string> _usedText = new List<string>();

        protected string _buffer = "";

        public override string TypeName => "GNU gettext";

        public override string TypeExtension => ".po";

        public override TranslationLine[] ImportTranslation(string data)
        {
            var lines = new List<TranslationLine>();
            string id = "";
            string str = "";
            foreach (var line in data.Replace("\r", "").Split('\n'))
            {
                // Ignore blank lines
                if (line.Length == 0)
                    continue;

                if (line.StartsWith("msgid"))
                    id = line.Substring(7, line.Length - 8).Replace("\\\"", "\"");
                if (line.StartsWith("msgstr"))
                   str = line.Substring(8, line.Length - 9).Replace("\\\"", "\"");

                lines.Add(new TranslationLine("", "", id, str));
            }
            return lines.ToArray();
        }

        public override string ExportTranslation(TranslationLine[] lines)
        {
            // Header
            WriteHeader();

            // Fields
            foreach (var translation in lines)
                AddEntry(translation.Comment, translation.Key);

            return FinaliseExport();
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

        public string FinaliseExport()
        {
            // Move the buffer onto the stack and clear the main one for next use
            string buffer = _buffer;
            _buffer = null;
            // Clear the list of used text
            _usedText.Clear();
            return buffer;
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
