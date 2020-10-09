using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.Flac;
using NAudio.Wave;
using SpotifyAPI.Web;
using SpotifyMixer.Core.TracksClasses;
using File = TagLib.File;

namespace SpotifyMixer.Views.Dialogs
{
    public partial class LoadingPlaylistDialog
    {
        public Playlist Playlist { get; private set; }
        private readonly string playlistName;
        private readonly SpotifyWebAPI spotifyWebApi;
        private readonly List<SpotifyPlaylist> playlists;
        private readonly List<string> folders;

        private Dictionary<string, Track> tracks;

        public LoadingPlaylistDialog(SpotifyWebAPI spotifyWebApi, string playlistName, List<SpotifyPlaylist> playlists,
            List<string> folders)
        {
            InitializeComponent();
            this.spotifyWebApi = spotifyWebApi;
            this.playlists = playlists;
            this.folders = folders;
            this.playlistName = playlistName;
            StartGenerating();
        }

        private void StartGenerating()
        {
            var thread = new Thread(GeneratePlaylist);
            thread.Start();
        }

        private void GeneratePlaylist()
        {
            tracks = new Dictionary<string, Track>();
            GetTracksFromPlaylists();
            GetTracksFromFolders();
            Playlist = new Playlist(playlistName, new List<Track>(tracks.Values));
            Playlist.Save();
            Dispatcher.Invoke(() => DialogResult = true);
        }

        private void GetTracksFromPlaylists(int startId = 1)
        {
            var index = startId;
            foreach (var playlist in playlists)
            {
                var hasNext = true;
                var tracksInPlaylist = 0;
                while (hasNext)
                {
                    var tracksLoading = spotifyWebApi.GetPlaylistTracks(playlist.Id, offset: tracksInPlaylist);
                    tracksInPlaylist += tracksLoading.Items.Count;
                    foreach (var current in tracksLoading.Items.Select(track => track.Track))
                    {
                        var uri = current.Uri;
                        if (uri.Contains("local")) continue;
                        if (tracks.ContainsKey(uri)) continue;
                        tracks.Add(uri,
                            new Track
                            {
                                Id = index++,
                                HasMetaData = true,
                                TrackName = current.Name,
                                Artist = string.Join(", ", current.Artists.Select(artist => artist.Name)),
                                Album = current.Album.Name,
                                IsSpotifyTrack = true,
                                TrackPath = uri,
                                TotalTime = current.DurationMs,
                                Location = playlist.Name
                            });
                    }

                    hasNext = tracksLoading.HasNextPage();
                }
            }
        }

        private void GetTracksFromFolders()
        {
            var extensions = new List<string> {".mp3", ".flac"};
            var index = Math.Max(tracks.Count, 1);
            foreach (
                var files in
                folders
                    .Select(folder =>
                        Directory
                            .GetFiles(folder, "*.*", SearchOption.AllDirectories)
                            .Where(f => extensions.IndexOf(Path.GetExtension(f)) >= 0)))
            {
                foreach (var file in files)
                {
                    var artists = "";
                    var album = "";
                    var title = "";
                    int duration;
                    try
                    {
                        var tagFile = File.Create(file);
                        duration = (int) tagFile.Properties.Duration.TotalMilliseconds;
                        artists = string.Join(", ", tagFile.Tag.Performers);
                        album = tagFile.Tag.Album ?? "";
                        title = tagFile.Tag.Title ?? "";
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var extension = file.Split('.').Last();
                            if (extension.Equals("mp3"))
                            {
                                var reader = new Mp3FileReader(file);
                                duration = (int) reader.TotalTime.TotalMilliseconds;
                            }
                            else
                            {
                                var reader = new FlacReader(file);
                                duration = (int) reader.TotalTime.TotalMilliseconds;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    if (duration > 10 * 60 * 1000) continue;
                    if (tracks.ContainsKey(file)) continue;
                    var track = new Track
                    {
                        Id = index++,
                        IsSpotifyTrack = false,
                        HasMetaData = title.Length != 0,
                        TrackName = title,
                        Artist = artists,
                        Album = album,
                        TrackPath = file,
                        TotalTime = duration,
                        Location = file
                    };
                    tracks.Add(file, track);
                }
            }
        }
    }
}