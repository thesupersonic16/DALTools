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
    public class TranslationTSVFile : TranslationBase
    {

        /// <summary>
        /// By RFC 4180 Standards, All line breaks should contain a CRLF which is common with Microsoft's applications
        /// </summary>
        private readonly string LINEBREAK = "\r\n";

        protected int _bufferIndex = 0;

        protected string _buffer = "";

        public override string TypeName => "Tab-Separated Values";

        public override string TypeExtension => ".tsv";

        public override TranslationLine[] ImportTranslation(string data)
        {
            var lines = new List<TranslationLine>();
            _buffer = data;
            _bufferIndex = 0;
            while (true)
            {
                // Split line by tabs
                string[] split = ReadTSVRow();
                // End of file
                if (split == null)
                    break;
                // Check if all the columns exist
                if (split.Length < 4)
                    throw new InvalidFileFormatException(
                        "The CSV file being imported is missing columns. The expected columns are (Operator, Title, Key and Translation)");

                lines.Add(new TranslationLine(split[0], split[1], split[2], split[3]));
            }
            return lines.ToArray();
        }

        public override string ExportTranslation(TranslationLine[] lines)
        {
            string data = "";
            // Header
            data += AddRow("Operator", "Comment", "Key", "Translation");

            // Fields
            foreach (var translation in lines)
                data += AddRow(translation.Operator, translation.Comment, translation.Key, translation.Translation);

            return data;
        }

        public string[] ReadTSVRow()
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
                if (!escape && _buffer[_bufferIndex] == '\t')
                {
                    splits.Add(buffer.Replace("\r", "").Replace("\\n", "\n"));
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
                    splits.Add(buffer.Replace("\r", "").Replace("\\n", "\n"));
                    break;
                }
                // Add char
                buffer += _buffer[_bufferIndex];
            }
            return splits.ToArray();
        }

        public string AddRow(params string[] fields)
        {
            for (int i = 0; i < fields.Length; ++i)
            {
                if (fields[i].Length > 0 && fields[i][0] != '"' &&
                    (fields[i].Contains("\n")))
                    fields[i] = $"\"{fields[i].Replace("\n", "\\n")}\"";
            }
            return string.Join("\t", fields) + LINEBREAK;
        }
    }
}
