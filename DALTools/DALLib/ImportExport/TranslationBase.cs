using DALLib.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.ImportExport
{
    public abstract class TranslationBase
    {
        /// <summary>
        /// Name of the file type
        /// </summary>
        public abstract string TypeName { get; }
        /// <summary>
        /// The extension of the file type, must be formatted with a fullstop following with the extension e.g. ".bin"
        /// </summary>
        public abstract string TypeExtension { get; }

        public abstract TranslationLine[] ImportTranslation(string data);
        public abstract string ExportTranslation(TranslationLine[] lines);

        public class TranslationLine
        {
            public string Operator;
            public string Comment;
            public string Key;
            public string Translation;

            public TranslationLine(string @operator, string comment, string key, string translation)
            {
                Operator = @operator;
                Comment = comment;
                Key = key;
                Translation = translation;
            }
        }
    }
}
