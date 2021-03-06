﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using SpotifyAPI.Web;
using SpotifyMixer.Core;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.ViewModels
{
    public class PlayerWindowViewModel : INotifyPropertyChanged
    {
        #region Fields

        private Track selectedTrack;
        private int currentTrackPosition;
        private string buttonContent;
        private string currentUserContent;
        private string currentFilter;

        #endregion

        #region Properties

        public SpotifyAuthenticationData SpotifyApi { get; }
        public MusicController MusicController { get; }
        public PlaylistController PlaylistController { get; }

        public int CurrentTrackPosition
        {
            get => currentTrackPosition;
            set
            {
                currentTrackPosition = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentTrackPositionString));
                CurrentProgress = currentTrackPosition * 1000 / MusicController?.CurrentTrack?.TotalTime ?? 0;
            }
        }

        public string CurrentTrackPositionString =>
            MusicController.CurrentTrack != null
                ? $"{Utility.GetCorrectTime(currentTrackPosition)} / {Utility.GetCorrectTime(MusicController.CurrentTrack.TotalTime)}"
                : "Nothing is playing now";

        private int progress;

        public int CurrentProgress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged();
            }
        }

        public Track SelectedTrack
        {
            get => selectedTrack;
            set
            {
                selectedTrack = value;
                OnPropertyChanged();
            }
        }

        public string ButtonContent
        {
            get => buttonContent;
            set
            {
                buttonContent = value;
                OnPropertyChanged();
            }
        }

        public string CurrentUserContent
        {
            get => currentUserContent;
            set
            {
                currentUserContent = value;
                OnPropertyChanged();
            }
        }

        public string CurrentFilter
        {
            get => currentFilter;
            set
            {
                currentFilter = value.ToLower();
                OnPropertyChanged();
                MusicController.Playlist?.Filter(currentFilter.Trim());
            }
        }

        #endregion

        public PlayerWindowViewModel()
        {
            ButtonContent = "Connect to Spotify";
            CurrentUserContent = "";
            SpotifyApi = new SpotifyAuthenticationData
            {
                UpdateProfileData = UpdateData,
                UpdateSpotifyApi = UpdateSpotifyApi
            };
            MusicController = new MusicController
            {
                UpdateCurrentTrack = UpdateCurrentTrack,
                UpdateCurrentPosition = UpdateCurrentPosition
            };
            PlaylistController = new PlaylistController(SpotifyApi);
        }

        #region Methods

        private void UpdateSpotifyApi(SpotifyWebAPI spotifyWebApi)
        {
            MusicController.UpdateSpotifyApi(spotifyWebApi);
        }

        private void UpdateData(string username)
        {
            ButtonContent = "Connected";
            CurrentUserContent = $"Current Spotify user: {username}";
        }

        private void UpdateCurrentTrack(Track track)
        {
            SelectedTrack = track;
        }

        private void UpdateCurrentPosition(int position)
        {
            CurrentTrackPosition = position;
        }

        #endregion

        #region Commands

        private ICommand connectCommand;

        private ICommand previousTrackCommand;
        private ICommand nextTrackCommand;

        private ICommand pauseCommand;

        private ICommand clearSearchCommand;

        private ICommand addPlaylistCommand;
        private ICommand openPlaylistCommand;

        private ICommand playSelectedTrackCommand;

        private ICommand contextMenuCopyLinkCommand;
        private ICommand contextMenuAddToQueueCommand;
        private ICommand contextMenuDeleteCommand;
        private ICommand contextMenuShowTrackInfo;

        public ICommand ConnectCommand => connectCommand ??=
            new Command(() => SpotifyApi.Connect(), () => SpotifyApi.IsSpotifyAvailable);

        public ICommand PreviousTrackCommand => previousTrackCommand ??=
            new Command(() => MusicController.PreviousTrack());

        public ICommand NextTrackCommand => nextTrackCommand ??=
            new Command(() => MusicController.NextTrack());

        public ICommand PauseCommand => pauseCommand ??=
            new Command(() => MusicController.ChangeState());

        public ICommand PlaySelectedTrackCommand => playSelectedTrackCommand ??=
            new Command(() =>
            {
                if (SelectedTrack != null) MusicController.PlayTrack(SelectedTrack);
            });

        public ICommand AddPlaylistCommand => addPlaylistCommand ??=
            new Command(() =>
            {
                var playlist = PlaylistController.AddPlaylistDialog();
                if (playlist == null) return;
                MusicController.OpenPlaylist(playlist);
            });

        public ICommand OpenPlaylistCommand => openPlaylistCommand ??=
            new Command(() =>
            {
                var playlist = PlaylistController.OpenPlaylistDialog();
                if (playlist == null) return;
                MusicController.OpenPlaylist(playlist);
            });

        public ICommand ClearSearchCommand => clearSearchCommand ??=
            new Command(() => CurrentFilter = "");

        public ICommand ContextMenuCopyLinkCommand => contextMenuCopyLinkCommand ??=
            new Command(() => Clipboard.SetText(Utility.UriToLink(SelectedTrack.TrackPath)),
                () => SelectedTrack != null && SelectedTrack.IsSpotifyTrack);

        public ICommand ContextMenuAddToQueueCommand => contextMenuAddToQueueCommand ??=
            new Command(() => MusicController.AddToQueue(SelectedTrack),
                () => SelectedTrack != null);

        public ICommand ContextMenuDeleteCommand => contextMenuDeleteCommand ??=
            new Command(() =>
                {
                    if (!Utility.ShowConfirmDialog("Do you want to delete this track?", "Delete track"))
                        return;
                    var removed = MusicController.Playlist.RemoveTrack(SelectedTrack);
                    if (!removed) Utility.ShowErrorDialog("Track is not deleted!", "Error");
                    SelectedTrack = MusicController.CurrentTrack;
                },
                () => SelectedTrack != null);

        public ICommand ContextMenuShowTrackInfo => contextMenuShowTrackInfo ??=
            new Command(() => Utility.ShowInfoDialog(SelectedTrack.GetTrackInfo(), "Selected track"),
                () => SelectedTrack != null);

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}