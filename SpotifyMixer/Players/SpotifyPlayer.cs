using System.Collections.Generic;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyMixer.TracksClasses;

namespace SpotifyMixer.Players
{
    public class SpotifyPlayer : IPlayer
    {
        private SpotifyWebAPI _spotify;
        private bool _isLoggedIn;
        private Track _currentTrack;
        private int _position;

        private bool _paused;

        public SpotifyPlayer()
        {
            _isLoggedIn = false;
            _paused = true;
        }

        public void SetSpotifyApi(SpotifyWebAPI spotifyWebApi)
        {
            _spotify = spotifyWebApi;
            _isLoggedIn = true;
        }

        public async Task<bool> Play(Track track)
        {
            if (!_isLoggedIn) return false;
            var error = await _spotify.ResumePlaybackAsync(uris: new List<string> {track.TrackPath}, offset: 0);
            _currentTrack = track;
            if (error.HasError())
            {
                Utility.ShowErrorMessage(error.Error.Message, "Error!");
                return false;
            }

            _paused = false;
            return true;
        }

        public async void Pause()
        {
            if (!_isLoggedIn) return;
            if (_paused) return;
            _paused = true;
            var res = await _spotify.PausePlaybackAsync();
            if (res.HasError())
            {
                if (res.Error.Message.Contains("Restriction violated")) return;
                Utility.ShowErrorMessage(res.Error.Message, "Error");
            }
            _paused = true;
            var playback = await _spotify.GetPlaybackAsync();
            _position = playback.ProgressMs;
        }

        public async void Unpause()
        {
            if (!_isLoggedIn) return;
            var res = await _spotify.ResumePlaybackAsync(
                uris: new List<string> {_currentTrack.TrackPath},
                offset: 0,
                positionMs: _position);
            if (res.HasError())
            {
                Utility.ShowErrorMessage(res.Error.Message, "Error");
            }

            _paused = false;
        }

        public async Task<int> CurrentPosition()
        {
            var playback = await _spotify.GetPlaybackAsync();
            return playback.ProgressMs;
        }
    }
}