#pragma warning disable CS0067
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
using static ScriptDatabaseEditor.STSCFileDatabase;

namespace ScriptDatabaseEditor
{
    /// <summary>
    /// Interaction logic for PropertyEditorDramaCD.xaml
    /// </summary>
    public partial class PropertyEditorDramaCD : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public DramaCDEntry DramaCD { get; set; }
        public DramaCDEntry OldDramaCD { get; set; }

        public PropertyEditorDramaCD(DramaCDEntry entry)
        {
            InitializeComponent();
            DramaCD = new DramaCDEntry()
            {
                FileName = entry.FileName,
                FriendlyName = entry.FriendlyName,
                SourceAlbum = entry.SourceAlbum,
                InternalName = entry.InternalName,
                ID = entry.ID,
                Game = entry.Game,
                Unknown7 = entry.Unknown7,
                SourceTrackID = entry.SourceTrackID,
                Unknown9 = entry.Unknown9
            };
            OldDramaCD = entry;
            DataContext = this;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            OldDramaCD.FileName      = DramaCD.FileName;
            OldDramaCD.FriendlyName  = DramaCD.FriendlyName;
            OldDramaCD.SourceAlbum   = DramaCD.SourceAlbum;
            OldDramaCD.InternalName  = DramaCD.InternalName;
            OldDramaCD.ID            = DramaCD.ID;
            OldDramaCD.Game          = DramaCD.Game;
            OldDramaCD.Unknown7      = DramaCD.Unknown7;
            OldDramaCD.SourceTrackID = DramaCD.SourceTrackID;
            OldDramaCD.Unknown9      = DramaCD.Unknown9;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
