using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Core.Players
{
    public class SpotifyPlayer : IPlayer
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private SpotifyWebAPI spotify;
        private bool isLoggedIn;
        private Track currentTrack;
        private int position;

        private bool paused;

        public SpotifyPlayer()
        {
            isLoggedIn = false;
            paused = true;
        }

        public void SetSpotifyApi(SpotifyWebAPI spotifyWebApi)
        {
            spotify = spotifyWebApi;
            isLoggedIn = true;
        }

        public bool Finished { get; set; }

        public async Task<bool> Play(Track current)
        {
            if (!isLoggedIn) return false;
            var error = await spotify.ResumePlaybackAsync(uris: new List<string> {current.TrackPath}, offset: 0);
            currentTrack = current;
            if (error.HasError())
            {
                Utility.ShowErrorMessage(error.Error.Message, "Error!");
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
            var res = await spotify.PausePlaybackAsync();
            if (res.HasError())
            {
                if (res.Error.Message.Contains("Restriction violated")) return;
                Utility.ShowErrorMessage(res.Error.Message, "Error");
            }
            paused = true;
            var playback = await spotify.GetPlaybackAsync();
            position = playback.ProgressMs;
        }

        public async void Unpause()
        {
            if (!isLoggedIn) return;
            var res = await spotify.ResumePlaybackAsync(
                uris: new List<string> {currentTrack.TrackPath},
                offset: 0,
                positionMs: position);
            if (res.HasError())
            {
                Utility.ShowErrorMessage(res.Error.Message, "Error");
            }

            paused = false;
        }

        public async Task<bool> GetState()
        {
            try
            {
                var playback = await spotify.GetPlaybackAsync();
                if (playback.HasError())
                {
                    Logger.Error(playback.Error.Message);
                    return false;
                }

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
                var playback = await spotify.GetPlaybackAsync();
                if (playback.HasError())
                {
                    Logger.Error(playback.Error.Message);
                }
                else
                {
                    position = playback.ProgressMs;
                    Finished = position == 0 && !playback.IsPlaying;
                }
            }
            catch (FormatException exception)
            {
                Logger.Error(exception.Message);
            }
            return position;
        }
    }
}