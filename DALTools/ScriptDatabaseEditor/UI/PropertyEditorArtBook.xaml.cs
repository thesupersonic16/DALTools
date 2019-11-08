using DALLib.File;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using static DALLib.File.STSCFileDatabase;

namespace ScriptDatabaseEditor.UI
{
    /// <summary>
    /// Interaction logic for NewPropertyArtBook.xaml
    /// </summary>
    public partial class PropertyEditorArtBook : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ArtBookPageEntry ArtBookPage { get; set; }
        public ArtBookPageEntry OldArtBookPage { get; set; }

        public PCKFile ThumbnailArchive;
        public PCKFile DataArchive;

        public PropertyEditorArtBook(ArtBookPageEntry entry, PCKFile thumbnailPCK, PCKFile dataPCK)
        {
            InitializeComponent();
            ArtBookPage = new ArtBookPageEntry()
            {
                PagePathThumbnail = entry.PagePathThumbnail,
                PagePathData = entry.PagePathData,
                Name = entry.Name,
                ID = entry.ID,
                GameID = entry.GameID,
                Page = entry.Page
            };
            OldArtBookPage = entry;
            ThumbnailArchive = thumbnailPCK;
            DataArchive = dataPCK;
            DataContext = this;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            OldArtBookPage.PagePathThumbnail = ArtBookPage.PagePathThumbnail;
            OldArtBookPage.PagePathData = ArtBookPage.PagePathData;
            OldArtBookPage.Name = ArtBookPage.Name;
            OldArtBookPage.ID = ArtBookPage.ID;
            OldArtBookPage.GameID = ArtBookPage.GameID;
            OldArtBookPage.Page = ArtBookPage.Page;
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonThumbSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PCKFileSelectorWindow(ThumbnailArchive);
            if (dialog.ShowDialog() == true)
            {
                var entry = dialog.PCKFileEntry;
                ArtBookPage.PagePathThumbnail = entry.FileName.Substring(0, entry.FileName.IndexOf("."));
            }
        }

        private void ButtonDataSelect_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PCKFileSelectorWindow(DataArchive);
            if (dialog.ShowDialog() == true)
            {
                var entry = dialog.PCKFileEntry;
                ArtBookPage.PagePathData = entry.FileName.Substring(0, entry.FileName.IndexOf("."));
            }
        }
    }
}
