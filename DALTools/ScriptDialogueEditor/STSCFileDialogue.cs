using DALLib.File;
using DALLib.IO;
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
    public class STSCFileDialogue : STSCFile
    {

        protected ObservableCollection<DialogueCode> _dialogueCodes = new ObservableCollection<DialogueCode>();
        protected STSCFileDatabase _database = null;

        public ObservableCollection<DialogueCode> DialogueCodes => _dialogueCodes;

        public STSCFileDialogue(STSCFileDatabase database)
        {
            _database = database;
        }

        public override void Load(ExtendedBinaryReader reader)
        {
            base.Load(reader);
            ConvertToDialogueCode();
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
            foreach (var code in DialogueCodes)
            {
                var inst = Instructions[code.Index];
                switch (code.Type)
                {
                    case "Title":
                        break;
                    case "Voice":
                        break;
                    case "Msg":
                        inst.Arguments[4] = code.Text;
                        break;
                    case "Script":
                        break;
                    case "Opt":
                        inst.Arguments[1] = code.Text;
                        break;
                    case "Map":
                        inst.Arguments[1] = code.Text;
                        break;
                    default:
                        continue;
                }

            }

            base.Save(writer);
        }

        public void ConvertToDialogueCode()
        {
            DialogueCodes.Clear();
            foreach (var inst in Instructions)
            {
                var code = new DialogueCode();
                switch (inst.Name)
                {
                    case "MesTitle":
                        byte titleID = inst.GetArgument<byte>(0);
                        code.Type = "Title";
                        code.ID = string.Format("0x{0:X2}", titleID);
                        if (_database != null)
                            code.Text = titleID == 0xFF ? "None" : _database.Characters.FirstOrDefault(t => t.ID == titleID)?.FriendlyName;
                        else
                            code.Text = titleID == 0xFF ? "None" : titleID.ToString();
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x00, 0x80));
                        break;
                    case "PlayVoice":
                        code.Type = "Voice";
                        code.ID = inst.GetArgument<string>(2);
                        code.Text = inst.GetArgument<string>(2);
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x80, 0x80));
                        break;
                    case "Mes":
                        code.Type = "Msg";
                        code.ID = inst.GetArgument<short>(5).ToString();
                        code.Text = inst.GetArgument<string>(4);
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x80, 0x00));
                        break;
                    case "FileJump":
                        code.Type = "Script";
                        code.ID = inst.GetArgument<string>(0);
                        code.Text = inst.GetArgument<string>(0);
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x00, 0x00));
                        break;
                    case "SetChoice":
                        code.Type = "Opt";
                        code.ID = inst.GetArgument<string>(0);
                        code.Text = inst.GetArgument<string>(1);
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x00));
                        break;
                    case "MapPlace":
                        code.Type = "Map";
                        code.ID = inst.GetArgument<string>(0);
                        code.Text = inst.GetArgument<string>(1);
                        code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x80));
                        break;
                    default:
                        continue;
                }
                code.Index = Instructions.IndexOf(inst);
                DialogueCodes.Add(code);
            }
        }

        public class DialogueCode : INotifyPropertyChanged
        {
            public int Index = 0;

            public string Type { get; set; }
            public string ID   { get; set; }
            public string Text { get; set; }

            // GUI
            public Brush Brush { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }

    }
}
