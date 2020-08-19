using System.Linq;
using System.Windows.Input;
using SpotifyAPI.Web;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Views
{
    public partial class PlaylistSelect
    {
        public SpotifyPlaylist Playlist { get; private set; }

        public PlaylistSelect(SpotifyWebAPI spotify)
        {
            InitializeComponent();
            var user = spotify.GetPrivateProfile();
            var playlists = spotify.GetUserPlaylists(user.Id).Items;
            Playlists.ItemsSource = playlists.Select(playlist => new SpotifyPlaylist {Id = playlist.Id, Name = playlist.Name});
        }

        private void ItemClicked(object sender, MouseButtonEventArgs e)
        {
            if (Playlists.SelectedIndex == -1) return;
            Playlist = Playlists.SelectedItem as SpotifyPlaylist;
            DialogResult = true;
        }
    }
}