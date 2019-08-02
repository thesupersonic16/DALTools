using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STSCTool
{
    public class STSCMacros
    {
        public static string[] CharacterNames = new string[0x100];

        public static void Fill()
        {
            CharacterNames[0x00] = "CHAR_SHIDO";
            CharacterNames[0x01] = "CHAR_TOHKA";
            CharacterNames[0x02] = "CHAR_ORIGAMI";
            CharacterNames[0x03] = "CHAR_YOSHINO";
            CharacterNames[0x04] = "CHAR_KURUMI";
            CharacterNames[0x05] = "CHAR_KOTORI";
            CharacterNames[0x06] = "CHAR_RINNE";
            CharacterNames[0x07] = "CHAR_YOSHINON";
            CharacterNames[0x08] = "CHAR_REINE";
            CharacterNames[0x09] = "CHAR_KANAZUKI";
            CharacterNames[0x0A] = "CHAR_TONOMACHI";
            CharacterNames[0x0B] = "CHAR_TAMAE";
            CharacterNames[0x0C] = "CHAR_KUSAKABE";
            CharacterNames[0x0D] = "CHAR_AI";
            CharacterNames[0x0E] = "CHAR_MAI";
            CharacterNames[0x0F] = "CHAR_MII";
            CharacterNames[0x10] = "CHAR_RULER";
            CharacterNames[0x64] = "CHAR_UNKNOWN";
            CharacterNames[0x65] = "CHAR_NEWSCASTER";
            CharacterNames[0x66] = "CHAR_GIRLSTUDENTA";
            CharacterNames[0x67] = "CHAR_GIRLSTUDENTB";
            CharacterNames[0x68] = "CHAR_GUYSTUDENTA";
            CharacterNames[0x69] = "CHAR_GUYSTUDENTB";
            CharacterNames[0x6A] = "CHAR_FOODCART";
            CharacterNames[0x6B] = "CHAR_CLERK";
            CharacterNames[0x6C] = "CHAR_MALECUSTOMERA";
            CharacterNames[0x6D] = "CHAR_MALECUSTOMERB";
            CharacterNames[0x6E] = "CHAR_MALECUSTOMERC";
            CharacterNames[0x6F] = "CHAR_FEMALECUSTOMERA";
            CharacterNames[0x70] = "CHAR_FEMALECUSTOMERB";
            CharacterNames[0x71] = "CHAR_FEMALECUSTOMERC";
            CharacterNames[0x72] = "CHAR_FEMALECREWMEMBER";
            CharacterNames[0x73] = "CHAR_PARKMANAGER";
            CharacterNames[0x74] = "CHAR_TEACHER";
            CharacterNames[0x75] = "CHAR_FEMALECUSTOMER";
            CharacterNames[0x76] = "CHAR_MALECUSTOMER";
            CharacterNames[0x77] = "CHAR_REPORTER";
            CharacterNames[0x78] = "CHAR_REFEREE";
            CharacterNames[0xFF] = "CHAR_NONE";
        }
    }
}
