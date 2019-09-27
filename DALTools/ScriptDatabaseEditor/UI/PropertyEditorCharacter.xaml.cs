using DALLib.File;
using DALLib.Imaging;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using static ScriptDatabaseEditor.STSCFileDatabase;

namespace ScriptDatabaseEditor
{
    /// <summary>
    /// Interaction logic for PropertyEditorCharacter.xaml
    /// </summary>
    public partial class PropertyEditorCharacter : Window
    {

        public CharacterEntry Character { get; set; }
        public CharacterEntry OldCharacter { get; set; }

        protected TEXFile _texture;
        protected PCKFile _archive;
        public PropertyEditorCharacter(CharacterEntry entry, PCKFile archive)
        {
            InitializeComponent();
            Character = new CharacterEntry() { FriendlyName = entry.FriendlyName, ID = entry.ID };
            OldCharacter = entry;
            _archive = archive;
            DataContext = this;
        }
        // TODO: Update Image when ID changes
        public void UpdateImage()
        {
            if (_archive != null)
            {
                var file = _archive.GetFileData($"chrName{Character.ID:D3}.tex");
                if (file == null)
                    return;
                using (var stream = new MemoryStream(file))
                    (_texture = new TEXFile()).Load(stream);
                EventNameImage.Source = ImageTools.ConvertToSource(_texture.CreateBitmap());
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            OldCharacter.FriendlyName = Character.FriendlyName;
            OldCharacter.ID = Character.ID;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateImage();
        }
    }
}
