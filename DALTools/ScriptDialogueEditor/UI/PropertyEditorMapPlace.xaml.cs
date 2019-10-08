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
    /// Interaction logic for PropertyEditorMapPlace.xaml
    /// </summary>
    public partial class PropertyEditorMapPlace : Window
    {
        public STSCFileDialogue.DialogueCode Code { get; set; }
        public string OldMessage { get; set; }

        public PropertyEditorMapPlace(STSCFileDialogue.DialogueCode code)
        {
            InitializeComponent();
            Code = code;
            OldMessage = code.Text;
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Gives the textbox focus so we can start typing as soon as the window loads
            MessageTextBox.Focus();
            // Set caret to the end so we can start working from the right
            MessageTextBox.CaretIndex = Code.Text.Length;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PropertyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ButtonSave_Click(sender, e);
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            if (DialogResult == null || DialogResult == false)
                Code.Text = OldMessage;
        }
    }
}
