using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DALLib.Scripting.STSCInstructions.ArgumentType;

namespace DALLib.Scripting
{
    public static partial class STSCInstructions
    {
        public static List<Instruction> DALRRInstructions = new List<Instruction>()
        {
            new Instruction("NOP", null),
            new Instruction("Exit", null),
            new Instruction("continue", null),
            new Instruction("Endv", null),
            null, // InfinitWait
            new Instruction("Wait", new []{ AT_Int32 }),
            new Instruction("Goto", new []{ AT_CodePointer }),
            new Instruction("return", null),
            null, // ReturnPush
            null, // ReturnPop
            null, // Call_iv
            new Instruction("SubStart", new []{ AT_Byte, AT_CodePointer }),
            new Instruction("SubEnd", new []{ AT_Byte }),
            new Instruction("RandJump", new []{ AT_PointerArray }),
            new Instruction("Printf", new []{ AT_String }),
            new Instruction("FileJump", new []{ AT_String }),

            null, // FlgOnJump
            null, // FlgOffJump
            new Instruction("FlagSet", new []{ AT_Int16, AT_Int16, AT_Byte }),
            null, // PrmTrueJump
            null, // PrmFalseJump
            new Instruction("PrmSet", new []{ AT_Int32, AT_Int32 }),
            new Instruction("PrmCopy", new []{ AT_Int32, AT_Int32 }),
            new Instruction("PrmAdd", new []{ AT_Int32, AT_Int32 }),
            null, // PrmAddWk
            null, // PrmBranch
            new Instruction("Call", new []{ AT_CodePointer }),
            new Instruction("CallReturn", null),
            null, // SubEndWait
            new InstructionIf(),
            new InstructionSwitch("switch", null),
            null, // PrmRand

            new Instruction("DataBaseParam", new []{ AT_Byte, AT_DataBlock }),
            new Instruction("NewGameOpen", null),
            new Instruction("EventStartMes", new []{ AT_String }),
            new Instruction("Dummy23", null),
            new Instruction("Dummy24", null),
            new Instruction("Dummy25", null),
            new Instruction("Dummy26", null),
            new Instruction("Dummy27", null),
            new Instruction("Dummy28", null),
            new Instruction("Dummy29", null),
            new Instruction("Dummy2A", null),
            new Instruction("Dummy2B", null),
            new Instruction("Dummy2C", null),
            new Instruction("Dummy2D", null),
            new Instruction("Dummy2E", null),
            null, // SetEventNumber

            new Instruction("PlayMovie", new []{ AT_String, AT_Int16, AT_Byte }),
            new Instruction("BgmWait", new []{ AT_Byte, AT_Int16 }),
            new Instruction("BgmVolume", new []{ AT_Int32, AT_Int16 }),
            new Instruction("SePlay", new []{ AT_Byte, AT_Byte, AT_Bool }),
            new Instruction("SeStop", new []{ AT_Byte, AT_Byte }),
            null, // SeWait 
            new Instruction("SeVolume", new []{ AT_Int16, AT_Int32, AT_Int16 }),
            new Instruction("SeAllStop", null),
            new Instruction("BgmDummy", new []{ AT_Int32, AT_Int16 }), // Data is discarded
            new Instruction("Dummy39", null),
            new Instruction("Dummy3A", null),
            new Instruction("Dummy3B", null),
            new Instruction("Dummy3C", null),
            new Instruction("Dummy3D", null),
            new Instruction("Dummy3E", null),
            new Instruction("Dummy3F", null),

            new Instruction("SetNowLoading", new []{ AT_Bool }),
            new Instruction("Fade", new []{ AT_Byte, AT_Int16, AT_Int16 }),
            new Instruction("PatternFade", new []{ AT_Int16, AT_Int16, AT_Int16 }),
            new Instruction("Quake", new []{ AT_Byte, AT_Int16}),
            new Instruction("CrossFade", new []{ AT_Int16}),
            new Instruction("PatternCrossFade", new []{ AT_Int16, AT_Int16 }),
            new Instruction("DispTone", new []{ AT_Byte }),
            new Instruction("Dummy47", null),
            new Instruction("Dummy48", null),
            new Instruction("Dummy49", null),
            new Instruction("Dummy4A", null),
            new Instruction("Dummy4B", null),
            new Instruction("Dummy4C", null),
            new Instruction("Dummy4D", null),
            new Instruction("Dummy4E", null),
            new Instruction("Wait2", new []{ AT_Int32 }),

            new Instruction("Mes", new []{ AT_Int16, AT_Int16, AT_String, AT_Int16 }),
            new Instruction("MesWait", null),
            new Instruction("MesTitle", new []{ AT_Byte }),
            new Instruction("SetChoice", new []{ AT_CodePointer, AT_String, AT_Int16 }),
            new Instruction("ShowChoices", new []{ AT_Bool }),
            new Instruction("SetFontSize", new []{ AT_Byte }),
            new Instruction("MapPlace", new []{ AT_Int16, AT_String, AT_CodePointer }),
            new Instruction("MapChara", new []{ AT_Int16, AT_Int16, AT_Byte }),
            new Instruction("MapBg", new []{ AT_Byte }), // mapBg{}.tex
            new Instruction("MapCoord", new []{ AT_Int16, AT_Byte, AT_Bool, AT_Int16, AT_Int16 }),
            new Instruction("MapStart", null),
            new Instruction("MapInit", null),
            new Instruction("MesWinClose", null),
            new Instruction("BgOpen", new []{ AT_Int32, AT_Int32}), // MesWinOpen?
            new Instruction("BgClose", new []{ AT_Byte}),
            new Instruction("MaAnime", new []{ AT_Byte}),

            new Instruction("BgMove", new []{ AT_Byte, AT_Int32, AT_Int16}),
            new Instruction("BgScale", new []{ AT_Float, AT_Int16, AT_Byte, AT_Bool}),
            new Instruction("BustOpen", new []{ AT_Byte, AT_Int32, AT_Int32}),
            new Instruction("BustClose", new []{ AT_Byte, AT_Int16}),
            new Instruction("BustMove", new []{ AT_Byte, AT_Int16, AT_Int16, AT_Int16, AT_Byte}),
            new Instruction("BustMoveAdd", new []{ AT_Byte, AT_Int16, AT_Int16, AT_Int16, AT_Byte}),
            new Instruction("BustScale", new []{ AT_Byte, AT_Float, AT_Int16, AT_Byte, AT_Byte}),
            new Instruction("BustPriority", new []{ AT_Byte, AT_Byte}),
            new Instruction("PlayVoice", new []{ AT_Byte, AT_Int32, AT_String }),
            new Instruction("VoiceCharaDraw", new []{ AT_Int16 }),
            new Instruction("DateSet", new []{ AT_Byte, AT_Byte, AT_Byte }),
            new Instruction("TellOpen", new []{ AT_Byte, AT_Int32, AT_Int16 }),
            new Instruction("TellClose", new []{ AT_Byte, AT_Int16 }),
            new Instruction("Trophy", new []{ AT_Byte }),
            new Instruction("SetVibration", new []{ AT_Byte, AT_Float }), // NOTE: It's "Vibraiton" in-game, did they misspell vibration?
            new Instruction("BustQuake", new []{ AT_Byte, AT_Byte, AT_Int16}),

            new Instruction("BustFade", new []{ AT_Byte, AT_Float, AT_Float, AT_Int16}),
            new Instruction("BustCrossMove", null), // TODO
            new Instruction("BustTone", new []{ AT_Byte, AT_Byte}), // TODO
            new Instruction("BustAnime", new []{ AT_Byte, AT_Byte}), // TODO
            null, // CameraMoveXY
            null, // CameraMoveZ
            new Instruction("CameraMoveXYZ", new []{ AT_Int16, AT_Int16, AT_Float, AT_Int16, AT_Int16}), // TODO
            new Instruction("ScaleMode", new []{ AT_Byte}), // TODO
            new Instruction("GetBgNo", new []{ AT_Int32}), // TODO
            new Instruction("GetFadeState", new []{ AT_Int32}), // TODO
            new Instruction("SetAmbiguous", new []{ AT_Float, AT_Byte, AT_Bool }),
            new Instruction("AmbiguousPowerFade", new []{ AT_Float, AT_Float, AT_Int16 }),
            new Instruction("SetBlur", new []{ AT_Int32, AT_Bool }),
            new Instruction("BlurPowerFade", new []{ AT_Float, AT_Float, AT_Int16}),
            new Instruction("EnableMonologue", new [] { AT_Bool }), // TODO
            new Instruction("SetMirage", new [] { AT_Float, AT_Bool }), // TODO
            
            new Instruction("MiragePowerFade", new [] { AT_Int32, AT_Float, AT_Int16 }), // TODO
            new Instruction("MessageVoiceWait", new [] { AT_Byte }), // TODO
            new Instruction("SetRasterScroll", new [] { AT_Byte, AT_Float, AT_Int16, AT_Byte }),
            new Instruction("RasterScrollPowerFade", new [] { AT_Int32, AT_Float, AT_Int16 }),
            new Instruction("MesDel", null),
            new Instruction("MemoryOn", new [] { AT_Int16 }),
            new Instruction("SaveDateSet", new []{ AT_Byte, AT_String }),
            new Instruction("ExiPlay", new []{ AT_Int16, AT_Byte, AT_Byte, AT_Byte, AT_Byte, AT_Byte, AT_Int32, AT_Byte }), // TODO
            new Instruction("ExiStop", new []{ AT_Int16, AT_Byte }), // TODO
            new Instruction("GalleryFlg", new []{ AT_Int16, AT_Int16 }),
            new Instruction("DateChange", new []{ AT_Int32, AT_Int16 }),
            new Instruction("BustSpeed", new []{ AT_Byte, AT_Int32 }),
            new Instruction("DateRestNumber", new []{ AT_Byte }),
            new Instruction("MapTutorial", new []{ AT_Int16, AT_Int16 }),
            new Instruction("Ending", new []{ AT_Byte }),

            new Instruction("Set/Del+FixAuto", new []{ AT_Byte, AT_Byte, AT_Byte }),
            new Instruction("ExiLoopStop", new []{ AT_Int16, AT_Byte }),
            new Instruction("ExiEndWait", new []{ AT_Int16, AT_Byte }), // TODO
            new Instruction("Set/Del+EventKeyNg", new []{ AT_Byte }),
        };
    }
}
