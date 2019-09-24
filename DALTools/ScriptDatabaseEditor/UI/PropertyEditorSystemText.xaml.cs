using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ScriptDatabaseEditor
{
    /// <summary>
    /// Interaction logic for PropertyEditorSystemText.xaml
    /// </summary>
    public partial class PropertyEditorSystemText : Window
    {
        // TODO: add Bindings

        public ObservableCollection<string> SystemTextCollection { get; set; }
        public int SystemTextIndex { get; set; }
        public string SystemText { get; set; }

        public PropertyEditorSystemText(ObservableCollection<string> systemTextCollection, int index)
        {
            InitializeComponent();
            SystemTextCollection = systemTextCollection;
            SystemTextIndex = index;
            SystemText = SystemTextCollection[SystemTextIndex];
            PropertyTextBox.Text = SystemText;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PropertyTextBox.Focus();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SystemTextCollection[SystemTextIndex] = SystemText;
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void PropertyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ButtonSave_Click(sender, e);
        }

        private void PropertyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SystemText = PropertyTextBox.Text;
        }
    }
}
