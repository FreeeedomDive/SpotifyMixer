using System.Windows;

namespace SpotifyMixer.Views
{
    public partial class LocalFolderSelect
    {
        public string Folder { get; private set; }
        
        public LocalFolderSelect()
        {
            InitializeComponent();
        }

        private void AddFolder(object sender, RoutedEventArgs e)
        {
            Folder = FolderPath.Text;
            if (Folder.Length == 0) return;
            DialogResult = true;
        }

        private void SelectFolder(object sender, RoutedEventArgs e)
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;
            FolderPath.Text = dialog.SelectedPath;
        }
    }
}