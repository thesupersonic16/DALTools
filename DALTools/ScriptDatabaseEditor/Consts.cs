using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptDatabaseEditor
{
    public static class Consts
    {

        /// <summary>
        /// Convert Voice ID to Frame IDs for option.tex.
        /// Not sure why they are out of order.
        /// </summary>
        public static int[] VOICETOFRAMEID = new [] { 22, 10, 06, 11, 03, 01, 16, 15, 14, 20, 21, 23,
                                                      18, 19, 12, 07, 09, 08, 00, 04, 05, 13, 17, 02};
        public static string[] GAMEDIRNAME = new[] { "", "1st", "2nd", "3rd" };
    }
}
