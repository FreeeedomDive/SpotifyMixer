using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SpotifyMixer.ViewModels;

namespace SpotifyMixer.Views
{
    public partial class PlayerWindowView
    {
        public PlayerWindowView()
        {
            InitializeComponent();
        }

        private void PlaylistListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = DataGrid.SelectedItem;
            if (selected != null)
                DataGrid.ScrollIntoView(selected);
        }

        private void TextChanged(object sender, TextCompositionEventArgs e)
        {
            var selected = DataGrid.SelectedItem;
            if (selected != null)
                DataGrid.ScrollIntoView(selected);
        }
    }
}