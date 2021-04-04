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
        public static List<Instruction> PBBInstructions = new List<Instruction>()
        {
            // 0x00
            new Instruction("NOP", null),
            new Instruction("Exit", null),
            new Instruction("Continue", new []{ AT_Byte }),
            new Instruction("Endv", null),
            null, // InfinitWait
            new Instruction("VWait", new []{ AT_Int32 }),
            new Instruction("Goto", new []{ AT_CodePointer }),
            new Instruction("Return", null),
            null, // RetAddrPush
            null, // RetAddrPop
            null, // CalliV
            new Instruction("SubStart", new []{ AT_Byte, AT_CodePointer }),
            new Instruction("SubEnd", new []{ AT_Byte }),
            new Instruction("RandJmp", new []{ AT_PointerArray }),
            new Instruction("Printf", new []{ AT_String }),
            new Instruction("FileJump", new []{ AT_String }),
            // 0x10
            null, // IsFlg
            new Instruction("NgFlg", new []{ AT_Int32, AT_Int32 }),
            new Instruction("FlgSw", new []{ AT_Byte, AT_Int16, AT_Int16 }),
            null, // IsPrm
            null, // NgPrm
            new Instruction("SetPrm", new []{ AT_Int32, AT_Int32 }),
            null, // SetPrmWk
            new Instruction("AddPrm", new []{ AT_Int32, AT_Int32 }),
            null, // AddPrmWk
            null, // PrmBranch
            new Instruction("Call", new []{ AT_CodePointer }),
            new Instruction("CallRet", null),
            null, // SubEndWait
            new InstructionIf(),
            new InstructionSwitch("switch", null),
            null, // RandPrm
            // 0x20
            new Instruction("DataBaseParam", new []{ AT_Byte, AT_DataBlock }),
            new Instruction("CourseFlag", new []{ AT_Int16 }),
            new Instruction("SetNowScenario", new []{ AT_Int16 }),
            null, // CharEntry
            new Instruction("CrossFade", new []{ AT_Int16}),
            new Instruction("PatternCrossFade", new []{ AT_Int16, AT_Int16 }),
            new Instruction("DispTone", new []{ AT_Byte }),
            new Instruction("Monologue", new []{ AT_Bool }),
            new Instruction("ExiPlay", new []{ AT_Int32, AT_Int16, AT_Int16, AT_Int16, AT_Int32, AT_Byte }), // TODO
            new Instruction("ExiStop", new []{ AT_Byte, AT_Byte }), // TODO
            new Instruction("PatternFade", new []{ AT_Int16, AT_Int16, AT_Int16 }),
            new Instruction("Ambiguous", new []{ AT_Float, AT_Byte, AT_Bool }),
            new Instruction("AmbiguousFade", new []{ AT_Float, AT_Float, AT_Int16 }),
            new Instruction("TouchWait", new []{ AT_Int32, AT_Int16 }),
            new Instruction("CourseFlagGet", new []{ AT_Int32, AT_Int16 }),
            new Instruction("Chapter", new []{ AT_Int16 }),
            // 0x30
            new Instruction("Movie", new []{ AT_String, AT_Byte, AT_Byte }),
            new Instruction("BgmPlay", new []{ AT_Byte, AT_Int16 }),
            new Instruction("BgmVolume", new []{ AT_Int32, AT_Int16 }),
            new Instruction("SePlay", new []{ AT_Int32, AT_Bool }),
            new Instruction("SeStop", new []{ AT_Int32, AT_Int16 }),
            null, // SeWait 
            new Instruction("SeVolume", new []{ AT_Int32, AT_Int16, AT_Int16, AT_Int16 }),
            new Instruction("SeAllStop", new []{ AT_Int16 }),
            null, // VoicePlay
            null, // VoiceStop
            new Instruction("VoiceWait", new []{ AT_Byte }),
            null, // VoiceVolumePlay
            new Instruction("Dummy3C", null),
            new Instruction("Dummy3D", null),
            null, // BackLogReset
            new Instruction("GetCountry", new []{ AT_Int32 }),
            // 0x40
            new Instruction("BgOpen", new []{ AT_Int32, AT_Int16, AT_Byte}),
            new Instruction("BgClose", new []{ AT_Byte }),
            new Instruction("BgFrame", new []{ AT_Int16, AT_Int16 }),
            new Instruction("BgMove", new []{ AT_Byte, AT_Int32, AT_Int16}),
            new Instruction("BgScale", new []{ AT_Float, AT_Int16, AT_Byte, AT_Bool}),
            new Instruction("BustOpen", new []{ AT_Byte, AT_Int32, AT_Byte, AT_Int16 }),
            new Instruction("BustClose", new []{ AT_Byte, AT_Int16}),
            new Instruction("BustMove", new []{ AT_Byte, AT_Int16, AT_Int16, AT_Int16, AT_Byte}),
            new Instruction("BustMoveAdd", new []{ AT_Byte, AT_Int16, AT_Int16, AT_Int16, AT_Byte}),
            new Instruction("BustScale", new []{ AT_Byte, AT_Float, AT_Int16, AT_Byte, AT_Byte}),
            new Instruction("BustPriority", new []{ AT_Byte, AT_Byte}),
            new Instruction("BustQuake", new []{ AT_Byte, AT_Byte, AT_Byte, AT_Int16 }),
            new Instruction("SetEntryCharFlg", new []{ AT_Byte }),
            new Instruction("BustTone", new []{ AT_Byte, AT_Byte}),
            new Instruction("BustFade", new []{ AT_Byte, AT_Float, AT_Float, AT_Int16}),
            new Instruction("Name", new []{ AT_String }),
            // 0x50
            new Instruction("Message", new []{ AT_Int16, AT_Int16, AT_String, AT_Int16, AT_Byte }),
            new Instruction("MessageWait", new []{ AT_Bool }),
            new Instruction("MessageWinClose", null),
            new Instruction("MessageFontSize", new []{ AT_Byte }),
            new Instruction("MessageQuake", new []{ AT_Byte, AT_Byte, AT_Int16 }),
            new Instruction("Trophy", new []{ AT_Byte }),
            new Instruction("MessageDelete", null),
            new Instruction("Quake", new []{ AT_Byte, AT_Byte, AT_Int16 }),
            new Instruction("Fade", new []{ AT_Byte, AT_Byte, AT_Int16 }),
            new Instruction("Choice", new []{ AT_CodePointer, AT_String, AT_Int16 }),
            new Instruction("ChoiceStart", null),
            new Instruction("GetBg", new []{ AT_Int32 }),
            new Instruction("FontColor", new []{ AT_Byte, AT_Byte }),
            new Instruction("WorldType", new []{ AT_Byte }),
            new Instruction("GetWorld", new []{ AT_Int32 }),
            new Instruction("Flowchart", new []{ AT_Int16, AT_Int16 }),
            // 0x60
            new Instruction("MiniGame", new []{ AT_Byte, AT_String }),
            new Instruction("CourseOpenGet", new []{ AT_Int16, AT_Int32 }),
            new Instruction("SystemSave", null),
            null, // BackLogNg
            null, // EventSkipNg
            null, // EventAutoNg
            null, // StopNg
            new Instruction("DataSave", null),
            new Instruction("SkipStop", null),
            new Instruction("MessageVoice", new []{ AT_String, AT_String }),
            null, // MessageVoice2
            new Instruction("SaveNg", new []{ AT_Bool }),
            new Instruction("Dummy6C", null),
            new Instruction("Dummy6D", null),
            new Instruction("Dummy6E", null),
            null, // Dialog
            // 0x70
            new Instruction("ExiLoopStop", new []{ AT_Int16, AT_Byte }),
            new Instruction("ExiEndWait", new []{ AT_Byte, AT_Int16 }),
            new Instruction("ClearSet", new []{ AT_Byte }),
            null, // SaveTitleSet
            new Instruction("Dummy74", null),
            new Instruction("Dummy75", null),
            new Instruction("Dummy76", null),
            new Instruction("Dummy77", null),
            null, // VideoFlg
            null, // AlbumFlg
            null, // WaitFlg
            null, // AudioFlg
            new Instruction("Dummy7C", null),
            new Instruction("Dummy7D", null),
            new Instruction("Dummy7E", null),
            new Instruction("Dummy7F", null),
            // 0x80
            new Instruction("Dummy80", null),
            new Instruction("Dummy81", null),
            new Instruction("Dummy82", null),
            new Instruction("Dummy83", null),
            new Instruction("Dummy84", null),
            new Instruction("Dummy85", null),
            new Instruction("Dummy86", null),
            new Instruction("Dummy87", null),
            new Instruction("Dummy88", null),
            new Instruction("Dummy89", null),
            new Instruction("Dummy8A", null),
            new Instruction("Dummy8B", null),
            new Instruction("Dummy8C", null),
            new Instruction("Dummy8D", null),
            new Instruction("Dummy8E", null),
            new Instruction("Dummy8F", null),
            // 0x90
            new Instruction("Dummy90", null),
            new Instruction("Dummy91", null),
            new Instruction("Dummy92", null),
            new Instruction("Dummy93", null),
        };
    }
}
