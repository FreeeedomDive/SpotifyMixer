using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using SpotifyAPI.Web;
using SpotifyMixer.Core.Players;
using SpotifyMixer.Core.TracksClasses;
using SpotifyMixer.Players;
using Timer = System.Timers.Timer;

namespace SpotifyMixer.Core
{
    public class MusicController : INotifyPropertyChanged
    {
        #region Fields

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly SpotifyAuthenticationData spotifyApi;
        
        private Timer switcherTimer;
        private int currentPosition;

        private readonly SpotifyPlayer spotifyPlayer;
        private readonly LocalPlayer localPlayer;

        private bool started;
        private bool paused = true;

        private Playlist playlist;

        private TrackQueue queue;
        private Track currentTrack;

        #endregion

        #region Properties

        public Playlist Playlist
                {
                    get => playlist;
                    set
                    {
                        playlist = value;
                        OnPropertyChanged();
                    }
                }
        
                public Track CurrentTrack
                {
                    get => currentTrack;
                    set
                    {
                        currentTrack = value;
                        OnPropertyChanged();
                    }
                }
                
        #endregion
        
        public MusicController(SpotifyAuthenticationData spotify)
        {
            spotifyApi = spotify;
            spotifyPlayer = new SpotifyPlayer();
            localPlayer = new LocalPlayer();
        }
        
        #region Delegates

        public delegate void UpdateCurrentTrackDelegate(Track track);

        public UpdateCurrentTrackDelegate UpdateCurrentTrack;

        public delegate void UpdateCurrentPositionDelegate(int position);

        public UpdateCurrentPositionDelegate UpdateCurrentPosition;

        #endregion
        
        #region Methods

        public void UpdateSpotifyApi(SpotifyWebAPI api)
        {
            spotifyPlayer.SetSpotifyApi(api);
        }

        public void OpenPlaylist(Playlist playlist)
        {
            Playlist = playlist;
            queue = new TrackQueue(playlist);
        }

        private void SetupAutoSwitcherTimer()
        {
            if (switcherTimer != null) switcherTimer.Enabled = false;
            switcherTimer = new Timer(500);
            switcherTimer.Elapsed += SwitcherTimerEvent;
            switcherTimer.AutoReset = true;
            switcherTimer.Enabled = true;
        }

        private async void SwitcherTimerEvent(object source, ElapsedEventArgs e)
        {
            IPlayer player;
            if (CurrentTrack == null) return;
            if (CurrentTrack.IsSpotifyTrack)
                player = spotifyPlayer;
            else
                player = localPlayer;

            currentPosition = await player.CurrentPosition();
            UpdateCurrentPosition?.Invoke(currentPosition);
            var state = await player.GetState();
            if (!paused && started && state)
            {
                Logger.Info("Next track in Switcher, auto switch after end");
                currentPosition = 0;
                UpdateCurrentPosition?.Invoke(currentPosition);
                switcherTimer.Enabled = false;
                switcherTimer = null;
                NextTrack();
            }
        }

        public async void PlayTrack(Track track)
        {
            if (track.IsSpotifyTrack)
            {
                Application.Current.Dispatcher.Invoke(() => localPlayer.Pause());
                var isPlaying = await spotifyPlayer.Play(track);
                if (!isPlaying)
                {
                    Logger.Info("Next track in method PlayTrack");
                    NextTrack();
                    return;
                }
            }
            else
            {
                spotifyPlayer.Pause();
                await localPlayer.Play(track);
            }

            started = true;
            CurrentTrack = track;
            SetupAutoSwitcherTimer();
            UpdateCurrentTrack?.Invoke(track);
        }

        public void ChangeState()
        {
            if (Playlist == null) return;
            if (queue == null) return;
            if (CurrentTrack == null)
            {
                NextTrack();
                return;
            }

            switch (paused)
            {
                case true:
                    Unpause();
                    break;
                case false:
                    Pause();
                    break;
            }
        }

        private void Pause()
        {
            paused = true;
            localPlayer.Pause();
            spotifyPlayer.Pause();
        }

        private void Unpause()
        {
            paused = false;
            if (CurrentTrack.IsSpotifyTrack)
            {
                spotifyPlayer.Unpause();
                return;
            }

            localPlayer.Unpause();
        }

        public void PreviousTrack()
        {
            if (Playlist == null) return;
            if (queue == null) return;

            var prev = queue.GetPreviousTrack();
            if (prev == null)
            {
                Utility.ShowErrorMessage("No previous track", "Error");
                return;
            }

            paused = false;

            PlayTrack(prev);
        }

        public void NextTrack()
        {
            if (Playlist == null) return;
            if (queue == null) return;

            var next = queue.GetNextTrack();
            if (next == null)
            {
                Utility.ShowErrorMessage("No next track", "Error");
                return;
            }

            paused = false;

            PlayTrack(next);
        }

        public void AddToQueue(Track track)
        {
            queue.AddUserTrack(track);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}