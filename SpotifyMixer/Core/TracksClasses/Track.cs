using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace SpotifyMixer.Core.TracksClasses
{
    [Serializable]
    public class Track : INotifyPropertyChanged
    {
        #region Fields

        public int Id;
        public bool HasMetaData;
        public int TotalTime;
        public bool IsSpotifyTrack;
        public string TrackPath;
        private int pos;

        #endregion

        #region Properties

        public string Artist { get; set; }
        public string TrackName { get; set; }
        public string Album { get; set; }

        public int QueuePosition
        {
            get => pos;
            set
            {
                pos = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(QueuePositionStr));
            }
        }

        public string QueuePositionStr => pos == 0 ? "" : $"[{pos}]";

        public string Duration => Utility.GetCorrectTime(TotalTime);

        public int TrackPosition => Id;

        public string TrackInfo =>
            HasMetaData ? $"{Artist} - {TrackName} ({Album})" : Path.GetFileNameWithoutExtension(TrackPath);

        public string ShortInfo =>
            HasMetaData ? Artist : Path.GetFileNameWithoutExtension(TrackPath);

        public string Location => IsSpotifyTrack ? "Spotify" : "Local";

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Track track)) return false;
            return Id == track.Id;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
                hashCode = (hashCode * 397) ^ HasMetaData.GetHashCode();
                hashCode = (hashCode * 397) ^ (Artist != null ? Artist.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TrackName != null ? TrackName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Album != null ? Album.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsSpotifyTrack.GetHashCode();
                hashCode = (hashCode * 397) ^ (TrackPath != null ? TrackPath.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}