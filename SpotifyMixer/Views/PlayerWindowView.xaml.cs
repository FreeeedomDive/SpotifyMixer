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
            if (e.AddedItems.Count == 0) return;
            var newSelectedItem = e.AddedItems[0];
            if (newSelectedItem != null)
            {
                (sender as ListBox).ScrollIntoView(newSelectedItem);
            }
        }
    }
}