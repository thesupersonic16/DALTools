using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DALLib.File;
using DALLib.Imaging;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace FontEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public TEXFile FontImageTexFile = new TEXFile();
        public PCKFile PCKFontArchive = new PCKFile();
        public FontFile FontCodeFile = new FontFile();
        public FNTFile FontTableFile = null;
        public List<Border> Borders = new List<Border>();

        public string FilePath = null;

        public int LastIndex = 0;
        public int HoverIndex = 0;

        public bool Dragging = false;
        public bool MonospaceOnly { get; set; } = false;


        public Point Anchor;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void UpdateBorderSize(int id)
        {
            if (id >= Borders.Count)
                return;
            if (UI_FontImage.Source == null)
                return;
            var character = FontCodeFile.Characters[id];
            double x = (character.XScale + character.Kerning / UI_FontImage.Source.Width) * UI_FontImage.ActualWidth;
            double y = character.YScale * UI_FontImage.ActualHeight;
            double w = (FontCodeFile.WidthScale + character.Width) * (UI_FontImage.ActualWidth / UI_FontImage.Source.Width);
            double h = FontCodeFile.CharacterHeight * (UI_FontImage.ActualHeight / UI_FontImage.Source.Height);
            Borders[id].Margin = new Thickness(x, y, 0, 0);
            Borders[id].Width = w;
            Borders[id].Height = h;
            Borders[id].HorizontalAlignment = HorizontalAlignment.Left;
            Borders[id].VerticalAlignment = VerticalAlignment.Top;
        }

        public void ReloadUI()
        {
            var entry = UI_CharactersListBox.SelectedItem as FontFile.FontEntry;
            RemoveAllBorders();
            UI_CharactersListBox.Items.Clear();
            FontCodeFile.Characters.ForEach(t => UI_CharactersListBox.Items.Add(t));
            AddAllBorder();
            if (UI_CharactersListBox.Items.Contains(entry))
                UI_CharactersListBox.SelectedItem = entry;
            for (int i = 0; i < FontCodeFile.Characters.Count; ++i)
                UpdateBorderSize(i);
        }

        public void AddAllBorder()
        {
            for (int i = 0; i < FontCodeFile.Characters.Count; ++i)
            {
                var border = new Border();
                border.Tag = i;
                border.MouseEnter += UIElement_OnMouseEnter;
                border.MouseLeave += UIElement_OnMouseLeave;
                border.MouseLeftButtonDown += UIElement_OnMouseLeftButtonDown;

                border.MouseRightButtonDown += UI_BorderDrag_OnMouseLeftButtonDown;
                border.MouseRightButtonUp += UI_BorderDrag_OnMouseLeftButtonUp;
                border.MouseMove += UI_BorderDrag_OnMouseMove;

                border.Background = new SolidColorBrush(Colors.Transparent);
                Borders.Add(border);
                UI_ImageGrid.Children.Add(border);
            }
        }

        public void RemoveAllBorders()
        {
            for (int i = 0; i < UI_ImageGrid.Children.Count; ++i)
            {
                if (UI_ImageGrid.Children[i] is Border)
                    UI_ImageGrid.Children.RemoveAt(i--);
            }

            Borders.Clear();
        }

        public void UpdatePreview()
        {
            var entry = UI_CharactersListBox.SelectedItem as FontFile.FontEntry;
            if (entry == null)
                return;
            double x = Math.Round(entry.XScale * UI_FontImage.Source.Width + entry.Kerning);
            double y = Math.Round(entry.YScale * (UI_FontImage.Source.Height));
            double w = entry.Width;
            double h = FontCodeFile.CharacterHeight;
            UI_CharTextBox.Text = entry.Character.ToString();
            UI_XTextBox.Text = x.ToString(CultureInfo.GetCultureInfo("en-US"));
            UI_YTextBox.Text = y.ToString(CultureInfo.GetCultureInfo("en-US"));
            UI_WTextBox.Text = entry.Width.ToString();
            UI_KTextBox.Text = entry.Kerning.ToString();
            UI_PreviewImage.Source = ImageTools.ConvertToSource(FontImageTexFile.CreateBitmap((int)x, (int)y, (int)w, (int)h));
        }

        public void SaveFont()
        {
            if (!string.IsNullOrEmpty(FilePath) && FontTableFile != null)
            {
                FontTableFile.Save(FilePath);
            }
            else if (!string.IsNullOrEmpty(FilePath) && FilePath.EndsWith(".pck"))
            {
                // PCK Save
                using (var stream = new MemoryStream())
                {
                    FontCodeFile.Save(stream);
                    PCKFontArchive.ReplaceFile(PCKFontArchive.SearchForFile(".code"), stream.ToArray());
                }
                PCKFontArchive.Save(FilePath);
            }
            else
            {
                // Code Save
                FontCodeFile.Save(FilePath);
            }
        }

        public void LoadFontPCK(string path)
        {
            FilePath = path;
            PCKFontArchive = new PCKFile();
            PCKFontArchive.Load(path);

            var textureFilePath = PCKFontArchive.SearchForFile(".tex");
            var fontCodeFilePath = PCKFontArchive.SearchForFile(".code");
            FontImageTexFile = new TEXFile();
            FontCodeFile = new FontFile();
            FontCodeFile.MonospaceOnly = MonospaceOnly;

            // Load Font Code
            if (fontCodeFilePath != null)
                FontCodeFile.Load(PCKFontArchive.GetFileStream(fontCodeFilePath));
            else
                MessageBox.Show("Failed to load Code File!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            if (textureFilePath != null)
            {
                // Load Texture
                FontImageTexFile.Load(PCKFontArchive.GetFileStream(textureFilePath));
                // Set Texture
                UI_FontImage.Source = ImageTools.ConvertToSource(FontImageTexFile.CreateBitmap());
                UI_ExportButton.IsEnabled = true;
            }
            else
                MessageBox.Show("Failed to load Texture!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            LastIndex = -1;
            HoverIndex = -1;
            
            // Reload
            ReloadUI();

            UI_SaveButton.IsEnabled = true;
        }

        public void LoadFontFNT(string path)
        {
            FilePath = path;

            FontTableFile = new FNTFile();
            FontTableFile.Load(path);

            FontCodeFile = FontTableFile.FontCode;
            FontImageTexFile = FontTableFile.FontTexture;

            UI_FontImage.Source = ImageTools.ConvertToSource(FontImageTexFile.CreateBitmap());

            LastIndex = -1;
            HoverIndex = -1;

            // Reload
            ReloadUI();

            UI_SaveButton.IsEnabled = true;
            UI_ExportButton.IsEnabled = true;
        }

        public void LoadFile(string path)
        {
            if (path == null)
                return;

            FontTableFile = null;

            // Check if its a PCK
            if (path.ToLowerInvariant().EndsWith(".pck"))
            {
                LoadFontPCK(path);
                return;
            }

            // Check if its a FNT
            if (path.ToLowerInvariant().EndsWith(".fnt"))
            {
                LoadFontFNT(path);
                return;
            }

            // Check if file is valid and make sure FilePath is the .code
            if (path.ToLowerInvariant().EndsWith("_data.tex") || path.ToLowerInvariant().EndsWith(".code"))
            {
                FilePath = path.Replace("_data.tex", ".code");
            }
            else
            {
                MessageBox.Show($"Unknown file type\n{path}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Recreate Objects
            FontImageTexFile = new TEXFile();
            FontCodeFile = new FontFile();
            FontCodeFile.MonospaceOnly = MonospaceOnly;

            try
            {
                // Load Font TEX
                FontImageTexFile.Load(FilePath.Replace(".code", "_data.tex"));
                UI_FontImage.Source = ImageTools.ConvertToSource(FontImageTexFile.CreateBitmap());
            }
            catch
            {
                MessageBox.Show("Failed to load Texture!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LastIndex = -1;
            HoverIndex = -1;

            // Load FontCode
            FontCodeFile.Load(FilePath);

            // Reload
            ReloadUI();

            UI_SaveButton.IsEnabled = true;
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void MainWindow_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update Scale of borders to match with the Image
            for (int i = 0; i < FontCodeFile.Characters.Count; ++i)
                UpdateBorderSize(i);
        }

        private void UI_CharactersListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
            if ((e.Source as ListBox).SelectedIndex == -1)
            {
                UI_CopyCharButton.IsEnabled = false;
                UI_DeleteCharButton.IsEnabled = false;
                return;
            }
            UI_CopyCharButton.IsEnabled = true;
            UI_DeleteCharButton.IsEnabled = true;
            if (LastIndex != -1)
                Borders[LastIndex].BorderThickness = new Thickness(0);
            Borders[LastIndex = (e.Source as ListBox).SelectedIndex].BorderThickness = new Thickness(1);
            if (HoverIndex == LastIndex)
                Borders[LastIndex].BorderBrush = new SolidColorBrush(Colors.Aqua);
            else
                Borders[LastIndex].BorderBrush = new SolidColorBrush(Colors.Red);
        }

        private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
        {
            int id = (int)(sender as Border).Tag;
            Borders[id].BorderThickness = new Thickness(1);
            Borders[id].BorderBrush = new SolidColorBrush(Colors.Aqua);
        }

        private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
        {
            int id = (int)(sender as Border).Tag;
            if (id == UI_CharactersListBox.SelectedIndex)
                Borders[id].BorderBrush = new SolidColorBrush(Colors.Red);
            else
                Borders[id].BorderThickness = new Thickness(0);
        }

        private void UIElement_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            int id = (int)(sender as Border).Tag;
            if (UI_CharactersListBox.SelectedIndex != id)
            {
                UI_CharactersListBox.SelectedIndex = id;
            }
        }

        private void UI_CharTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (UI_CharactersListBox.SelectedIndex >= Borders.Count || UI_CharactersListBox.SelectedIndex == -1)
                return;
            if (UI_CharTextBox.Text.Length == 0)
                return;
            FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].Character = UI_CharTextBox.Text.First();
            UpdateBorderSize(UI_CharactersListBox.SelectedIndex);
        }

        private void UI_XTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (UI_CharactersListBox.SelectedIndex >= Borders.Count || UI_CharactersListBox.SelectedIndex == -1)
                return;
            if (UI_XTextBox.Text.Length == 0)
                return;
            if (double.TryParse(UI_XTextBox.Text, out double output))
            {
                double xScale = (output - FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].Kerning) /
                               UI_FontImage.Source.Width;
                FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].XScale = (float)xScale;
            }
            FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].Character = UI_CharTextBox.Text.First();
            UpdateBorderSize(UI_CharactersListBox.SelectedIndex);
            UpdatePreview();
        }

        private void UI_YTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (UI_CharactersListBox.SelectedIndex >= Borders.Count || UI_CharactersListBox.SelectedIndex == -1)
                return;
            if (UI_YTextBox.Text.Length == 0)
                return;
            if (double.TryParse(UI_YTextBox.Text, out double output))
            {
                double yScale = output / UI_FontImage.Source.Height;
                FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].YScale = (float)yScale;
            }
            FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].Character = UI_CharTextBox.Text.First();
            UpdateBorderSize(UI_CharactersListBox.SelectedIndex);
            UpdatePreview();
        }

        private void UI_WTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (UI_CharactersListBox.SelectedIndex >= Borders.Count || UI_CharactersListBox.SelectedIndex == -1)
                return;
            if (UI_WTextBox.Text.Length == 0)
                return;
            if (int.TryParse(UI_WTextBox.Text, out int output) && output > 0)
            {
                FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].Width = output;
            }
            FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].Character = UI_CharTextBox.Text.First();
            UpdateBorderSize(UI_CharactersListBox.SelectedIndex);
            UpdatePreview();
        }

        private void UI_KTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (UI_CharactersListBox.SelectedIndex >= Borders.Count || UI_CharactersListBox.SelectedIndex == -1)
                return;
            if (UI_KTextBox.Text.Length == 0)
                return;
            if (int.TryParse(UI_KTextBox.Text, out int output))
            {
                FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].Kerning = output;
            }
            FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].Character = UI_CharTextBox.Text.First();
            UpdateBorderSize(UI_CharactersListBox.SelectedIndex);
            UpdatePreview();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "All Supported Files|*.code;*.pck;*.fnt|Font Code|*.code|PCK Archive|*.pck|Font Bundle|*.fnt";
            if (ofd.ShowDialog() == true)
            {
                LoadFile(ofd.FileName);
                var image = FontCodeFile.RenderFont(FontImageTexFile, " " + UI_TestTextBox.Text);
                UI_FontTest.Source = ImageTools.ConvertToSource(image);
            }
        }

        private void UI_FontImage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update Scale of borders to match with the Image
            for (int i = 0; i < FontCodeFile.Characters.Count; ++i)
                UpdateBorderSize(i);
        }

        private void UI_SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFont();
        }

        private void UI_NewCharButton_Click(object sender, RoutedEventArgs e)
        {
            FontCodeFile.Characters.Add(new FontFile.FontEntry() {Character = '-', Kerning = 10, Width = 10});
            ReloadUI();
        }

        private void UI_CopyCharButton_Click(object sender, RoutedEventArgs e)
        {
            var entry = UI_CharactersListBox.SelectedItem as FontFile.FontEntry;
            FontCodeFile.Characters.Add(new FontFile.FontEntry() { Character = entry.Character, Kerning = entry.Kerning, XScale = entry.XScale, YScale = entry.YScale, Width = entry.Width});
            ReloadUI();
        }

        private void UI_DeleteCharButton_Click(object sender, RoutedEventArgs e)
        {
            int index = UI_CharactersListBox.SelectedIndex;
            FontCodeFile.Characters.RemoveAt(index);
            LastIndex = -1;
            ReloadUI();
        }

        private void UI_LoadTextureButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "All Supported Files|*.tex|Texture|*.tex";
            if (ofd.ShowDialog() == true)
            {
                // Load Font TEX
                FontImageTexFile.Load(ofd.FileName);
                UI_FontImage.Source = ImageTools.ConvertToSource(FontImageTexFile.CreateBitmap());
                UI_ExportButton.IsEnabled = true;
            }
        }

        private void UI_ImportTextureButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "All Supported Files|*.tex;*.png|Texture|*.tex|Supported Image Files|*.png";
            if (ofd.ShowDialog() == true)
            {
                if (ofd.FileName.ToLowerInvariant().EndsWith(".tex"))
                {
                    var textureFilePath = PCKFontArchive.SearchForFile(".tex");
                    var newTexture = new TEXFile();

                    // Set Sigless
                    newTexture.Sigless = true;

                    // Load new Texture
                    newTexture.Load(ofd.FileName);

                    // Check Dimensions
                    if (FontImageTexFile.SheetData != null &&
                        (newTexture.SheetWidth != FontImageTexFile.SheetWidth ||
                         newTexture.SheetHeight != FontImageTexFile.SheetHeight))
                    {
                        if (MessageBox.Show("Texture dimensions do not match!\n" +
                                            "FontEditor currently does not support rescaling, Do you want to continue?",
                                "WARNING",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                            return;
                    }

                    // Copy texture to archive
                    if (FontTableFile != null)
                        FontTableFile.FontTexture = newTexture;
                    else
                        PCKFontArchive.ReplaceFile(textureFilePath, File.ReadAllBytes(ofd.FileName));

                    // Set Texture
                    UI_FontImage.Source = ImageTools.ConvertToSource((FontImageTexFile = newTexture).CreateBitmap());
                }
                else
                {
                    var textureFilePath = PCKFontArchive.SearchForFile(".tex");
                    var newTexture = new TEXFile();

                    // Set Sigless
                    newTexture.Sigless = true;
                    
                    // Load Image to Texture
                    newTexture.LoadSheetImage(ofd.FileName);

                    // Check Dimensions
                    if (FontImageTexFile.SheetData != null &&
                        (newTexture.SheetWidth != FontImageTexFile.SheetWidth ||
                         newTexture.SheetHeight != FontImageTexFile.SheetHeight))
                    {
                        if (MessageBox.Show("Texture dimensions do not match!\n" +
                                            "FontEditor currently does not support rescaling, Do you want to continue?",
                                "WARNING",
                                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                            return;
                    }


                    if (FontTableFile != null)
                        FontTableFile.FontTexture = newTexture;
                    else
                    {
                        // Save Texture to PCK
                        using (var stream = new MemoryStream())
                        {
                            newTexture.Save(stream);
                           PCKFontArchive.ReplaceFile(textureFilePath, stream.ToArray());
                        }
                    }
                    // Set Texture
                    UI_FontImage.Source = ImageTools.ConvertToSource((FontImageTexFile = newTexture).CreateBitmap());
                }
            }
        }

        private void UI_ExportTextureButton_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Portable Network Graphics (*.png)|*.png|Texture|*.tex";
            sfd.FileName = Path.ChangeExtension(Path.GetFileName(FilePath), "png");
            if (sfd.ShowDialog() == true)
            {
                if (sfd.FilterIndex == 1)
                    FontImageTexFile.SaveSheetImage(sfd.FileName);
                else
                    FontImageTexFile.Save(sfd.FileName);
            }
        }

        private void UI_BorderDrag_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (UI_CharactersListBox.SelectedIndex == -1)
                return;
            Dragging = true;
            Anchor = e.GetPosition(Borders[UI_CharactersListBox.SelectedIndex]);
        }

        private void UI_BorderDrag_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Dragging)
            {
                Dragging = false;
                var border = Borders[UI_CharactersListBox.SelectedIndex];
                var kerningScale =
                    FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].Kerning /
                    UI_FontImage.Source.Width;
                double x = border.Margin.Left / (UI_FontImage.ActualWidth) - kerningScale;
                double y = border.Margin.Top / (UI_FontImage.ActualHeight);

                FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].XScale = (float)x;
                FontCodeFile.Characters[UI_CharactersListBox.SelectedIndex].YScale = (float)y;
                UpdateBorderSize(UI_CharactersListBox.SelectedIndex);
                UpdatePreview();
            }
        }

        private void UI_BorderDrag_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (Dragging)
            {
                var border = Borders[UI_CharactersListBox.SelectedIndex];
                var relPos = e.GetPosition(UI_FontImage);
                border.Margin = new Thickness(relPos.X - Anchor.X, relPos.Y - Anchor.Y, 0, 0);
            }
        }

        private void UI_TestTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (FontImageTexFile.SheetData == null)
                return;
            var image = FontCodeFile.RenderFont(FontImageTexFile, " " + UI_TestTextBox.Text);
            UI_FontTest.Source = ImageTools.ConvertToSource(image);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UI_WTextBox.IsEnabled = UI_KTextBox.IsEnabled = !MonospaceOnly;
            UI_WTextBox.UpdateLayout();
            UI_KTextBox.UpdateLayout();
        }
    }
}
