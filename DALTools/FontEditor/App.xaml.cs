using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FontEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static string[] Args;

        [STAThread]
        public static void Main(string[] args)
        {
            Args = args;
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
