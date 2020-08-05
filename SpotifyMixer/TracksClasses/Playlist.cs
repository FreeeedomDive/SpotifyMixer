using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using SpotifyMixer.TracksClasses;

namespace SpotifyMixer
{
    [Serializable]
    public class Playlist
    {
        public string Name;
        public List<Track> Tracks;

        public Playlist(string name, List<Track> tracks)
        {
            Name = name;
            Tracks = tracks;
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
    }
}