using System;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Core
{
    public class PlaylistController
    {
        private readonly SpotifyAuthenticationData spotify;
        
        public PlaylistController(SpotifyAuthenticationData spotify)
        {
            this.spotify = spotify;
        }

        public Playlist AddPlaylistDialog()
        {
            var window = new CreatePlaylistWindow(spotify);
            var res = window.ShowDialog();
            if (!res.HasValue || !res.Value) return null;
            var name = window.PlaylistName;
            var creator = new PlaylistCreatorWindow(spotify.SpotifyApi, name, window.Playlists, window.LocalFolders);
            var res2 = creator.ShowDialog();
            if (!res2.HasValue || !res2.Value) return null;
            return creator.Playlist;
        }

        public static Playlist OpenPlaylistDialog()
        {
            return Utility.GetPlaylistFromFile();
        }
    }
}