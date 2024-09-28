using DALLib.File;
using DALLib.ImportExport;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TableEditor.UI;

namespace TableEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public TableFile Table { get; set; }
        
        protected List<ObservableCollection<string>> TableData { get; set; }
        protected string FilePath;

        public void UpdateUI()
        {
            TableData = new List<ObservableCollection<string>>();
            for (int i = 0; i < Table.Rows.Count; i++)
            {
                var column = new ObservableCollection<string>(Table.Rows[i]);
                TableData.Add(column);
            }

            // TODO: Fix header not shrinking
            Editor.ColumnHeadersSource = Table.Columns.Select((x, i) => x.Name).ToList();
            Editor.ItemsSource = TableData;
            Title = $"{App.ProgramName} - {Path.GetFileName(FilePath)}";
        }

        public void LoadTable(string path)
        {
            Table = new TableFile();
            Table.Load(path);

            FilePath = path;
            UpdateUI();
        }

        public void SaveTable(string path)
        {
            Table.Rows.Clear();
            foreach (var row in TableData)
                Table.Rows.Add(row.ToArray());

            using (var stream = new MemoryStream())
            {
                try
                {
                    Table.Save(stream);
                    File.WriteAllBytes(path, stream.ToArray());
                }
                catch (FormatException)
                {
                    MessageBox.Show("Not all fields are populated with\nthe correct type of data",
                        "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void ExportTable(string path, string ext)
        {
            string text = "";
            if (ext.ToLowerInvariant() == ".csv")
            {
                var file = new TranslationCSVFile();
                text += file.AddRow(Table.Columns.Select(x => x.Name).ToArray());
                Table.Rows.ForEach(x => text += file.AddRow(x));
            } else if (ext.ToLowerInvariant() == ".tsv")
            {
                var file = new TranslationTSVFile();
                text += file.AddRow(Table.Columns.Select(x => x.Name).ToArray());
                Table.Rows.ForEach(x => text += file.AddRow(x));
            }

            File.WriteAllText(path, text, Encoding.UTF8);
        }

        public void ImportTable(string path, string ext)
        {
            string text = File.ReadAllText(path, Encoding.UTF8);
            Table.Rows.Clear();
            if (ext.ToLowerInvariant() == ".csv")
            {
                var file = new TranslationCSVFile(text);
                file.ReadCSVRow(); // Skip header
                while (true)
                {
                    string[] split = file.ReadCSVRow();
                    if (split == null)
                        break; // EOF
                    Table.Rows.Add(split);
                }
            }
            else if (ext.ToLowerInvariant() == ".tsv")
            {
                var file = new TranslationTSVFile();
                file.ReadTSVRow(); // Skip header
                while (true)
                {
                    string[] split = file.ReadTSVRow();
                    if (split == null)
                        break; // EOF
                    Table.Rows.Add(split);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.Args.Length > 0 && File.Exists(App.Args[0]))
                LoadTable(App.Args[0]);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilePath == null)
            {
                MessageBox.Show("No table is loaded.", "Save Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var shiftKey = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (shiftKey)
            {
                var sfd = new SaveFileDialog
                {
                    Filter = "Table Files (*.bin)|*.bin",
                    FileName = FilePath
                };
                if (sfd.ShowDialog() == true)
                    SaveTable(sfd.FileName);
            }
            else
                SaveTable(FilePath);
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Table Files (*.bin)|*.bin"
            };
            if (ofd.ShowDialog() == true)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    LoadTable(ofd.FileName);
                });
            }
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (Table == null)
            {
                MessageBox.Show("No table is loaded.", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var sfd = new SaveFileDialog
            {
                Filter = "Comma-Separated Values (*.csv)|*.csv|Tab-Separated Values (*.tsv)|*.tsv",
                FileName = Path.ChangeExtension(FilePath, "csv")
            };
            if (sfd.ShowDialog() == true)
            {
                ExportTable(sfd.FileName, Path.GetExtension(sfd.FileName));
            }
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            if (Table == null)
            {
                MessageBox.Show("No table is loaded.", "Import Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var sfd = new OpenFileDialog
            {
                Filter = "Comma-Separated Values (*.csv)|*.csv|Tab-Separated Values (*.tsv)|*.tsv",
                FileName = Path.ChangeExtension(FilePath, "csv")
            };
            if (sfd.ShowDialog() == true)
                ImportTable(sfd.FileName, Path.GetExtension(sfd.FileName));
            UpdateUI();
        }
    }
}
