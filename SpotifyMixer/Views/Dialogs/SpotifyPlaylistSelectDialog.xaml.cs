using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SpotifyAPI.Web;
using SpotifyMixer.Core;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Views.Dialogs
{
    public partial class SpotifyPlaylistSelectDialog
    {
        private readonly SpotifyClient spotify;
        public SpotifyPlaylist Playlist { get; private set; }

        public SpotifyPlaylistSelectDialog(SpotifyClient spotify)
        {
            InitializeComponent();
            this.spotify = spotify;
            SetUserPlaylists();
        }

        private async void SetUserPlaylists()
        {
            var user = spotify.UserProfile;
            var playlists = (await spotify.Playlists.CurrentUsers()).Items;
            Playlists.ItemsSource = playlists.Select(playlist => new SpotifyPlaylist {Id = playlist.Id, Name = playlist.Name});
        }

        private void ItemClicked(object sender, MouseButtonEventArgs e)
        {
            if (Playlists.SelectedIndex == -1) return;
            Playlist = Playlists.SelectedItem as SpotifyPlaylist;
            DialogResult = true;
        }

        private async void AddPlaylistWithLink(object sender, RoutedEventArgs e)
        {
            const string linkPrefix = "https://open.spotify.com/playlist/";
            const string uriPrefix = "spotify:playlist:";
            var text = LinkTextBox.Text;
            var link = "";
            if (text.Contains(linkPrefix))
            {
                link = text.Substring(linkPrefix.Length);
                link = link.Substring(0, link.IndexOf("?", StringComparison.Ordinal));
            }
            else if (text.Contains(uriPrefix))
            {
                link = text.Substring(uriPrefix.Length);
            }
            var playlist = await spotify.Playlists.Get(link);
            if (playlist.Id == null)
            {
                Utility.ShowErrorMessage("Bad link or URI", "Error");
                LinkTextBox.Text = "";
                return;
            }

            Playlist = new SpotifyPlaylist
            {
                Id = playlist.Id,
                Name = playlist.Name
            };
            DialogResult = true;
        }
    }
}