using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.Scripting
{
    public class STSCMacros
    {
        public static string[] DALRRCharacterNames = new string[0x100];

        public static void Fill()
        {
            DALRRCharacterNames[0x00] = "CHAR_SHIDO";
            DALRRCharacterNames[0x01] = "CHAR_TOHKA";
            DALRRCharacterNames[0x02] = "CHAR_ORIGAMI";
            DALRRCharacterNames[0x03] = "CHAR_YOSHINO";
            DALRRCharacterNames[0x04] = "CHAR_KURUMI";
            DALRRCharacterNames[0x05] = "CHAR_KOTORI";
            DALRRCharacterNames[0x06] = "CHAR_RINNE";
            DALRRCharacterNames[0x07] = "CHAR_YOSHINON";
            DALRRCharacterNames[0x08] = "CHAR_REINE";
            DALRRCharacterNames[0x09] = "CHAR_KANAZUKI";
            DALRRCharacterNames[0x0A] = "CHAR_TONOMACHI";
            DALRRCharacterNames[0x0B] = "CHAR_TAMAE";
            DALRRCharacterNames[0x0C] = "CHAR_KUSAKABE";
            DALRRCharacterNames[0x0D] = "CHAR_AI";
            DALRRCharacterNames[0x0E] = "CHAR_MAI";
            DALRRCharacterNames[0x0F] = "CHAR_MII";
            DALRRCharacterNames[0x10] = "CHAR_RULER";
            DALRRCharacterNames[0x15] = "CHAR_MARIA";
            DALRRCharacterNames[0x16] = "CHAR_KAGUYA";
            DALRRCharacterNames[0x17] = "CHAR_YUZURU";
            DALRRCharacterNames[0x18] = "CHAR_MIKU";
            DALRRCharacterNames[0x19] = "CHAR_MANA";
            DALRRCharacterNames[0x1A] = "CHAR_MIKIE";
            DALRRCharacterNames[0x1B] = "CHAR_MARINA";
            DALRRCharacterNames[0x1C] = "CHAR_RIO";
            DALRRCharacterNames[0x32] = "CHAR_YAMAI";
            DALRRCharacterNames[0x64] = "CHAR_UNKNOWN";
            DALRRCharacterNames[0x65] = "CHAR_NEWSCASTER";
            DALRRCharacterNames[0x66] = "CHAR_GIRLSTUDENTA";
            DALRRCharacterNames[0x67] = "CHAR_GIRLSTUDENTB";
            DALRRCharacterNames[0x68] = "CHAR_GUYSTUDENTA";
            DALRRCharacterNames[0x69] = "CHAR_GUYSTUDENTB";
            DALRRCharacterNames[0x6A] = "CHAR_FOODCART";
            DALRRCharacterNames[0x6B] = "CHAR_CLERK";
            DALRRCharacterNames[0x6C] = "CHAR_MALECUSTOMERA";
            DALRRCharacterNames[0x6D] = "CHAR_MALECUSTOMERB";
            DALRRCharacterNames[0x6E] = "CHAR_MALECUSTOMERC";
            DALRRCharacterNames[0x6F] = "CHAR_FEMALECUSTOMERA";
            DALRRCharacterNames[0x70] = "CHAR_FEMALECUSTOMERB";
            DALRRCharacterNames[0x71] = "CHAR_FEMALECUSTOMERC";
            DALRRCharacterNames[0x72] = "CHAR_FEMALECREWMEMBER";
            DALRRCharacterNames[0x73] = "CHAR_PARKMANAGER";
            DALRRCharacterNames[0x74] = "CHAR_TEACHER";
            DALRRCharacterNames[0x75] = "CHAR_FEMALECUSTOMER";
            DALRRCharacterNames[0x76] = "CHAR_MALECUSTOMER";
            DALRRCharacterNames[0x77] = "CHAR_REPORTER";
            DALRRCharacterNames[0x78] = "CHAR_REFEREE";
            DALRRCharacterNames[0x79] = "CHAR_MAYOR";
            DALRRCharacterNames[0x7A] = "CHAR_MAINKURUMI";
            DALRRCharacterNames[0x7B] = "CHAR_CITYMANA";
            DALRRCharacterNames[0x7C] = "CHAR_CITYMANB";
            DALRRCharacterNames[0x7D] = "CHAR_CITYWOMANA";
            DALRRCharacterNames[0x7E] = "CHAR_CITYWOMANB";
            DALRRCharacterNames[0x7F] = "CHAR_CHARAO";
            DALRRCharacterNames[0x80] = "CHAR_ANNOUNCER";
            DALRRCharacterNames[0x81] = "CHAR_FEMALEPRESENTER";
            DALRRCharacterNames[0x82] = "CHAR_TRAINER";
            DALRRCharacterNames[0x83] = "CHAR_ANGEL";
            DALRRCharacterNames[0x84] = "CHAR_VIDEO";
            DALRRCharacterNames[0x85] = "CHAR_BAKERYWORKER";
            DALRRCharacterNames[0x86] = "CHAR_CAKESHOPWORKER";
            DALRRCharacterNames[0x87] = "CHAR_KEBABSHOPWORKER";
            DALRRCharacterNames[0x88] = "CHAR_TENNISREFEREE";
            DALRRCharacterNames[0x89] = "CHAR_FEMALEANNOUNCERA";
            DALRRCharacterNames[0x8A] = "CHAR_FEMALEANNOUNCERB";
            DALRRCharacterNames[0x8B] = "CHAR_YOUNGLADY";
            DALRRCharacterNames[0x8C] = "CHAR_YOUNGMAN";
            DALRRCharacterNames[0x8D] = "CHAR_PHANTOM";
            DALRRCharacterNames[0x8E] = "CHAR_AIMAIMII";
            DALRRCharacterNames[0x8F] = "CHAR_KURUMISHIDO";

            DALRRCharacterNames[0xFF] = "CHAR_NONE";
        }
    }
}
