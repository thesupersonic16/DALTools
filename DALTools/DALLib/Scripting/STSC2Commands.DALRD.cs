﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALLib.Scripting
{
    public static partial class STSC2Commands
    {

        public static List<Command> DALRDCommands = new List<Command>()
        {
            // 0x00
            new Command("Nop", null),
            new Command("Exit", null),
            new Command("Cont", new [] { typeof(int) }), // TODO
            new Command("Printf", new [] { typeof(STSC2Node) }),
            new Command("VWait", new [] { typeof(STSC2Node) }),
            new Command("Goto", new [] { typeof(int) }),
            new CommandSubStart(),
            new Command("SubEnd", null),
            new Command("SubEndWait", new [] { typeof(byte) }),
            new CommandFuncCall(),
            new CommandReturn(),
            new Command("FileJump", new [] { typeof(STSC2Node) }),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            // 0x10
            new Command("FlgSw", new [] { typeof(long), typeof(bool) }),
            new Command("FlgSet", new [] { typeof(long), typeof(STSC2Node) }),
            new Command("ValSet", new [] { typeof(long), typeof(int), typeof(byte) }),
            new Command("StrSet", null),
            new Command("JumpIf", new[] { typeof(uint), typeof(STSC2Node) }),
            new CommandJumpSwitch(),
            new Command("RandVal", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("AutoStop", null),
            // 0x20
            new Command("Movie", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(byte) }),
            new Command("Bgm+ Play/Stop", new [] { typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("BgmVolume", new [] { typeof(int), typeof(STSC2Node) }),
            new Command("SePlay", new [] { typeof(STSC2Node), typeof(bool) }),
            new Command("SeStop", new [] { typeof(STSC2Node) }),
            new Command("SeWait", new [] { typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("SeVolume", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("SeAllStop", null),
            new Command("Dummy", null),
            new Command("VoicePlay", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            // 0x30
            new Command("NowLoading+ Start/Stop", new [] { typeof(bool) }),
            new Command("Fade", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(byte) }),
            new Command("PatternFade", null),
            new Command("Quake", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("CrossFade", new [] { typeof(STSC2Node) }),
            new Command("PatternCrossFade", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("DispTone", new [] { typeof(STSC2Node) }),
            new Command("FadeWait", new [] { typeof(STSC2Node) }),
            new Command("ExiWait", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("MesQuake", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("SetRootName", new [] { typeof(STSC2Node) }),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Dummy", null),
            new Command("Wait2", new [] { typeof(STSC2Node) }),
            // 0x40
            new Command("Mes", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(string), typeof(short) }),
            new Command("MesWait", null),
            new Command("Name / NameOff", new [] { typeof(STSC2Node) }),
            new Command("Choice", new [] { typeof(uint), typeof(string), typeof(short) }),
            new Command("ChoiceStart", new [] { typeof(bool) }),
            new Command("FontSize", new [] { typeof(STSC2Node) }),
            new Command("MapPlace", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(int) }), // TODO
            new Command("MapChara", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("MapBg", new [] { typeof(STSC2Node) }),
            new Command("MapCoord", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("MapStart", null),
            new Command("MapInit", null),
            new Command("MesWinClose", null),
            new Command("BgOpen", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("BgClose", new [] { typeof(STSC2Node) }),
            new Command("MaAnime", new [] { typeof(STSC2Node) }),
            // 0x50
            new Command("BgMove", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(bool), typeof(byte) }),
            new Command("BgScale", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(byte), typeof(byte) }),
            new Command("BustOpen", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("BustClose", new [] { typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("BustMove", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node)}),
            new Command("BustMoveAdd", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("BustScale", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(byte), typeof(byte) }),
            new Command("BustPriority", new [] { typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("MesVoice+ 2/Idx", new [] { typeof(byte), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("VoiceCharaDraw", new [] { typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("DateSet", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("TellOpen", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("TellClose", new [] { typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("Trophy", new [] { typeof(STSC2Node) }),
            new Command("Vibraiton", null),
            new Command("BustQuake", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            // 0x60
            new Command("BustFade", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("BustCrossMove", null),
            new Command("BustTone", new [] { typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("BustAnime", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("CameraMoveXY", null),
            new Command("CameraMoveZ", null),
            new Command("CameraMoveXYZ", null),
            new Command("ScaleMode", null),
            new Command("GetBgNo", null),
            new Command("GetFadeState", null),
            new Command("Ambiguous+ On/Off", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(byte) }),
            new Command("AmbiguousPowerFade", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("Blur+ On/Off", new [] { typeof(STSC2Node), typeof(byte) }), // TODO
            new Command("BlurPowerFade", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("Monologue+ On/Off", new [] { typeof(STSC2Node) }),
            new Command("Mirage+ On/Off", new [] { typeof(STSC2Node), typeof(bool) }),
            // 0x70
            new Command("MiragePowerFade", null),
            new Command("MessageVoiceWait", new [] { typeof(STSC2Node) }),
            new Command("RasterScroll+ On/Off", null),
            new Command("RasterScrollPowerFade", null),
            new Command("MesDel", null),
            new Command("MemoryOn", new [] { typeof(STSC2Node) }),
            new Command("SaveDateSet", null),
            new Command("ExiPlay", new [] { typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("ExiStop", new [] { typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("GalleryFlg", new [] { typeof(STSC2Node) }),
            new Command("DateChange", null),
            new Command("BustSpeed", null),
            new Command("DateRestNumber", new [] { typeof(STSC2Node) }),
            new Command("MapTutorial", null),
            new Command("Ending", null),
            new Command("Set/Del +FixAuto", null),
            // 0x80
            new Command("ExiLoopStop", null),
            new Command("ExiEndWait", new [] { typeof(STSC2Node), typeof(STSC2Node) }),
            new Command("Set/Del +EventKeyNg", new [] { typeof(bool) }),
            new Command("BustExpr", null),
            new Command("BustLoadStart", null),
            new Command("BustLoadWait", null),
            // 0x86
        };
    }
}