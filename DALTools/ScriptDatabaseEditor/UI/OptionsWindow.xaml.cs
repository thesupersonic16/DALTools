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

namespace ScriptDatabaseEditor
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {

        public MainWindow MainWindow { get; set; }

        public OptionsWindow(MainWindow window)
        {
            InitializeComponent();
            MainWindow = window;
            DataContext = this;
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.CurrentConfig.SaveConfig();
            Close();
        }

        private void LG_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.LoadGameFiles();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            App.RunAnimations = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            App.RunAnimations = false;
        }
    }
}
