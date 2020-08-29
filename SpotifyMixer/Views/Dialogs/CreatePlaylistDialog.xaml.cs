using System.Collections.Generic;
using System.Windows;
using SpotifyMixer.Core;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Views.Dialogs
{
    public partial class CreatePlaylistDialog
    {
        private readonly SpotifyAuthenticationData spotify;
        public string PlaylistName { get; private set; }
        public readonly List<SpotifyPlaylist> Playlists;
        public readonly List<string> LocalFolders;

        public CreatePlaylistDialog(SpotifyAuthenticationData spotifyWebApi)
        {
            InitializeComponent();
            spotify = spotifyWebApi;
            Playlists = new List<SpotifyPlaylist>();
            LocalFolders = new List<string>();
        }

        private void AddLocalFolder(object sender, RoutedEventArgs e)
        {
            var folderWindow = new LocalFolderSelectDialog();
            var res = folderWindow.ShowDialog();
            if (!res.HasValue || !res.Value) return;
            var folder = folderWindow.Folder;
            LocalFolders.Add(folder);
            FoldersCount.Text = $"Local folders: {LocalFolders.Count}";
        }

        private void AddSpotifyPlaylist(object sender, RoutedEventArgs e)
        {
            if (spotify.SpotifyProfile == null) return;
            var playlistSelectorWindow = new SpotifyPlaylistSelectDialog(spotify.SpotifyApi);
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

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            PlaylistNameInput.Focus();
        }
    }
}