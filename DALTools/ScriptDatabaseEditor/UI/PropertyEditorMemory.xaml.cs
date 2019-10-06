using DALLib.Misc;
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
using static DALLib.File.STSCFileDatabase;
using static DALLib.File.STSCFileDatabase.MemoryEntry;

namespace ScriptDatabaseEditor
{
    /// <summary>
    /// Interaction logic for PropertyEditorMemory.xaml
    /// </summary>
    public partial class PropertyEditorMemory : Window
    {

        protected Game _game;

        public MemoryEntry Memory { get; set; }
        public MemoryEntry OldMemory { get; set; }

        public string Note { get; set; }

        public PropertyEditorMemory(MemoryEntry entry)
        {
            InitializeComponent();
            Memory = new MemoryEntry() { Description = entry.Description.Replace("\\n", "\n"), Game = entry.Game,
                GameID = entry.GameID, ID = entry.ID, Name = entry.Name };
            OldMemory = entry;
            DataContext = this;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            OldMemory.Name = Memory.Name;
            OldMemory.Description = Memory.Description.Replace("\n", "\\n").Replace("\r", "");
            OldMemory.ID = Memory.ID;
            OldMemory.GameID = Memory.GameID;
            if (Memory.GameID >= GameID.Rinne_Utopia)
                OldMemory.Game = (MemoryGame)Memory.GameID - 1;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
