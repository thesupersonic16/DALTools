#pragma warning disable CS0067
using DALLib.File;
using DALLib.Imaging;
using Microsoft.Win32;
using ScriptDatabaseEditor.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
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

namespace ScriptDatabaseEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private STSCFileDatabase _stscDatabase        = new STSCFileDatabase();
        private PCKFile _nameTextureArchive           = new PCKFile();
        private PCKFile _movieTextureArchive          = new PCKFile();
        private PCKFile _CGThumbnailTextureArchive    = new PCKFile();
        private PCKFile _novelTextureArchive          = new PCKFile();
        private PCKFile _novelthumbnailTextureArchive = new PCKFile();

        private TEXFile _optionTexture              = new TEXFile();

        private Game _game;

        private Config _config = new Config();

        public Config Config => _config;
        public Game Game => _game;

        // Voice Names
        public ImageSource VN_BG { get; set; }
        public ImageSource VN_FR { get; set; }
        public ImageSource VN_CH { get; set; }

        // CG
        public ImageSource CG_IM { get; set; }

        // ArtBook
        public ImageSource AB_PG { get; set; }

        public STSCFileDatabase STSCDatabase => _stscDatabase;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Loads Game Resources into memory
        /// </summary>
        public void LoadGameFiles()
        {
            // Check if files should not be loaded
            if (!_game.EnableResourceLoading)
                return;
            try
            {
                // Load Archives and Textures
                _nameTextureArchive.Load(Path.Combine(_game.LangPath, "Event\\Name.pck"), true);
                _movieTextureArchive.Load(Path.Combine(_game.LangPath, "Extra\\mov\\movieThumb.pck"), true);
                _CGThumbnailTextureArchive.Load(Path.Combine(_game.GamePath, "Data\\Data\\Extra\\gal\\GalThumb.pck"), true);
                _novelTextureArchive.Load(Path.Combine(_game.LangPath, "Extra\\nov\\NovData.pck"), true);
                _novelthumbnailTextureArchive.Load(Path.Combine(_game.LangPath, "Extra\\nov\\NovThumb.pck"), true);
                (_optionTexture = new TEXFile()).Load(Path.Combine(_game.LangPath, "Init\\option.tex"), true);

                // Create and Set ImageSource
                VN_BG = ImageTools.ConvertToSource(_optionTexture.CreateBitmapFromFrame(27));
                VN_FR = ImageTools.ConvertToSource(_optionTexture.CreateBitmapFromFrame(26));
            }
            catch (Exception e)
            {
                // Disable Resources after failing the first time
                _game.EnableResourceLoading = false;
                // Show error
                new ExceptionWindow(e).ShowDialog();
            }
        }

        /// <summary>
        /// Recreates the Database object and Loads the new database file and stores the path
        /// </summary>
        /// <param name="path">Path to the database file</param>
        public void ChangeDatabase(string path)
        {
            // Load Database
            (_stscDatabase = new STSCFileDatabase()).Load(App.DataBasePath = path);

            // Rebind
            DataContext = null;
            DataContext = this;
        }

        /// <summary>
        /// Switches the language of reources to use
        /// </summary>
        /// <param name="lang">The Language to switch to</param>
        public void SwitchLanguage(GameLanguage lang)
        {
            _game.GameLanguage = lang;
            // Debug
            App.Debug_GameLanguage = lang;
            LoadGameFiles();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Load Config from Registry
            _config.LoadConfig();
            List<SteamGame> games = new List<SteamGame>();
            try
            {
                // Reads Steam information
                Steam.Init();

                // Searches for DATE A LIVE: Rio Reincarnation
                games = Steam.SearchForGames("DATE A LIVE: Rio Reincarnation");
            }catch { }

            if (games.Count != 0)
                _game = new Game(games[0].RootDirectory, GameLanguage.English);
            else
                _game = new Game("", GameLanguage.English);

            // Debug
            App.Debug_GamePath = _game?.GamePath;
            App.Debug_GameLanguage = _config.DefaultGameLanguage;


            // Set Default Language
            _game.GameLanguage = _config.DefaultGameLanguage;
            App.RunAnimations = _config.EnableAnimations;

            // If not file was loaded, Load default
            if (string.IsNullOrEmpty(App.DataBasePath))
                App.DataBasePath = Path.Combine(_game.LangPath, @"Script\database.bin");

            if (File.Exists(App.DataBasePath))
                ChangeDatabase(App.DataBasePath);
            LoadGameFiles();
        }

        private void CharactersListView_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected
            new PropertyEditorCharacter(_stscDatabase.Characters[list.SelectedIndex], _nameTextureArchive).ShowDialog();
        }

        private void MoviesListView_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected
            new PropertyEditorMovie(_stscDatabase.Movies[list.SelectedIndex], _game, _movieTextureArchive).ShowDialog();
        }

        private void ST_ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListBox;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected
            new PropertyEditorSystemText(_stscDatabase.SystemText, list.SelectedIndex).ShowDialog();
        }

        private void ME_ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListBox;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected
            new PropertyEditorMemory(_stscDatabase.Memories[list.SelectedIndex]).ShowDialog();
        }

        private void VN_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected

            // Check if files should not be loaded
            if (!_game.EnableResourceLoading)
                return;

            int id = (list.SelectedItem as STSCFileDatabase.VoiceEntry).ID;

            // Check if ID is in range
            if (id < 0 || id >= Consts.VOICETOFRAMEID.Length)
                return;

            // Change Character Image
            VN_CH = ImageTools.ConvertToSource(_optionTexture.CreateBitmapFromFrame(Consts.VOICETOFRAMEID[id]));
        }

        private void CG_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected

            // Check if files should not be loaded
            if (!_game.EnableResourceLoading)
                return;

            int id = (list.SelectedItem as STSCFileDatabase.CGEntry).CGID;
            var game = (list.SelectedItem as STSCFileDatabase.CGEntry).GameID;
            string filepath = Path.Combine(_game.GamePath, $"Data\\Data\\Ma\\{Consts.GAMEDIRNAME[(int)game]}\\MA{id:D6}.pck");

            if (_CGThumbnailTextureArchive.GetFileData($"{Consts.GAMEDIRNAME[(int)game]}/MA{id:D6}.tex") is byte[] data)
            {
                // Change Character Image
                var tex = new TEXFile();
                using (var stream = new MemoryStream(data))
                    tex.Load(stream);

                CG_IM = ImageTools.ConvertToSource(tex.CreateBitmap());
            }
            try
            {
                if (File.Exists(filepath))
                {
                    var pck = new PCKFile();
                    pck.Load(filepath, true);
                    CG_ExportImage.Visibility = pck.FileEntries.Where(t => t.FileName.EndsWith(".tex")).Count() == 1 ? Visibility.Visible : Visibility.Collapsed;
                    pck.Dispose();
                }
                else
                    CG_ExportImage.Visibility = Visibility.Collapsed;
            }
            catch
            {
                MessageBox.Show("Failed to open the CG archive. DAL: RR may also crash on this CG, Reinstalling the game is recommended!", "CG Load Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                CG_ExportImage.Visibility = Visibility.Collapsed;
            }

        }

        private void CG_ExportThumbButton_Click(object sender, RoutedEventArgs e)
        {
            // Show error if DAL: RR is not installed as its needed to export
            if (!_game.EnableResourceLoading)
            {
                MessageBox.Show("This feature requires DATE A LIVE: Rio Reincarnation to be installed!", "DAL: RR not found!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sfd = new SaveFileDialog();
            sfd.Filter = "Portable Network Graphics (*.png)|*.png";
            if (sfd.ShowDialog(this) == true)
            {

                if (CG_ListView.SelectedIndex == -1)
                    return; // Return if no item is selected
                int id = (CG_ListView.SelectedItem as STSCFileDatabase.CGEntry).CGID;
                var game = (CG_ListView.SelectedItem as STSCFileDatabase.CGEntry).GameID;

                if (_CGThumbnailTextureArchive.GetFileData($"{Consts.GAMEDIRNAME[(int)game]}/MA{id:D6}.tex") is byte[] data)
                {
                    // Change Character Image
                    var tex = new TEXFile();
                    using (var stream = new MemoryStream(data))
                        tex.Load(stream);

                    tex.SaveSheetImage(sfd.FileName);
                }
            }

        }

        private void AB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected

            // Check if files should not be loaded
            if (!_game.EnableResourceLoading)
                return;

            string path = (list.SelectedItem as STSCFileDatabase.ArtBookPageEntry).PagePathData;

            if (_novelTextureArchive.GetFileData($"{path}.tex") is byte[] data)
            {
                var tex = new TEXFile();
                using (var stream = new MemoryStream(data))
                    tex.Load(stream);

                AB_PG = ImageTools.ConvertToSource(tex.CreateBitmap());
                tex.Dispose();
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            _stscDatabase.Save(App.DataBasePath);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "DAL Database Script (*database*.bin)|*database*.bin|DAL Script (*.bin)|*.bin";
            if (ofd.ShowDialog() == true)
                ChangeDatabase(ofd.FileName);
        }

        private void ButtonOptions_Click(object sender, RoutedEventArgs e)
        {
            new OptionsWindow(this).ShowDialog();
        }

        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void DC_ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListBox;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected
            new PropertyEditorDramaCD(_stscDatabase.DramaCDs[list.SelectedIndex]).ShowDialog();
        }

        private void VN_ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListBox;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected
            new PropertyEditorVoiceName(_stscDatabase.Voices[list.SelectedIndex], this).ShowDialog();
        }

        private void AB_ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var list = sender as ListBox;
            if (list.SelectedIndex == -1)
                return; // Return if no item is selected
            new PropertyEditorArtBook(_stscDatabase.ArtBookPages[list.SelectedIndex], _novelthumbnailTextureArchive, _novelTextureArchive).ShowDialog();
        }

        private void AB_AddEntryButton_Click(object sender, RoutedEventArgs e)
        {
            var page = new STSCFileDatabase.ArtBookPageEntry();
            page.ID = _stscDatabase.ArtBookPages.Count;
            if (new PropertyEditorArtBook(page, _novelthumbnailTextureArchive, _novelTextureArchive).ShowDialog() == true)
                _stscDatabase.ArtBookPages.Add(page);
        }

        private void AB_SortPagesButton_Click(object sender, RoutedEventArgs e)
        {
            var pages = _stscDatabase.ArtBookPages.OrderBy(t => t.GameID).ThenBy(t => t.Page).ToList();
            _stscDatabase.ArtBookPages.Clear();
            for (int i = 0; i < pages.Count; ++i)
            {
                pages[i].ID = i;
                _stscDatabase.ArtBookPages.Add(pages[i]);
            }
        }

        private void CG_ExportImage_Click(object sender, RoutedEventArgs e)
        {
            // Show error if DAL: RR is not installed as its needed to export
            if (!_game.EnableResourceLoading)
            {
                MessageBox.Show("Resource loading is currently disabled. Make sure DAL: RR is installed correctly", "Resource Loading Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var sfd = new SaveFileDialog();
            sfd.Filter = "Portable Network Graphics (*.png)|*.png";
            if (sfd.ShowDialog(this) == true)
            {
                if (CG_ListView.SelectedIndex == -1)
                    return; // Return if no item is selected
                int id = (CG_ListView.SelectedItem as STSCFileDatabase.CGEntry).CGID;
                var game = (CG_ListView.SelectedItem as STSCFileDatabase.CGEntry).GameID;
                var width = (CG_ListView.SelectedItem as STSCFileDatabase.CGEntry).TextureWidth;
                var height = (CG_ListView.SelectedItem as STSCFileDatabase.CGEntry).TextureHeight;
                string filepath = Path.Combine(_game.GamePath, $"Data\\Data\\Ma\\{Consts.GAMEDIRNAME[(int)game]}\\MA{id:D6}.pck");

                var pck = new PCKFile();
                pck.Load(filepath, true);

                if (pck.GetFileData(pck.SearchForFile(".tex")) is byte[] data)
                {
                    var tex = new TEXFile();
                    using (var stream = new MemoryStream(data))
                    tex.Load(stream);
                    try
                    {
                        tex.CreateBitmap(0, 0, width, height).Save(sfd.FileName, ImageFormat.Png);
                    }
                    catch
                    {
                        tex.SaveSheetImage(sfd.FileName);
                    }
                }
                pck.Dispose();
            }
        }
    }
}
