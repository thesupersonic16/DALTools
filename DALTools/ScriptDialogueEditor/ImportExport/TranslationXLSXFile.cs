using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NanoXLSX;

namespace DALLib.ImportExport
{
    public static class TranslationXLSXFile
    {

        public static Worksheet ExportWorksheet(TranslationBase.TranslationLine[] lines)
        {
            Worksheet sheet = new Worksheet();
            // Header
            sheet.AddNextCell("Operator");
            sheet.AddNextCell("Comment");
            sheet.AddNextCell("Key");
            sheet.AddNextCell("Translation");
            sheet.GoToNextRow();

            // Fields
            foreach (var translation in lines)
            {
                sheet.AddNextCell(translation.Operator);
                sheet.AddNextCell(translation.Comment);
                sheet.AddNextCell(translation.Key);
                sheet.AddNextCell(translation.Translation);
                sheet.GoToNextRow();
            }

            return sheet;
        }

        public static TranslationBase.TranslationLine[] ImportWorksheet(Worksheet sheet)
        {
            var lines = new List<TranslationBase.TranslationLine>();

            int row = 0;
            while (sheet.HasCell(0, row + 1))
            {
                string op = sheet.GetCell(0, row + 1).Value.ToString();
                string comment = sheet.GetCell(1, row + 1).Value.ToString();
                string key = sheet.GetCell(2, row + 1).Value.ToString();
                string transla = sheet.GetCell(3, row + 1).Value.ToString();
                lines.Add(new TranslationBase.TranslationLine(op, comment, key, transla));
                ++row;
            }
            return lines.ToArray();
        }
    }
}