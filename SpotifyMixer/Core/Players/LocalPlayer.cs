using System;
using System.Threading.Tasks;
using System.Windows.Media;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Core.Players
{
    public class LocalPlayer : IPlayer
    {
        private readonly MediaPlayer mediaPlayer;
        private int position;
        private Track track;

        public LocalPlayer()
        {
            mediaPlayer = new MediaPlayer {Volume = 0.05};
        }

        public bool Finished { get; set; }

        public Task<bool> Play(Track current)
        {
            track = current;
            mediaPlayer.Dispatcher.Invoke(() =>
            {
                mediaPlayer.Open(new Uri(current.TrackPath));
                mediaPlayer.Play();
            });
            return Task.FromResult(true);
        }

        public void Pause()
        {
            mediaPlayer.Dispatcher.Invoke(() => mediaPlayer.Pause());
        }

        public void Unpause()
        {
            mediaPlayer.Dispatcher.Invoke(() => mediaPlayer.Play());
        }

        public Task<int> CurrentPosition()
        {
            mediaPlayer.Dispatcher.Invoke(() =>
            {
                Finished = mediaPlayer.Position.TotalMilliseconds >= track.TotalTime - 1000;
                position = (int) mediaPlayer.Position.TotalMilliseconds;
            });
            return Task.FromResult(position);
        }
    }
}