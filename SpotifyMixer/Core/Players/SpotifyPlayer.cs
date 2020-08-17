using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyMixer.Core;
using SpotifyMixer.Core.Players;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Players
{
    public class SpotifyPlayer : IPlayer
    {
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

        public async Task<bool> Play(Track track)
        {
            if (!isLoggedIn) return false;
            var error = await spotify.ResumePlaybackAsync(uris: new List<string> {track.TrackPath}, offset: 0);
            currentTrack = track;
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
            var playback = await spotify.GetPlaybackAsync();
            return playback.ProgressMs == 0 && !playback.IsPlaying;
        }

        public async Task<int> CurrentPosition()
        {
            var playback = await spotify.GetPlaybackAsync();
            return playback.ProgressMs;
        }
    }
}