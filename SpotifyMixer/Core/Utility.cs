using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using Microsoft.Win32;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Core
{
    public static class Utility
    {
        private const string TokenFileName = "token";

        public static void SaveToken(UserToken token)
        {
            IFormatter formatter = new BinaryFormatter();
            var stream = new FileStream(TokenFileName, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, token);
            stream.Close();
        }

        public static UserToken LoadToken()
        {
            IFormatter formatter = new BinaryFormatter();
            var stream = new FileStream(TokenFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var token = (UserToken) formatter.Deserialize(stream);
            stream.Close();
            return token;
        }

        public static bool IsUserExists()
        {
            return File.Exists(TokenFileName);
        }

        public static void ShowErrorMessage(string message, string header)
        {
            MessageBox.Show(message, header);
        }

        public static Playlist GetPlaylistFromFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Playlists (*.pls)|*.pls|All files (*.*)|*.*"
            };
            var res = openFileDialog.ShowDialog();
            if (!res.HasValue || !res.Value) return null;
            var fileName = openFileDialog.FileName;
            return Playlist.LoadPlaylistFromFile(fileName);
        }

        public static string GetCorrectTime(int time)
        {
            return
                $"{time / (1000 * 60)}:{GetCorrectSeconds((time / 1000) % 60)}.{GetCorrectMilliseconds(time % 1000)}";
        }

        private static string GetCorrectSeconds(int seconds)
        {
            return seconds < 10 ? $"0{seconds}" : seconds.ToString();
        }

        private static string GetCorrectMilliseconds(int milliseconds)
        {
            if (milliseconds < 10) return $"00{milliseconds}";
            return milliseconds < 100 ? $"0{milliseconds}" : milliseconds.ToString();
        }

        public static string UriToLink(string uri)
        {
            return $"https://open.spotify.com/track/{uri.Substring("spotify:track:".Length)}";
        }
    }
}