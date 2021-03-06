﻿using DALLib.File;
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
        protected Game _game = Game.DateALiveRioReincarnation;

        public ObservableCollection<DialogueCode> DialogueCodes => _dialogueCodes;

        public STSCFileDialogue(STSCFileDatabase database, Game game = Game.DateALiveRioReincarnation)
        {
            _database = database;
            _game = game;
        }

        public override void Load(ExtendedBinaryReader reader, bool keepOpen = false)
        {
            base.Load(reader);
            ConvertToDialogueCode();
        }

        public override void Save(ExtendedBinaryWriter writer)
        {
            foreach (var code in DialogueCodes)
            {
                var inst = Instructions[code.Index];
                switch (_game)
                {
                    case Game.DateALiveRioReincarnation:
                        switch (code.Type)
                        {
                            case "Title":
                                break;
                            case "Voice":
                                break;
                            case "Msg":
                                inst.Arguments[2] = code.Text;
                                break;
                            case "Script":
                                break;
                            case "Opt": // TODO: Is this meant to be Choice?
                                inst.Arguments[1] = code.Text;
                                break;
                            case "Map":
                                inst.Arguments[1] = code.Text;
                                break;
                            default:
                                continue;
                        }
                        break;
                    case Game.PsychedelicaOfTheBlackButterfly:
                        switch (code.Type)
                        {
                            case "Title":
                                break;
                            case "Voice":
                                break;
                            case "Msg":
                                inst.Arguments[2] = code.Text;
                                break;
                            case "Choice":
                                inst.Arguments[1] = code.Text;
                                break;
                            default:
                                continue;
                        }
                        break;
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
                switch (_game)
                {
                    case Game.DateALiveRioReincarnation:
                        switch (inst.Name)
                        {
                            case "MesTitle":
                                byte titleID = inst.GetArgument<byte>(0);
                                code.Type = "Title";
                                code.ID = string.Format("0x{0:X2}", titleID);
                                if (_database != null)
                                    code.Text = titleID == 0xFF
                                        ? "None"
                                        : _database.Characters.FirstOrDefault(t => t.ID == titleID)?.FriendlyName;
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
                                code.ID = inst.GetArgument<short>(3).ToString();
                                code.Text = inst.GetArgument<string>(2);
                                code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x80, 0x00));
                                break;
                            case "FileJump":
                                code.Type = "Script";
                                code.ID = inst.GetArgument<string>(0);
                                code.Text = inst.GetArgument<string>(0);
                                code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x00, 0x00));
                                break;
                            case "SetChoice":
                                code.Type = "Choice";
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
                            case "EnableMonologue":
                                byte mode = inst.GetArgument<byte>(0);
                                code.Type = "Monologue";
                                code.ID = inst.GetArgument<string>(0);
                                code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x40));
                                switch (mode)
                                {
                                    case 0:
                                        code.Text = "0: Hidden";
                                        break;
                                    case 1:
                                        code.Text = "1: Show";
                                        break;
                                    case 2:
                                        code.Text = "2: Unknown";
                                        break;
                                    default:
                                        code.Text = mode + ": Unknown, Please report";
                                        break;
                                }

                                break;
                            default:
                                continue;
                        }
                        break;
                    case Game.PsychedelicaOfTheBlackButterfly:
                        switch (inst.Name)
                        {
                            case "Name":
                                int index = Instructions.IndexOf(inst);
                                if (Instructions.Count > index && 
                                    (Instructions[index + 1].Name == "Message" ||
                                     Instructions[index + 1].Name == "MessageVoice"))
                                {
                                    code.Type = "Name";
                                    code.ID = inst.GetArgument<string>(0);
                                    code.Text = inst.GetArgument<string>(0);
                                    code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x00, 0x80));
                                    break;
                                }
                                continue;
                            case "MessageVoice":
                                code.Type = "Voice";
                                code.ID = inst.GetArgument<string>(0);
                                code.Text = inst.GetArgument<string>(0);
                                code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x80, 0x80));
                                break;
                            case "Message":
                                code.Type = "Msg";
                                code.ID = inst.GetArgument<short>(3).ToString();
                                code.Text = inst.GetArgument<string>(2);
                                code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x00, 0x80, 0x00));
                                break;
                            case "FileJump":
                                code.Type = "Script";
                                code.ID = inst.GetArgument<string>(0);
                                code.Text = inst.GetArgument<string>(0);
                                code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x00, 0x00));
                                break;
                            case "Choice":
                                code.Type = "Choice";
                                code.ID = inst.GetArgument<string>(0);
                                code.Text = inst.GetArgument<string>(1);
                                code.Brush = new SolidColorBrush(Color.FromArgb(0x30, 0x80, 0x80, 0x00));
                                break;
                            default:
                                continue;
                        }
                        break;

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
            public string TextPreview
            {
                get { return App.StringProcess.Process(Text); }
                set { Text = App.StringProcess.ProcessReverse(value); }
            }
            // GUI
            public Brush Brush { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
        }

    }
}
