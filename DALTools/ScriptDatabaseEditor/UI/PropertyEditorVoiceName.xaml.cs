using System;
using System.Collections.Generic;
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
    /// Interaction logic for PropertyEditorVoiceName.xaml
    /// </summary>
    public partial class PropertyEditorVoiceName : Window
    {
        public MainWindow _parent { get; set; }

        public VoiceEntry Voice { get; set; }
        public VoiceEntry OldVoice { get; set; }


        public PropertyEditorVoiceName(VoiceEntry entry, MainWindow parent)
        {
            InitializeComponent();
            Voice = new VoiceEntry()
            {
                ID = entry.ID,
                KnownName = entry.KnownName,
                PreferedName = entry.PreferedName,
                UnknownName = entry.UnknownName
            };
            OldVoice = entry;
            _parent = parent;
            DataContext = this;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            OldVoice.ID           = Voice.ID;
            OldVoice.KnownName    = Voice.KnownName;
            OldVoice.PreferedName = Voice.PreferedName;
            OldVoice.UnknownName  = Voice.UnknownName;
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
