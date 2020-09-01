using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpotifyMixer.Core.TracksClasses
{
    [Serializable]
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
            
            // Tracks = new ObservableCollection<Track>(allTracks.Where(track =>
            //     track.TrackName.ToLower().Contains(filter) ||
            //     track.Artist.ToLower().Contains(filter) ||
            //     track.Album.ToLower().Contains(filter)));
        }

        public void Save()
        {
            var formatter = new BinaryFormatter();
            Stream stream = new FileStream($"{Name}.pls", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, this);
            stream.Close();
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
            var formatter = new BinaryFormatter();
            Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var obj = (Playlist) formatter.Deserialize(stream);
            stream.Close();
            return obj;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}