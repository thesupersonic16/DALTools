using DALLib.File;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ScriptDatabaseEditor.UI
{
    /// <summary>
    /// Interaction logic for PCKFileSelectorWindow.xaml
    /// </summary>
    public partial class PCKFileSelectorWindow : Window
    {

        protected ObservableCollection<FileEntry> _files = new ObservableCollection<FileEntry>();

        public ObservableCollection<FileEntry> Files => _files;
        public PCKFile Archive { get; set; }
        public PCKFile.FileEntry PCKFileEntry { get; set; }
        public List<string> Directory = new List<string>();

        public PCKFileSelectorWindow(PCKFile pck)
        {
            InitializeComponent();
            Archive = pck;
            UpdateList();
        }

        public void UpdateList()
        {
            _files.Clear();
            DataContext = null;
            Archive.FileEntries.ForEach(t => Files.Add(CreateFileEntry(t)));
            // Remove Entries that are not inside of the directory
            for (int ii = 0; ii < Files.Count; ++ii)
            {
                for (int i = 0; i < Directory.Count; ++i)
                {
                    if (Files[ii].Directories.Count <= i || Files[ii].Directories[i] != Directory[i])
                    {
                        Files.RemoveAt(ii);
                        --ii;
                    }
                }
            }
            // Subdirectories
            var subdirs = new List<string>();
            for (int ii = 0; ii < Files.Count; ++ii)
            {
                // Check if the file is in the directory
                if (Files[ii].Directories.Count == Directory.Count)
                {
                    continue;
                }
                // Check if the file is in a sub directory
                if (Files[ii].Directories.Count > Directory.Count)
                {
                    if (!subdirs.Contains(Files[ii].Directories[Directory.Count]))
                        subdirs.Add(Files[ii].Directories[Directory.Count]);
                    Files.RemoveAt(ii);
                    --ii;
                }
            }
            // Add parent directory
            if (Directory.Count > 0)
            {
                var dirs = new List<string>(Directory);
                dirs.RemoveAt(dirs.Count - 1);
                Files.Insert(0, CreateDirectoryEntry(dirs, ".."));
            }
            // Add Sub Directories
            for (int i = 0; i < subdirs.Count; ++i)
            {
                var dirs = new List<string>(Directory);
                dirs.Add(subdirs[i]);
                Files.Add(CreateDirectoryEntry(dirs, subdirs[i]));
            }
            DataContext = this;
        }

        public void ProcessFileEntry(FileEntry entry)
        {
            if (entry.IsFile)
            {
                PCKFileEntry = entry.PCKFileEntry;
                DialogResult = true;
                Close();
            }
            else
            {
                Directory = entry.Directories;
                UpdateList();
            }
        }

        public FileEntry CreateDirectoryEntry(List<string> dirs, string fileName)
        {
            var entry = new FileEntry();
            entry.IsFile = false;
            entry.FileName = fileName;
            entry.Directories = dirs;
            return entry;

        }

        public FileEntry CreateFileEntry(PCKFile.FileEntry PCKEntry)
        {
            var entry = new FileEntry();
            entry.IsFile = true;
            entry.PCKFileEntry = PCKEntry;
            entry.FileName = System.IO.Path.GetFileName(PCKEntry.FileName);
            string[] dirs = PCKEntry.FileName.Split('/');
            for (int i = 0; i < dirs.Length - 1; ++i)
                entry.Directories.Add(dirs[i]);
            return entry;
        }

        public void OpenFile(ListBox listBox = null, string path = null)
        {
            if (listBox != null)
            {
                ProcessFileEntry(listBox.SelectedItem as FileEntry);
            }
            else if (!string.IsNullOrEmpty(path))
            {
                if (!Archive.FileEntries.Any(t => t.FileName == path))
                {
                    MessageBox.Show("File not found!", $"The file \"{path}\" does not exist in the archive!",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    PCKFileEntry = Archive.FileEntries.FirstOrDefault(t => t.FileName == path);
                    DialogResult = true;
                    Close();
                }
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listbox = sender as ListBox;
            if (listbox.SelectedIndex == -1)
                return;
            OpenFile(listbox);
        }

        public class FileEntry
        {
            public List<string> Directories = new List<string>();
            
            public bool IsFile { get; set; }

            public string FileName { get; set; }

            public PCKFile.FileEntry PCKFileEntry { get; set; }

            public override string ToString()
            {
                return FileName;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OpenFile(null, (sender as TextBox).Text);
            }
        }
    }
}
