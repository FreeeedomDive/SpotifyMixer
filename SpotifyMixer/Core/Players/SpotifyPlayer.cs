using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using SpotifyAPI.Web;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Core.Players
{
    public class SpotifyPlayer : IPlayer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private SpotifyClient spotify;
        private bool isLoggedIn;
        private Track currentTrack;
        private int position;

        private bool paused;

        public SpotifyPlayer()
        {
            isLoggedIn = false;
            paused = true;
        }

        public void SetSpotifyApi(SpotifyClient spotifyWebApi)
        {
            spotify = spotifyWebApi;
            isLoggedIn = true;
        }

        public bool Finished { get; set; }

        public async Task<bool> Play(Track current)
        {
            if (!isLoggedIn) return false;
            var playbackRequest = new PlayerResumePlaybackRequest()
            {
                Uris = new List<string> { current.TrackPath },
                OffsetParam = new PlayerResumePlaybackRequest.Offset() { Position = 0 }
            };
            var isPlay = await spotify.Player.ResumePlayback(playbackRequest);
            currentTrack = current;

            if (!isPlay)
            {
                Utility.ShowErrorMessage("resume playback error", "Error!");
                return false;
            }

            paused = false;
            return true;
        }

        public async void Pause()
        {
            if (!isLoggedIn) return;
            if (paused) return;
            paused = true;
            var isResume = await spotify.Player.PausePlayback();
            if (!isResume)
            {
                Utility.ShowErrorMessage("pause playback error", "Error");
            }

            paused = true;
            var playback = await spotify.Player.GetCurrentPlayback();
            position = playback.ProgressMs;
        }

        public async void Unpause()
        {
            if (!isLoggedIn) return;
            var resumeRequest = new PlayerResumePlaybackRequest()
            {
                Uris = new List<string> { currentTrack.TrackPath },
                PositionMs = position,
                OffsetParam = new PlayerResumePlaybackRequest.Offset() { Position = 0 }
            };
            var isResume = await spotify.Player.ResumePlayback(resumeRequest);

            if (!isResume)
            {
                Utility.ShowErrorMessage("resume playback error", "Error");
            }

            paused = false;
        }

        public async Task<bool> GetState()
        {
            try
            {
                var playback = await spotify.Player.GetCurrentPlayback();
                //if (playback.HasError())
                //{
                //    Logger.Error(playback.Error.Message);
                //    return false;
                //}

                position = playback.ProgressMs;
                return position == 0 && !playback.IsPlaying;
            }
            catch (FormatException exception)
            {
                Logger.Error(exception.Message);
                return false;
            }
        }

        public async Task<int> CurrentPosition()
        {
            try
            {
                var playback = await spotify.Player.GetCurrentPlayback();
                //if (playback.HasError())
                //{
                //    Logger.Error(playback.Error.Message);
                //}
                //else
                //{
                    position = playback.ProgressMs;
                    Finished = position == 0 && !playback.IsPlaying;
                //}
            }
            catch (FormatException exception)
            {
                Logger.Error(exception.Message);
            }

            return position;
        }
    }
}