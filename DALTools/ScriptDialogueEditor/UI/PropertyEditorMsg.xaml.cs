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

namespace ScriptDialogueEditor.UI
{
    /// <summary>
    /// Interaction logic for PropertyEditorMsg.xaml
    /// </summary>
    public partial class PropertyEditorMsg : Window
    {

        protected STSCFileDialogue.DialogueCode _code;
        public string NewMessage { get; set; }
        public string OldMessage { get; set; }

        public PropertyEditorMsg(STSCFileDialogue.DialogueCode code)
        {
            InitializeComponent();
            _code = code;
            // Converts "\n" to a carriage return and newline character
            NewMessage = code.Text.Replace("\\n", "\r\n");
            // Converts "\n" to a newline character
            OldMessage = code.Text;
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Gives the textbox focus so we can start typing as soon as the window loads
            MessageTextBox.Focus();
            // Set caret to the end so we can start working from the right
            MessageTextBox.CaretIndex = _code.Text.Length;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Convert newline characters to "\n"
            _code.Text = NewMessage.Replace("\r\n", "\\n");
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

    }
}
