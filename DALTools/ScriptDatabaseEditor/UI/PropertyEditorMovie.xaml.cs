#pragma warning disable CS0067
using DALLib.File;
using DALLib.Imaging;
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
using static DALLib.File.STSCFileDatabase;

namespace ScriptDatabaseEditor
{
    /// <summary>
    /// Interaction logic for PropertyEditorMovie.xaml
    /// </summary>
    public partial class PropertyEditorMovie : Window, INotifyPropertyChanged
    {

        protected Game _game;

        public event PropertyChangedEventHandler PropertyChanged;

        public MovieEntry Movie { get; set; }
        public MovieEntry OldMovie { get; set; }

        public string Note { get; set; }

        protected TEXFile _texture;
        protected PCKFile _archive;
        public PropertyEditorMovie(MovieEntry entry, Game game, PCKFile archive)
        {
            InitializeComponent();
            Movie = new MovieEntry() { FriendlyName = entry.FriendlyName, ID = entry.ID, FilePath = entry.FilePath, Unknown4 = entry.Unknown4, GameID = entry.GameID, Unknown5 = entry.Unknown5 };
            OldMovie = entry;
            _game = game;
            _archive = archive;
            DataContext = this;
        }

        // TODO: Update image when path changes
        public void UpdateImage()
        {
            if (_archive != null)
            {
                var file = _archive.GetFileData($"{Movie.FilePath}.tex");
                if (file == null)
                    return;
                using (var stream = new MemoryStream(file))
                    (_texture = new TEXFile()).Load(stream);
                ThumbnailImage.Source = ImageTools.ConvertToSource(_texture.CreateBitmap());
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            OldMovie.FriendlyName = Movie.FriendlyName;
            OldMovie.ID = Movie.ID;
            OldMovie.FilePath = Movie.FilePath;
            OldMovie.Unknown4 = Movie.Unknown4;
            OldMovie.GameID = Movie.GameID;
            OldMovie.Unknown5 = Movie.Unknown5;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Note = $"Movie will be loaded at \"{_game.LangPathRel}\\Movie\\{((TextBox)sender).Text.Replace("_", "__")}.movie\"";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateImage();
        }
    }
}
