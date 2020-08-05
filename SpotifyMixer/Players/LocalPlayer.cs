using System;
using System.Threading.Tasks;
using System.Windows.Media;
using SpotifyMixer.TracksClasses;

namespace SpotifyMixer.Players
{
    public class LocalPlayer : IPlayer
    {
        private readonly MediaPlayer _mediaPlayer;

        public LocalPlayer()
        {
            _mediaPlayer = new MediaPlayer {Volume = 0.05};
        }

        public Task<bool> Play(Track track)
        {
            _mediaPlayer.Open(new Uri(track.TrackPath));
            _mediaPlayer.Play();
            return Task.FromResult(true);
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
        }

        public void Unpause()
        {
            _mediaPlayer.Play();
        }

        public Task<int> CurrentPosition()
        {
            return Task.FromResult((int) _mediaPlayer.Position.TotalMilliseconds);
        }
    }
}