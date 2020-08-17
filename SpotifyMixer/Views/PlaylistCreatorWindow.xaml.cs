using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using NAudio.Flac;
using NAudio.Wave;
using SpotifyAPI.Web;
using SpotifyMixer.Core.TracksClasses;
using File = TagLib.File;

namespace SpotifyMixer.Views
{
    public partial class PlaylistCreatorWindow : Window
    {
        public Playlist Playlist { get; private set; }
        private readonly string playlistName;
        private readonly SpotifyWebAPI spotifyWebApi;
        private readonly List<SpotifyPlaylist> playlists;
        private readonly List<string> folders;

        public PlaylistCreatorWindow(SpotifyWebAPI spotifyWebApi, string playlistName, List<SpotifyPlaylist> playlists,
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
            var tracks = new List<Track>();
            var tracksFromPlaylists = GetTracksFromPlaylists(tracks);
            var tracksFromFolders = GetTracksFromFolders(tracksFromPlaylists);
            Playlist = new Playlist(playlistName, tracksFromFolders);
            Playlist.Save();
            Dispatcher.Invoke(() => DialogResult = true);
        }

        private List<Track> GetTracksFromPlaylists(List<Track> tracks, int startId = 1)
        {
            var index = startId;
            foreach (var playlistId in playlists.Select(pl => pl.Id))
            {
                var hasNext = true;
                var tracksInPlaylist = 0;
                while (hasNext)
                {
                    var tracksLoading = spotifyWebApi.GetPlaylistTracks(playlistId, offset: tracksInPlaylist);
                    tracksInPlaylist += tracksLoading.Items.Count;
                    foreach (var current in tracksLoading.Items.Select(track => track.Track))
                    {
                        if (current.Uri.Contains("local")) continue;
                        tracks.Add(new Track
                        {
                            Id = index,
                            HasMetaData = true,
                            TrackName = current.Name,
                            Artist = string.Join(", ", current.Artists.Select(artist => artist.Name)),
                            Album = current.Album.Name,
                            IsSpotifyTrack = true,
                            TrackPath = current.Uri,
                            TotalTime = current.DurationMs
                        });
                        index++;
                    }

                    hasNext = tracksLoading.HasNextPage();
                }
            }

            return tracks;
        }

        private List<Track> GetTracksFromFolders(List<Track> tracks)
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
                                duration = (int)reader.TotalTime.TotalMilliseconds;
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
                    var track = new Track
                    {
                        Id = index,
                        IsSpotifyTrack = false,
                        HasMetaData = title.Length != 0,
                        TrackName = title,
                        Artist = artists,
                        Album = album,
                        TrackPath = file,
                        TotalTime = duration
                    };
                    tracks.Add(track);
                    index++;
                }
            }

            return tracks;
        }
    }
}