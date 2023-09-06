using DALLib.File;
using DALLib.IO;
using DALLib.Scripting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ScriptDialogueEditor
{
    public class STSC2FileDialogue : STSC2File
    {
        protected ObservableCollection<STSCFileDialogue.DialogueCode> _dialogueCodes = new ObservableCollection<STSCFileDialogue.DialogueCode>();

        public ObservableCollection<STSCFileDialogue.DialogueCode> DialogueCodes => _dialogueCodes;

        public Dictionary<short, string> CharacterNames { get; set; }

        public STSC2FileDialogue(Dictionary<short, string> characterNames)
        {
            CharacterNames = characterNames;
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
            foreach (var code in DialogueCodes)
            {
                var line = Sequence.Lines[code.Index];
                switch (code.Type)
                {
                    case "Title":
                        break;
                    case "Voice":
                        break;
                    case "Mes":
                        line.arguments[3] = code.Text;
                        break;
                    case "Script":
                        break;
                    case "Opt":
                        break;
                    case "Map":
                        break;
                    default:
                        continue;
                }
            }

            base.Save(writer);
        }

        public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            base.Load(reader);
            ConvertToDialogueCode();
        }

        public void ConvertToDialogueCode()
        {
            DialogueCodes.Clear();
            foreach (var line in Sequence.Lines)
            {
                var code = new STSCFileDialogue.DialogueCode();
                switch (line.Name)
                {
                    case "Name / NameOff":
                        short nameID = (short)(int)(line.arguments[0] as STSC2Node).Value;
                        string name = CharacterNames.ContainsKey(nameID) ? CharacterNames[nameID] : $"0x{nameID:X2}";
                        code.Type = "Name";
                        code.ID = string.Format("0x{0:X4}", nameID);
                        code.Text = name ?? string.Format("0x{0:X4}", nameID);
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x00, 0x80));
                        break;
                    case "MesVoice+ 2/Idx":
                        code.Type = "MesVoice";
                        code.ID = (line.arguments[2] as STSC2Node).Value as string;
                        code.Text = (line.arguments[2] as STSC2Node).Value as string;
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x80, 0x80));
                        break;
                    case "Mes":
                        code.Type = line.Name;
                        code.ID = ((short)line.arguments[4]).ToString();
                        code.Text = (line.arguments[3] as string).Replace("\n", "\\n");
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x80, 0x00));
                        break;
                    case "FileJump":
                        code.Type = line.Name;
                        code.ID = (line.arguments[0] as STSC2Node).Value as string;
                        code.Text = (line.arguments[0] as STSC2Node).Value as string;
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x00, 0x00));
                        break;
                    case "Choice":
                        code.Type = "Choice";
                        code.ID = line.arguments[2].ToString();
                        code.Text = (line.arguments[1] as string);
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x00));
                        break;
                    case "MapPlace":
                        code.Type = "MapPlace";
                        code.ID = Convert.ToString(line.arguments[0]);
                        code.Text = Convert.ToString(line.arguments[1]);
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x80));
                        break;
                    default:
                        continue;
                }
                code.Index = Sequence.Lines.IndexOf(line);
                DialogueCodes.Add(code);
            }
        }
    }
}
