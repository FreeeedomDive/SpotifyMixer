﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace SpotifyMixer.Core.TracksClasses
{
    public class Track : INotifyPropertyChanged
    {
        #region Fields

        private int id;

        public bool HasMetaData;
        public int TotalTime;
        public bool IsSpotifyTrack;
        public string TrackPath;
        private int pos;

        public string Location { get; set; }

        #endregion

        #region Properties

        public int Id
        {
            get => id;
            set
            {
                id = value;
                NotifyPropertyChanged();
            }
        }

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

        public string ArtistOrFilename =>
            HasMetaData ? Artist : Path.GetFileNameWithoutExtension(TrackPath);

        public string PlatformLocation => IsSpotifyTrack ? "Spotify" : "Local";

        #endregion

        #region Methods

        public string GetTrackInfo()
        {
            return HasMetaData
                ? $"Artist: {Artist}\n" +
                  $"Name: {TrackName}\n" +
                  $"Album: {Album}\n" +
                  $"Duration: {Duration}\n" +
                  "\nLocation: " +
                  (IsSpotifyTrack ? $"Playlist \"{Location}\"" : $"Folder \"{Location}\"")
                : Location;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (!(obj is Track track)) return false;
            return TrackPath == track.TrackPath;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id;
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