using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace SpotifyMixer.Core.TracksClasses
{
    public class Playlist : INotifyPropertyChanged
    {
        #region Fields

        public string Name;
        private List<Track> allTracks;
        private ObservableCollection<Track> tracks;

        #endregion

        #region Properties

        public ObservableCollection<Track> Tracks
        {
            get => tracks;
            set
            {
                tracks = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public Playlist(string name, List<Track> allTracks)
        {
            Name = name;
            this.allTracks = allTracks;
            Tracks = new ObservableCollection<Track>(allTracks);
        }

        #region Methods

        public bool RemoveTrack(Track removable)
        {
            var removed = Tracks.Remove(removable);
            allTracks.Remove(removable);
            if (!removed) return false;
            foreach (var track in Tracks.Where(track => track.Id > removable.Id))
            {
                track.Id--;
            }
            Save();
            OnPropertyChanged(nameof(Tracks));
            return true;
        }

        public void Filter(string filter)
        {
            if (filter.Length == 0)
            {
                Tracks = new ObservableCollection<Track>(allTracks);
                return;
            }

            if (filter.Equals("spotify"))
            {
                Tracks = new ObservableCollection<Track>(allTracks.Where(track => track.IsSpotifyTrack));
                return;
            }

            if (filter.Equals("local"))
            {
                Tracks = new ObservableCollection<Track>(allTracks.Where(track => !track.IsSpotifyTrack));
                return;
            }

            Tracks = new ObservableCollection<Track>(allTracks.Where(track =>
            {
                var info = $"{track.TrackName.ToLower()} {track.Artist.ToLower()} {track.Album.ToLower()}";
                return filter.Split().All(filterWord => info.Contains(filterWord));
            }));
        }

        public void Save()
        {
            using var writer = File.CreateText($"{Name}.pls");
            using var jsWriter = new JsonTextWriter(writer) { Formatting = Formatting.Indented };
            new JsonSerializer().Serialize(jsWriter, allTracks);
        }

        public static Playlist LoadPlaylistByName(string name)
        {
            return Deserialize($"{name}.pls");
        }

        public static Playlist LoadPlaylistFromFile(string filename)
        {
            return Deserialize(filename);
        }

        private static Playlist Deserialize(string fileName)
        {
            using var reader = File.OpenText(fileName);
            using var jsonReader = new JsonTextReader(reader);
            var tracks = new JsonSerializer().Deserialize<List<Track>>(jsonReader);
            return new Playlist(fileName.Substring(0, fileName.Length - 4), tracks);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}