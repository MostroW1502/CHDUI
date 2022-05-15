using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows;
using System.Windows.Controls;

namespace CHDUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly FileHandler fh;

        public MainWindow()
        {
            InitializeComponent();
            fh = new();
            DataContext = fh;
            FileDataGrid.ItemsSource = fh.FileEntries;
        }

        private void FileDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) { fh.FromFileDrop((string[])e.Data.GetData("FileDrop")); }
        }

        private void ButtonOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            using (CommonOpenFileDialog cofd = new() { IsFolderPicker = true, EnsureFileExists = true })
            {
                if (cofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    fh.SelectedOutputFolder = cofd.FileName;
                }
            }
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            fh.ConvertFiles();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            fh.Clear();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.ScrollToEnd();
        }
    }
}