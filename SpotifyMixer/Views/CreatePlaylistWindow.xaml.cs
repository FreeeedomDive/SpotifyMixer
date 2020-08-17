using System.Collections.Generic;
using System.Windows;
using SpotifyAPI.Web;
using SpotifyMixer.Core;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer
{
    public partial class CreatePlaylistWindow : Window
    {
        private readonly SpotifyAuthenticationData spotify;
        public string PlaylistName { get; private set; }
        public readonly List<SpotifyPlaylist> Playlists;
        public readonly List<string> LocalFolders;
        
        public CreatePlaylistWindow(SpotifyAuthenticationData spotifyWebApi)
        {
            InitializeComponent();
            spotify = spotifyWebApi;
            Playlists = new List<SpotifyPlaylist>();
            LocalFolders = new List<string>();
        }

        private void AddLocalHolder(object sender, RoutedEventArgs e)
        {
            var folderWindow = new LocalFolderSelect();
            var res = folderWindow.ShowDialog();
            if (!res.HasValue || !res.Value) return;
            var folder = folderWindow.Folder;
            LocalFolders.Add(folder);
            FoldersCount.Text = $"Local folders: {LocalFolders.Count}";
        }

        private void AddSpotifyPlaylist(object sender, RoutedEventArgs e)
        {
            if (spotify.SpotifyProfile == null) return;
            var playlistSelectorWindow = new PlaylistSelect(spotify.SpotifyApi);
            var res = playlistSelectorWindow.ShowDialog();
            if (!res.HasValue || !res.Value) return;
            Playlists.Add(playlistSelectorWindow.Playlist);
            SpotifyCount.Text = $"Spotify playlists: {Playlists.Count}";
        }

        private void CreatePlaylist(object sender, RoutedEventArgs e)
        {
            if (Playlists.Count == 0 && LocalFolders.Count == 0) return;
            PlaylistName = PlaylistNameInput.Text;
            if (PlaylistName.Length == 0) return;
            DialogResult = true;
        }
    }
}