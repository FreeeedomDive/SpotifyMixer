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
        private bool finished;

        public LocalPlayer()
        {
            mediaPlayer = new MediaPlayer {Volume = 0.05};
        }

        public Task<bool> Play(Track track)
        {
            this.track = track;
            mediaPlayer.Dispatcher.Invoke(() =>
            {

                mediaPlayer.Open(new Uri(track.TrackPath));
                mediaPlayer.Play();
            });
            return Task.FromResult(true);
        }

        public Task<bool> GetState()
        {
            mediaPlayer.Dispatcher.Invoke(() =>
            {
                finished = mediaPlayer.Position.TotalMilliseconds >= track.TotalTime - 1000;
            });
            return Task.FromResult(finished);
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
            mediaPlayer.Dispatcher.Invoke(() => position = (int) mediaPlayer.Position.TotalMilliseconds);
            return Task.FromResult(position);
        }
    }
}