using DALLib.File;
using DALLib.ImportExport;
using Microsoft.Win32;
using ScriptDialogueEditor.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using NanoXLSX;
using Path = System.IO.Path;
using DALLib.IO;
using DALLib.Scripting;

namespace ScriptDialogueEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Config Config { get; set; } = new Config();

        public PCKFile ScriptArchive { get; set; }
        public STSCFileDialogue ScriptFile { get; set; }
        public STSC2FileDialogue ScriptFilev2 { get; set; }
        public STSCFileDatabase ScriptDB { get; set; }

        // STSC2
        public Dictionary<short, string> CharacterNames { get; set; } = new Dictionary<short, string>() { { 0xFF, "None" } };
        public ScriptVersion CurrentScriptVersion { get; set; }
        public Game CurrentGame { get; set; }
        public bool IsScriptv2 => CurrentScriptVersion == ScriptVersion.STSC2;

        public List<PCKFile.FileEntry> ScriptArchiveFiles => ScriptArchive.FileEntries;

        public DALRRLivePreview Preview = null;

        public bool ScriptEdited = false;
        protected bool RefreshDatabase = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        public ScriptVersion DetectScriptVersion(Stream stream)
        {
            var reader = new ExtendedBinaryReader(stream);
            string sig = reader.PeekSignature(6);
            if (sig == "STSC2.") // Assume STSC2
                return ScriptVersion.STSC2;
            return ScriptVersion.STSC1;
        }

        public void LoadScriptDatabase()
        {
            if (!string.IsNullOrEmpty(App.ScriptPath))
            {
                // Load the DAL RR database file if the script is loaded
                if (CurrentGame == Game.DateALiveRioReincarnation)
                {
                    string scriptDB = Path.Combine(Path.GetDirectoryName(App.ScriptPath), "database.bin");
                    if (File.Exists(scriptDB))
                    {
                        ScriptDB = new STSCFileDatabase();
                        ScriptDB.Load(scriptDB);
                    }
                    else
                    {
                        MessageBox.Show("Could not find database.bin!\n" +
                            "Voice IDs will be used instead of character names without the database.bin file");
                    }
                }

                // Load character data if scripts are from DAL RD
                if (CurrentGame == Game.DateALiveRenDystopia)
                {
                    string path = Path.Combine(Path.GetDirectoryName(App.ScriptPath), "data_chara.bin");
                    if (File.Exists(path))
                    {
                        CharacterNames.Clear();
                        CharacterNames.Add(0xFF, "None");
                        var table = new TableFile();
                        table.Load(path);
                        var entries = table.ToObject<CharaEntry>();
                        foreach (var chara in entries)
                            CharacterNames.Add((short)chara.No, chara.Name);
                    }
                    else
                    {
                        MessageBox.Show("data_chara.bin is missing!\n" +
                            "Voice IDs will be used instead of character names");
                    }
                }
            }
            RefreshDatabase = false;
        }

        public void LoadScript(string scriptName, bool save = true)
        {
            if (ScriptEdited && save)
                SaveScript();

            // Auto detect file version
            var scriptStream = ScriptArchive.GetFileStream(scriptName);
            if (App.ScriptVersion == ScriptVersion.Unknown)
                CurrentScriptVersion = DetectScriptVersion(scriptStream);

            // Select game if not already
            if (App.WorkingGame == Game.Unknown)
                CurrentGame = IsScriptv2 ?
                    Game.DateALiveRenDystopia : Game.DateALiveRioReincarnation;
            else
                CurrentGame = App.WorkingGame;

            if (IsScriptv2)
            {
                // Backup incase of error
                var oldScript = ScriptFilev2;
                try
                {
                    ScriptFilev2 = new STSC2FileDialogue(CharacterNames);
                    ScriptFilev2.Load(scriptStream);
                }
                catch
                {
                    ScriptFilev2 = oldScript;
                    MessageBox.Show($"Failed to load script {scriptName}", "Script Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                ScriptFile = new STSCFileDialogue(ScriptDB, CurrentGame);
                ScriptFile.Load(scriptStream);
            }
            Config.LastOpenedScript = scriptName;
            ScriptEdited = false;

            if (RefreshDatabase)
                LoadScriptDatabase();

            UpdateTitle();

            // Fix bindings
            BindingOperations.SetBinding(ScriptListView, ListView.ItemsSourceProperty,
                new Binding(IsScriptv2 ? "ScriptFilev2.DialogueCodes" : "ScriptFile.DialogueCodes") { Source = this });
        }

        public void SaveScript()
        {
            if (IsScriptv2)
            {
                if (ScriptFilev2 != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        // Builds the script file and saves it to the stream
                        ScriptFilev2.Save(stream);
                        // Replaces the file that was in the archive with the newly built script
                        ScriptArchive.ReplaceFile(Config.LastOpenedScript, stream.ToArray());
                    }
                    ScriptEdited = false;
                }
            }
            else
            {
                if (ScriptFile != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        // Builds the script file and saves it to the stream
                        ScriptFile.Save(stream);
                        // Replaces the file that was in the archive with the newly built script
                        ScriptArchive.ReplaceFile(Config.LastOpenedScript, stream.ToArray());
                    }
                    ScriptEdited = false;
                }
            }
        }

        /// <summary>
        /// Updates the title of the window
        /// </summary>
        public void UpdateTitle()
        {
            string title = App.ProgramName;
            if (ScriptFile != null || ScriptFilev2 != null)
                title += $" - {Config.LastOpenedScript}";
            if (ScriptEdited)
                title += "*";
            Title = title.Replace("_", "__");
        }

        public string GetPathFromSteam()
        {
            try
            {
                // List of supported Steam games
                List<SteamGame> games = new List<SteamGame>();
                // Reads Steam information
                Steam.Init();
                // Searches for DATE A LIVE: Rio Reincarnation
                games = Steam.SearchForGames("DATE A LIVE: Rio Reincarnation");
                if (games.Count != 0)
                    return Path.Combine(games[0].RootDirectory, $"Data/{GetLangDirectoryName(Config.DefaultGameLanguage)}/Script/Script.pck");
            }
            catch { }
            return "";
        }

        public static string GetLangDirectoryName(GameLanguage lang)
        {
            switch (lang)
            {
                case GameLanguage.English:
                    return "ENG";
                case GameLanguage.Japanese:
                    return "JPN";
                case GameLanguage.Chinese:
                    return "CHN";
                default:
                    return "ENG";
            }
        }

        public short GetUnusedMesID()
        {
            short id = 0;
            var codes = IsScriptv2 ? ScriptFilev2.DialogueCodes : ScriptFile.DialogueCodes;
            foreach (var code in codes)
            {
                if (IsScriptv2 && code.Type == "Mes")
                {
                    short mesID = (short)ScriptFilev2.Sequence.Lines[code.Index].arguments[4];
                    if (mesID > id)
                        id = mesID;
                }
                if (!IsScriptv2 && code.Type == "Meg")
                {
                    short mesID = ScriptFile.Instructions[code.Index].GetArgument<short>(3);
                    if (mesID > id)
                        id = mesID;
                }
            }
            return id;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load Config from Registry
            Config.LoadConfig();

            // If path is not set at startup, try find the file from the game
            if (string.IsNullOrEmpty(App.ScriptPath) && CurrentGame == Game.Unknown)
                App.ScriptPath = GetPathFromSteam();

            // Load Script Archive
            ScriptArchive = new PCKFile();
            if (!string.IsNullOrEmpty(App.ScriptPath))
            {
                ScriptArchive.Load(App.ScriptPath, true);
                // Load all the files into memory instead of streaming them
                ScriptArchive.Preload();

                // Load replacement table
                string replacementTablePath = Path.Combine(Path.GetDirectoryName(App.ScriptPath), "replace.ini");
                if (File.Exists(replacementTablePath))
                    App.StringProcess.Load(File.ReadAllText(replacementTablePath, Encoding.UTF8));

                // Load script from config or use default
                string fileName = !string.IsNullOrEmpty(Config.LastOpenedScript) && ScriptArchive.FileEntries.Any(t =>
                    t.FileName.ToLowerInvariant() == Config.LastOpenedScript.ToLowerInvariant())
                    ? Config.LastOpenedScript : ScriptArchive.FileEntries.First().FileName;
                LoadScript(fileName);
            }

            DataContext = null;
            DataContext = this;
            var dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += PreviewTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Config.SaveConfig();
        }

        private void PreviewTimer_Tick(object sender, EventArgs e)
        {
            if (Preview != null)
            {
                string scriptName = Preview.GetScriptName();
                if (ScriptFile.ScriptName != scriptName && !string.IsNullOrEmpty(scriptName))
                {
                    var index = ScriptArchiveFiles.FindIndex(t => t.FileName.Contains(scriptName));
                    ScriptListBox.SelectedIndex = index;
                }
            }
        }

        private void ScriptListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListBox;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected
            if (Preview != null)
            {
                string scriptName = Preview.GetScriptName();
                if (ScriptArchiveFiles[list.SelectedIndex].FileName.Contains(scriptName))
                {
                    LoadScript(ScriptArchiveFiles[list.SelectedIndex].FileName);
                    if (ScriptListView.Items.Count > 0)
                        ScriptListView.ScrollIntoView(ScriptListView.Items[0]);
                    return;
                }
                // Script that user is trying to load is not the right script
                var currentScript = ScriptArchiveFiles.FindIndex(t => t.FileName.Contains(scriptName));
                if (currentScript != -1)
                {
                    MessageBox.Show("The Script you are trying to load does not match with the game\n" +
                        "Switching to \"" + ScriptArchiveFiles[currentScript].FileName + "\"...", "Script Mismatch",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    list.SelectedIndex = currentScript;
                    return;
                }
            }
            LoadScript(ScriptArchiveFiles[list.SelectedIndex].FileName);
            if (ScriptListView.Items.Count > 0)
                ScriptListView.ScrollIntoView(ScriptListView.Items[0]);
        }

        private void CodeListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListBox;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected
            
            STSCFileDialogue.DialogueCode code;
            if (IsScriptv2)
                code = ScriptFilev2.DialogueCodes[list.SelectedIndex];
            else
                code = ScriptFile.DialogueCodes[list.SelectedIndex];
            
            switch (code.Type)
            {
                case "Title":
                    break;
                case "Voice":
                    break;
                case "Mes":
                case "Msg":
                    if (new PropertyEditorMsg(code).ShowDialog() == true)
                    {
                        // Mark script as edited
                        ScriptEdited = true;
                        if (Preview != null && Preview.CheckGameReadyState())
                        {
                            // Builds the script then injects it into the games memory
                            Preview.BuildAndInject(ScriptFile);
                            // Gets the address of the message
                            int addr = ScriptFile.FindAddress(code.Index);
                            // Goes further back if possible to cover the titles and voices
                            if (ScriptFile.Instructions[code.Index - 1].Name == "PlayVoice")
                                if (ScriptFile.Instructions[code.Index - 2].Name == "MesTitle")
                                    addr = ScriptFile.FindAddress(code.Index - 2);
                                else
                                    addr = ScriptFile.FindAddress(code.Index - 1);
                            else
                                if (ScriptFile.Instructions[code.Index - 1].Name == "MesTitle")
                                addr = ScriptFile.FindAddress(code.Index - 1);
                            // 
                            Preview.SetInstructionPointerMessage(addr);
                        }
                    }
                    break;
                case "Name":
                    break;
                case "FileJump":
                case "Script":
                    if (Preview != null)
                        Preview.SetInstructionPointerMessage(ScriptFile.FindAddress(code.Index));
                    else
                        ScriptListBox.SelectedIndex = ScriptArchiveFiles.FindIndex(t => t.FileName.Contains(code.ID));
                    break;
                case "Map":
                    // Open Editor
                    if (new PropertyEditorMapPlace(code).ShowDialog() == true)
                        ScriptEdited = true; // Mark script as edited
                    break;
                case "Choice":
                    // Check if the choice links to a FileJump
                    int instIndex = ScriptFile.FindIndex(int.Parse(code.ID));
                    string scriptFile = "Not a FileJump Choice";
                    for (int i = instIndex; i < instIndex + 30; ++i)
                        if (ScriptFile.Instructions[i].Name == "FileJump")
                        {
                            // Write the path to the script in which the choice will load
                            scriptFile = ScriptFile.Instructions[i].GetArgument<string>(0);
                            // Append .bin to the name to reduce confusion
                            scriptFile += ".bin";
                            // Append the amount of steps to the FileJump
                            if (i != instIndex)
                                scriptFile += $" ({i - instIndex} Step(s))";
                            break;
                        }

                    // Open Editor
                    if (new PropertyEditorChoice(code, scriptFile).ShowDialog() == true)
                        ScriptEdited = true; // Mark script as edited

                    break;
                default:
                    break;
            }
            // Update the title so we can tell the user that the file has been edited
            UpdateTitle();
        }

        private void CodeListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (!(sender is ListViewItem listItem))
            {
                e.Handled = true;
                return; // Return if no item is selected
            }
            var code = listItem.Content as STSCFileDialogue.DialogueCode;

            var addMesMenuItem = App.FindChild<MenuItem>(listItem.ContextMenu, "AddMesMenuItem");

            if (IsScriptv2)
            {
                // NOTE: Maybe change this?
                addMesMenuItem.IsEnabled = code.Type == "Mes";
            }
            else
            {
                // Scriptv1 not yet supported
                addMesMenuItem.IsEnabled = false;
            }
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CodeListView_MouseDoubleClick(sender, null);
        }

        private void AddMesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var code = ScriptListView.SelectedValue as STSCFileDialogue.DialogueCode;
            if (code == null)
            {
                e.Handled = true;
                return; // Return if no item is selected
            }

            if (CurrentGame == Game.DateALiveRenDystopia)
            {
                int index = code.Index;

                // Place below Name
                //  or place below MesWait
                if (code.Type == "Name")
                    index++;
                else if (ScriptFilev2.Sequence.Lines[index + 0].Name == "Mes" &&
                         ScriptFilev2.Sequence.Lines[index + 1].Name == "MesWait")
                    index += 2;

                // Lines
                var lineMes = new STSC2Commands.Command("Mes", STSC2Commands.DALRDCommands[0x40].ArgumentTypes);
                lineMes.SetCmdLineInfo(new byte[6]);
                lineMes.arguments.Add(new STSC2Node(240)); // X
                lineMes.arguments.Add(new STSC2Node(860)); // Y
                lineMes.arguments.Add(new STSC2Node(0)); // idk
                lineMes.arguments.Add(""); // Text
                lineMes.arguments.Add(GetUnusedMesID()); // ID
                var lineMesWait = new STSC2Commands.Command("MesWait", null);
                lineMesWait.SetCmdLineInfo(new byte[6]);

                ScriptFilev2.Sequence.Lines.InsertRange(index, new[] { lineMes, lineMesWait });
                ScriptFilev2.ConvertToDialogueCode();
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var code = ScriptListView.SelectedValue as STSCFileDialogue.DialogueCode;
            if (code == null)
            {
                e.Handled = true;
                return; // Return if no item is selected
            }

            if (CurrentGame == Game.DateALiveRenDystopia)
            {
                int index = code.Index;

                switch (ScriptFilev2.Sequence.Lines[index + 0].Name)
                {
                    case "Name / NameOff":
                        ScriptFilev2.Sequence.Lines.RemoveAt(index);
                        break;
                    case "Mes":
                        if (ScriptFilev2.Sequence.Lines[index + 1].Name == "MesWait")
                            ScriptFilev2.Sequence.Lines.RemoveAt(index + 1);
                        ScriptFilev2.Sequence.Lines.RemoveAt(index);
                        break;
                    default:
                        break;
                }

                ScriptFilev2.ConvertToDialogueCode();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Save the current script
            SaveScript();
            // Saves all the files onto disk
            ScriptArchive.Save(App.ScriptPath);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Sting Pack (*.pck)|*.pck"
            };
            if (ofd.ShowDialog() == true && !string.IsNullOrEmpty(ofd.FileName))
            {
                RefreshDatabase = true;

                // Load Script Archive
                ScriptArchive = new PCKFile();
                ScriptArchive.Load(App.ScriptPath = ofd.FileName, true);
                ScriptArchive.Preload();

                // Load script from config or use default
                string fileName = !string.IsNullOrEmpty(Config.LastOpenedScript) && ScriptArchive.FileEntries.Any(t =>
                    t.FileName.ToLowerInvariant() == Config.LastOpenedScript.ToLowerInvariant())
                    ? Config.LastOpenedScript : ScriptArchive.FileEntries.First().FileName;
                LoadScript(fileName);

                // Reset all the bindings
                DataContext = null;
                DataContext = this;
            }
        }

        private void LivePreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (Preview == null && new LivePreviewWarningWindow().ShowDialog() == true)
            { // Enable Live Preview (Currently not active and user selected Enable)
                Preview = new DALRRLivePreview();
                if (!Preview.Attach())
                {
                    Preview?.Detach();
                    Preview = null;
                    MessageBox.Show("Failed to attach to DATE A LIVE: Rio Reincarnation!\n" +
                        "Please make sure you have the game running with version 1.00.02 and currently in a scene", "Failed to start DAL: RR Live Preview", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (Preview != null)
            { // Disable Live Preview (Currently active)
                Preview?.Detach();
                Preview = null;
            }
            button.Content = Preview == null ? "Enable Live Preview" : "Disable Live Preview";
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void ImportTranslationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((MenuItem)sender)?.Tag as string;
            if (ScriptListBox.SelectedIndex == -1)
                return;
            var file = ScriptArchive.FileEntries[ScriptListBox.SelectedIndex];
            var ofd = new OpenFileDialog
            {
                Filter = GenerateFilters(true)
            };
            if (ofd.ShowDialog() == true)
            {
                // Gets the index of the filetype
                int index = TranslationSTSCHandler.FileTypes.ToList().FindIndex(t => ofd.FileName.Contains(t.TypeExtension));
                // if importer is not found
                if (index == -1)
                {
                    MessageBox.Show("Could not detect the file type!", "Import Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // TODO: Add key option
                // Import translation
                try
                {
                    string data = "";
                    using (var stream = File.Open(ofd.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader reader = new StreamReader(stream))
                        data = reader.ReadToEnd();
                    if (IsScriptv2)
                        TranslationSTSCHandler.ImportTranslation(index, ScriptFilev2, data, !tag.Contains("nokey"), App.StringProcess);
                    else
                        TranslationSTSCHandler.ImportTranslation(index, ScriptFile, data, !tag.Contains("nokey"), App.StringProcess);
                    // Save the script back into the archive
                    using (var stream = new MemoryStream())
                    {
                        if (IsScriptv2)
                        {
                            ScriptFilev2.ConvertToDialogueCode();
                            ScriptFilev2.Save(stream);
                        }
                        else
                        {
                            ScriptFile.ConvertToDialogueCode();
                            ScriptFile.Save(stream);
                        }
                        ScriptArchive.ReplaceFile(file.FileName, stream.ToArray());
                    }
                    // Reload the current script
                    LoadScript(ScriptArchive.FileEntries[ScriptListBox.SelectedIndex].FileName);
                }
                catch
                {
                    MessageBox.Show("Failed to open file! Possible another program is using it.", "Import Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportTranslationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptListBox.SelectedIndex == -1)
                return;
            var file = ScriptArchive.FileEntries[ScriptListBox.SelectedIndex];
            var sfd = new SaveFileDialog
            {
                Filter = GenerateFilters(false),
                FileName = Path.GetFileNameWithoutExtension(file.FileName)
            };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    // Gets the index of the filetype
                    int index = TranslationSTSCHandler.FileTypes.ToList().FindIndex(t => sfd.FileName.Contains(t.TypeExtension));

                    if (IsScriptv2)
                    {
                        var script = new STSC2File();
                        using (var stream = ScriptArchive.GetFileStream(file.FileName))
                            script.Load(stream);
                        File.WriteAllText(sfd.FileName, TranslationSTSCHandler.ExportTranslation(index, ScriptFilev2, CharacterNames, App.StringProcess), new UTF8Encoding(true));
                    }
                    else
                    {
                        var script = new STSCFile();
                        using (var stream = ScriptArchive.GetFileStream(file.FileName))
                            script.Load(stream);
                        File.WriteAllText(sfd.FileName, TranslationSTSCHandler.ExportTranslation(index, script, ScriptDB, App.StringProcess), new UTF8Encoding(true));
                    }
                }
                catch
                {
                    MessageBox.Show($"Failed to export.", "Export Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BatchImportTranslationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((MenuItem)sender)?.Tag as string;
            var sfd = new SaveFileDialog
            {
                Filter = GenerateFilters(false),
                FileName = "Select a file type and the folder which contains the translation files (the folder containing the 1st, 2nd and 3rd folder) and press Save",
                Title = "Batch Import"
            };
            if (sfd.ShowDialog() == true)
            {
                // Path to the directory which contains all the translation files
                string dir = Path.GetDirectoryName(sfd.FileName);
                // Gets the index of the filetype
                int index = sfd.FilterIndex - 1;
                foreach (var entry in ScriptArchive.FileEntries)
                {
                    var script = new STSCFile();
                    var scriptv2 = new STSC2File();
                    try
                    { 
                        if (IsScriptv2)
                            using (var stream = ScriptArchive.GetFileStream(entry.FileName))
                                scriptv2.Load(stream);
                        else
                            using (var stream = ScriptArchive.GetFileStream(entry.FileName))
                                script.Load(stream);
                    }
                    catch
                    {
                        MessageBox.Show($"Failed to import {entry.FileName}. Skipping.", "Import Error!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        continue;
                    }
                    // Path to the script file
                    string filepath = Path.ChangeExtension(Path.Combine(dir, entry.FileName), TranslationSTSCHandler.FileTypes[index].TypeExtension);
                    // Skip script if file does not exist
                    if (!File.Exists(filepath))
                        continue;
                    // Import translation
                    try
                    {
                        string data = "";
                        using (var stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        using (StreamReader reader = new StreamReader(stream))
                            data = reader.ReadToEnd();
                        if (IsScriptv2)
                        {
                            TranslationSTSCHandler.ImportTranslation(index, scriptv2, data, !tag.Contains("nokey"), App.StringProcess);
                            // Save the script back into the archive
                            using (var stream = new MemoryStream())
                            {
                                scriptv2.Save(stream);
                                ScriptArchive.ReplaceFile(entry.FileName, stream.ToArray());
                            }
                        }
                        else
                        {
                            TranslationSTSCHandler.ImportTranslation(index, script, data, !tag.Contains("nokey"), App.StringProcess);
                            // Save the script back into the archive
                            using (var stream = new MemoryStream())
                            {
                                script.Save(stream);
                                ScriptArchive.ReplaceFile(entry.FileName, stream.ToArray());
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Failed to open file! Possible another program is using it.", "Import Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            // Reload the current script
            if (ScriptListBox.SelectedIndex == -1)
                return;
            LoadScript(ScriptArchive.FileEntries[ScriptListBox.SelectedIndex].FileName);
        }

        private void BatchExportTranslationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog
            {
                Filter = GenerateFilters(false),
                FileName = "Select a file type and folder to store all the text files and press Save",
                Title = "Batch Export"
            };
            if (sfd.ShowDialog() == true)
            {
                // Path to the directory which will contain all the translation files
                string dir = Path.GetDirectoryName(sfd.FileName);
                // Gets the index of the filetype
                var index = sfd.FilterIndex - 1;
                foreach (var entry in ScriptArchive.FileEntries)
                {
                    try
                    {
                        if (IsScriptv2)
                        {
                            var script = new STSC2File();
                            using (var stream = ScriptArchive.GetFileStream(entry.FileName))
                                script.Load(stream);
                            string filepath = Path.ChangeExtension(Path.Combine(dir, entry.FileName),
                                TranslationSTSCHandler.FileTypes[index].TypeExtension);
                            // Create the directory
                            Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                            File.WriteAllText(filepath, TranslationSTSCHandler.ExportTranslation(index, script, CharacterNames, App.StringProcess), new UTF8Encoding(true));
                        }
                        else
                        {
                            var script = new STSCFile();
                            using (var stream = ScriptArchive.GetFileStream(entry.FileName))
                                script.Load(stream);
                            string filepath = Path.ChangeExtension(Path.Combine(dir, entry.FileName),
                                TranslationSTSCHandler.FileTypes[index].TypeExtension);
                            // Create the directory
                            Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                            File.WriteAllText(filepath, TranslationSTSCHandler.ExportTranslation(index, script, ScriptDB, App.StringProcess), new UTF8Encoding(true));
                        }
                    }
                    catch
                    {
                        MessageBox.Show($"Failed to export {entry.FileName}. Skipping.", "Export Error!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
        }

        private void ImportTranslationWorkbookMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string tag = ((MenuItem)sender)?.Tag as string;
            var sfd = new OpenFileDialog
            {
                Filter = "Excel Workbook(*.xlsx)|*.xlsx",
                FileName = "Script.xlsx",
                Title = "Import Excel Workbook"
            };
            if (sfd.ShowDialog() == true)
            {
                var workbook = Workbook.Load(sfd.FileName);

                foreach (var entry in ScriptArchive.FileEntries)
                {
                    var script = new STSCFile();
                    var scriptv2 = new STSC2File();
                    try
                    {
                        if (IsScriptv2)
                            using (var stream = ScriptArchive.GetFileStream(entry.FileName))
                                scriptv2.Load(stream);
                        else
                            using (var stream = ScriptArchive.GetFileStream(entry.FileName))
                                script.Load(stream);
                        }
                    catch
                    {
                        MessageBox.Show($"Failed to load {entry.FileName}. Skipping.", "Import Error!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        continue;
                    }
                    try
                    {
                        var worksheet = workbook.Worksheets.FirstOrDefault(t => entry.FileName.Contains(t.SheetName));
                        var lines = TranslationXLSXFile.ImportWorksheet(worksheet);

                        if (CurrentGame == Game.DateALiveRenDystopia)
                        {
                            TranslationSTSCHandler.ImportTranslation(lines, scriptv2, !tag.Contains("nokey"), App.StringProcess);
                            // Save the script back into the archive
                            using (var stream = new MemoryStream())
                            {
                                scriptv2.Save(stream);
                                ScriptArchive.ReplaceFile(entry.FileName, stream.ToArray());
                            }
                        }
                        else
                        {
                            TranslationSTSCHandler.ImportTranslation(lines, script, !tag.Contains("nokey"), App.StringProcess);
                            // Save the script back into the archive
                            using (var stream = new MemoryStream())
                            {
                                script.Save(stream);
                                ScriptArchive.ReplaceFile(entry.FileName, stream.ToArray());
                            }
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Failed to open file! Possible another program is using it.", "Import Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                // Reload the current script
                if (ScriptListBox.SelectedIndex == -1)
                    return;
                LoadScript(ScriptArchive.FileEntries[ScriptListBox.SelectedIndex].FileName);
            }
        }

        private string GenerateFilters(bool openfiledialog)
        {
            string filter = "";
            var filetypes = TranslationSTSCHandler.FileTypes;
            if (openfiledialog)
            {
                // Add filter for all supported files
                filter = "Supported Translation Table Files (";
                // Loop through all the supported file types
                for (int i = 0; i < filetypes.Count; ++i)
                {
                    if (i != 0)
                        filter += ';';
                    filter += $"*{filetypes[i].TypeExtension}";
                }
                filter += ")|";
                for (int i = 0; i < filetypes.Count; ++i)
                {
                    if (i != 0)
                        filter += ';';
                    filter += $"*{filetypes[i].TypeExtension}";
                }
                // Add each file type to its own filter
                for (int i = 0; i < filetypes.Count; ++i)
                    filter += $"|{filetypes[i].TypeName} Files (*{filetypes[i].TypeExtension})|*{filetypes[i].TypeExtension}";
            }
            else
            {
                // Add each file type to its own filter
                for (int i = 0; i < filetypes.Count; ++i)
                {
                    if (!string.IsNullOrEmpty(filter))
                        filter += '|';
                    filter += $"{filetypes[i].TypeName} File (*{filetypes[i].TypeExtension})|*{filetypes[i].TypeExtension}";
                }
            }
            return filter;
        }
    }
}
