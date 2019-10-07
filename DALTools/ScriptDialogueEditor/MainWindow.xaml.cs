using DALLib.File;
using DALLib.ImportExport;
using Microsoft.Win32;
using ScriptDialogueEditor.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
using Path = System.IO.Path;

namespace ScriptDialogueEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Config _config = new Config();

        public PCKFile ScriptArchive { get; set; }
        public STSCFileDialogue ScriptFile { get; set; }
        public STSCFileDatabase ScriptDB { get; set; }

        public List<PCKFile.FileEntry> ScriptArchiveFiles => ScriptArchive.FileEntries;

        public DALRRLivePreview Preview = null;

        public bool ScriptEdited = false;

        public STSCImportExportBase[] STSCIE = new STSCImportExportBase[] { new STSCTSVFile(), new GetTextFile() };

        public MainWindow()
        {
            InitializeComponent();
        }

        public void LoadScript(string scriptName, bool save = true)
        {
            if (ScriptEdited && save)
                SaveScript();
            // Load script file
            ScriptFile = new STSCFileDialogue(ScriptDB);
            ScriptFile.Load(ScriptArchive.GetFileStream(scriptName));
            // Write the script path to the config
            _config.LastOpenedScript = scriptName;
            // Mark script as not edited
            ScriptEdited = false;
            // Update the title to reflect the new loaded file
            UpdateTitle();
        }

        public void SaveScript()
        {
            if (ScriptFile != null)
            {
                using (var stream = new MemoryStream())
                {
                    // Builds the script file and saves it to the stream
                    ScriptFile.Save(stream);
                    // Replaces the file that was in the archive with the newly built script
                    ScriptArchive.ReplaceFile(_config.LastOpenedScript, stream.ToArray());
                }
                ScriptEdited = false;
            }
        }

        /// <summary>
        /// Updates the title of the window
        /// </summary>
        public void UpdateTitle()
        {
            string title = App.ProgramName;
            if (ScriptFile != null)
                title += $" - {_config.LastOpenedScript}";
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
                    return Path.Combine(games[0].RootDirectory, "Data/ENG/Script/Script.pck");
            }
            catch { }
            return "";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load Config from Registry
            _config.LoadConfig();

            // If path is not set at startup, try find the file from the game
            if (string.IsNullOrEmpty(App.ScriptPath))
                App.ScriptPath = GetPathFromSteam();

            // Load the database file if the script was found
            if (!string.IsNullOrEmpty(App.ScriptPath))
            {
                // Path to the database.bin file
                string scriptDB = Path.Combine(Path.GetDirectoryName(App.ScriptPath), "database.bin");
                // Check if the database exists
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
            // Load Script Archive
            ScriptArchive = new PCKFile();
            if (!string.IsNullOrEmpty(App.ScriptPath))
            {
                ScriptArchive.Load(App.ScriptPath, true);
                // Load all the files into memory instead of streaming them
                ScriptArchive.Preload();
                // Load script from config or use default
                LoadScript(string.IsNullOrEmpty(_config.LastOpenedScript) ? 
                    ScriptArchive.FileEntries.First().FileName : _config.LastOpenedScript);
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
            _config.SaveConfig();
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
                    return;
                }
                // Scipt that user is trying to load is not the right script
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

        }

        private void CodeListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListBox;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected
            var code = ScriptFile.DialogueCodes[list.SelectedIndex];
            switch (code.Type)
            {
                case "Title":
                    break;
                case "Voice":
                    break;
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
                case "Script":
                    if (Preview != null)
                        Preview.SetInstructionPointerMessage(ScriptFile.FindAddress(code.Index));
                    else
                        ScriptListBox.SelectedIndex = ScriptArchiveFiles.FindIndex(t => t.FileName.Contains(code.ID));
                    break;
                case "Opt":

                    // Check if the choice links to a FileJump
                    int instIndex = ScriptFile.FindIndex(int.Parse(code.ID));
                    string scriptFile = "Not a FileJump Choice";
                    for (int i = instIndex; i < instIndex + 5; ++i)
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
        }

        private void ListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CodeListView_MouseDoubleClick(sender, null);
        }

        private void CodeListView_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            var list = sender as ListBox;
            if (list.Items.Count != 0)
                list.ScrollIntoView(list.Items[0]);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Saves all the files onto disk
            ScriptArchive.Save(App.ScriptPath);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "DATE A LIVE Archives (*.pck)|*.pck"
            };
            if (ofd.ShowDialog() == true)
            {
                // Load the database file if the script was found
                if (!string.IsNullOrEmpty(ofd.FileName))
                {
                    // Path to the database.bin file
                    string scriptDB = Path.Combine(Path.GetDirectoryName(ofd.FileName), "database.bin");
                    // Check if the database exists
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
                // Load Script Archive
                ScriptArchive = new PCKFile();
                ScriptArchive.Load(ofd.FileName, true);
                // Load all the files into memory instead of streaming them
                ScriptArchive.Preload();
                // Load script from config or use default
                LoadScript(string.IsNullOrEmpty(_config.LastOpenedScript) ?
                    ScriptArchive.FileEntries.First().FileName : _config.LastOpenedScript);
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
            if (ScriptListBox.SelectedIndex == -1)
                return;
            var file = ScriptArchive.FileEntries[ScriptListBox.SelectedIndex];
            var ofd = new OpenFileDialog
            {
                Filter = GenerateFilters(true)
            };
            if (ofd.ShowDialog() == true)
            {
                // Gets the STSCIE
                var ie = STSCIE.FirstOrDefault(t => ofd.FileName.Contains(t.TypeExtension));
                // if importer is not found
                if (ie == null)
                {
                    MessageBox.Show("Could not detect the file type!", "Import Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Import translation
                ie.Import(ScriptFile, ScriptDB, File.ReadAllText(ofd.FileName));
                // Save the script back into the archive
                using (var stream = new MemoryStream())
                {
                    ScriptFile.ConvertToDialogueCode();
                    ScriptFile.Save(stream);
                    ScriptArchive.ReplaceFile(file.FileName, stream.ToArray());
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
                // Gets the STSCIE
                var ie = STSCIE[sfd.FilterIndex - 1];
                var script = new STSCFile();
                using (var stream = ScriptArchive.GetFileStream(file.FileName))
                    script.Load(stream);
                File.WriteAllText(sfd.FileName, ie.Export(script, ScriptDB));
            }
        }

        private string GenerateFilters(bool openfiledialog)
        {
            string filter = "";
            if (openfiledialog)
            {
                // Add filter for all supported files
                filter = "Supported STSCImportExport Files (";
                // Loop through all the supported STSCIE types
                for (int i = 0; i < STSCIE.Length; ++i)
                {
                    if (i != 0)
                        filter += ';';
                    filter += $"*{STSCIE[i].TypeExtension}";
                }
                filter += ")|";
                for (int i = 0; i < STSCIE.Length; ++i)
                {
                    if (i != 0)
                        filter += ';';
                    filter += $"*{STSCIE[i].TypeExtension}";
                }
                // Add each STSCIE to its own filter
                for (int i = 0; i < STSCIE.Length; ++i)
                    filter += $"|{STSCIE[i].TypeName} Files (*{STSCIE[i].TypeExtension})|*{STSCIE[i].TypeExtension}";
            }
            else
            {
                // Add each STSCIE to its own filter
                for (int i = 0; i < STSCIE.Length; ++i)
                {
                    if (!string.IsNullOrEmpty(filter))
                        filter += '|';
                    filter += $"{STSCIE[i].TypeName} File (*{STSCIE[i].TypeExtension})|*{STSCIE[i].TypeExtension}";
                }
            }
            return filter;
        }
    }
}
