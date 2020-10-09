using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using SpotifyAPI.Web;
using SpotifyMixer.Core.Players;
using SpotifyMixer.Core.TracksClasses;
using Timer = System.Timers.Timer;

namespace SpotifyMixer.Core
{
    public class MusicController : INotifyPropertyChanged
    {
        #region Fields

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private Timer switcherTimer, enableSwitchTimer;
        private int currentPosition;

        private readonly SpotifyPlayer spotifyPlayer;
        private readonly LocalPlayer localPlayer;

        private bool started;
        private bool paused;
        private bool switchedToNext;

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

        public MusicController()
        {
            paused = true;
            switchedToNext = false;
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

        private void SetupEnableSwitchTimer()
        {
            if (enableSwitchTimer != null) enableSwitchTimer.Enabled = false;
            enableSwitchTimer = new Timer(5000);
            enableSwitchTimer.Elapsed += EnableSwitcherEvent;
            enableSwitchTimer.AutoReset = false;
            enableSwitchTimer.Enabled = true;
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
            var finished = player.Finished;
            if (paused || !started || !finished || switchedToNext) return;
            Logger.Info("Next track in Switcher, auto switch after end");
            switcherTimer.Enabled = false;
            switcherTimer = null;
            switchedToNext = true;
            NextTrack();
        }

        private void EnableSwitcherEvent(object source, ElapsedEventArgs e)
        {
            switchedToNext = false;
            enableSwitchTimer.Enabled = false;
        }

        public async void PlayTrack(Track track)
        {
            if (track.IsSpotifyTrack)
            {
                localPlayer.Pause();
                var isPlaying = await spotifyPlayer.Play(track);
                if (!isPlaying)
                {
                    return;
                }
            }
            else
            {
                spotifyPlayer.Pause();
                await localPlayer.Play(track);
            }

            started = true;
            paused = false;
            CurrentTrack = track;
            SetupEnableSwitchTimer();
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
                Utility.ShowErrorDialog("No previous track", "Error");
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
                Utility.ShowErrorDialog("No next track", "Error");
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