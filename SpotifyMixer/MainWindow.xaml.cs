using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;
using SpotifyMixer.Players;
using SpotifyMixer.TracksClasses;
using Timer = System.Timers.Timer;

namespace SpotifyMixer
{
    public partial class MainWindow
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        private readonly SpotifyData _spotifyData = new SpotifyData();
        private SpotifyWebAPI _spotify;
        private PrivateProfile _user;
        private Timer _refreshTimer, _switcherTimer;

        private readonly SpotifyPlayer _spotifyPlayer;
        private readonly LocalPlayer _localPlayer;

        private bool _paused;
        private bool _readyToNext;

        private Playlist _playlist;
        private TrackQueue _queue;

        public MainWindow()
        {
            InitializeComponent();
            _spotifyPlayer = new SpotifyPlayer();
            _localPlayer = new LocalPlayer();
            SetupAutoSwitcherTimer();
        }

        private void SetUserName()
        {
            if (_user == null) return;
            Dispatcher.Invoke(() =>
            {
                CurrentSpotifyUser.Text = $"Current Spotify user: {_user.DisplayName}";
                SpotifyConnect.Content = "Connected!";
            });
        }

        private void ItemClicked(object sender, MouseButtonEventArgs e)
        {
            if (!(PlaylistListView.SelectedItem is Track track)) return;
            PlayTrack(track);
        }

        private async void PlayTrack(Track track)
        {
            _queue.SetCurrentTrack(track);
            if (track.IsSpotifyTrack)
            {
                _localPlayer.Pause();
                var isPlaying = await _spotifyPlayer.Play(track);
                if (!isPlaying)
                {
                    Logger.Info("Next track in method PlayTrack");
                    NextTrack();
                    return;
                }
            }
            else
            {
                _spotifyPlayer.Pause();
                await _localPlayer.Play(track);
            }

            SetInfo(track);
        }

        private void SetInfo(Track track)
        {
            Dispatcher.Invoke(() =>
            {
                _queue.SetCurrentTrack(track);
                PlaylistListView.SelectedItem = track;
                PlaylistListView.ScrollIntoView(track);
                if (track.HasMetaData)
                {
                    CurrentTrackName.Text = track.TrackName;
                    CurrentTrackArtist.Text = track.Artist;
                    CurrentTrackAlbum.Text = track.Album;
                }
                else
                {
                    CurrentTrackName.Text = track.TrackInfo;
                    CurrentTrackArtist.Text = "";
                    CurrentTrackAlbum.Text = "";
                }
            });
        }

        private void SpotifyAuthentication(object sender, RoutedEventArgs e)
        {
            if (Utility.IsUserExists())
            {
                RefreshToken();
                SetupRefreshTimer();
                return;
            }

            AuthorizationCodeAuth();
        }

        private void SetupAutoSwitcherTimer()
        {
            _switcherTimer = new Timer(500);
            _switcherTimer.Elapsed += SwitcherTimerEvent;
            _switcherTimer.AutoReset = true;
            _switcherTimer.Enabled = true;
        }

        private async void SwitcherTimerEvent(object source, ElapsedEventArgs e)
        {
            try
            {
                IPlayer player;
                var track = _queue?.CurrentTrack;
                if (track == null) return;
                if (track.IsSpotifyTrack)
                    player = _spotifyPlayer;
                else
                    player = _localPlayer;

                await Dispatcher.Invoke(async () =>
                {
                    var currentPos = await player.CurrentPosition();
                    Progress.Text = $"{Utility.GetCorrectTime(currentPos)} / {Utility.GetCorrectTime(track.TotalTime)}";
                    if (currentPos > 10000) _readyToNext = true;
                    if (_readyToNext && currentPos >= track.TotalTime - 1200)
                    {
                        Logger.Info("Next track in Switcher, auto switch after end");
                        NextTrack();
                        _readyToNext = false;
                        return;
                    }

                    if (_readyToNext && currentPos == 0 && !(await _spotify.GetPlaybackAsync()).IsPlaying)
                    {
                        Logger.Info("Next track in Switcher, switch if position is 0");
                        NextTrack();
                        _readyToNext = false;
                    }
                });
            }
            catch (Exception exception)
            {
                Dispatcher.Invoke(() => Utility.ShowErrorMessage(exception.StackTrace, exception.Message));
            }
        }

        private void SetupRefreshTimer()
        {
            _refreshTimer = new Timer(1000 * 60 * 1);
            _refreshTimer.Elapsed += RefreshTimerEvent;
            _refreshTimer.AutoReset = true;
            _refreshTimer.Enabled = true;
        }

        private void RefreshTimerEvent(object source, ElapsedEventArgs e)
        {
            RefreshToken();
        }

        private async void RefreshToken()
        {
            if (!_spotifyData.IsSpotifyAvailable)
            {
                Utility.ShowErrorMessage("Введены неверные данные о приложении Spotify, использование Spotify невозможно", "Error");
                return;
            };
            var auth = new AuthorizationCodeAuth(
                _spotifyData.ClientId,
                _spotifyData.ClientSecret,
                "http://localhost:1234",
                "http://localhost:1234",
                Scope.UserModifyPlaybackState | Scope.UserReadPlaybackState
            );
            var currentToken = Utility.LoadToken();
            var newToken = await auth.RefreshToken(currentToken.RefreshToken);
            _spotify = new SpotifyWebAPI
            {
                AccessToken = newToken.AccessToken,
                TokenType = newToken.TokenType
            };

            _user = await _spotify.GetPrivateProfileAsync();
            if (_user.HasError())
            {
                Utility.ShowErrorMessage(
                    $"Возникла ошибка при авторизации!\nКод ошибки: {_user.Error.Message}\nИспользование Spotify невозможно",
                    "Error");
                return;
            }

            var userToken = new UserToken
            {
                TokenType = newToken.TokenType,
                AccessToken = newToken.AccessToken,
                RefreshToken = currentToken.RefreshToken
            };
            Utility.SaveToken(userToken);
            _spotifyPlayer.SetSpotifyApi(_spotify);
            SetUserName();
        }

        private void AuthorizationCodeAuth()
        {
            if (!_spotifyData.IsSpotifyAvailable)
            {
                Utility.ShowErrorMessage("Введены неверные данные о приложении Spotify, использование Spotify невозможно", "Error");
                return;
            };
            var auth = new AuthorizationCodeAuth(
                _spotifyData.ClientId,
                _spotifyData.ClientSecret,
                "http://localhost:1234",
                "http://localhost:1234",
                Scope.UserModifyPlaybackState | Scope.UserReadPlaybackState
            );

            auth.AuthReceived += async (sender, payload) =>
            {
                auth.Stop();
                var token = await auth.ExchangeCode(payload.Code);
                _spotify = new SpotifyWebAPI
                {
                    TokenType = token.TokenType,
                    AccessToken = token.AccessToken
                };
                _user = await _spotify.GetPrivateProfileAsync();
                if (_user.HasError())
                {
                    Utility.ShowErrorMessage(
                        $"Возникла ошибка при авторизации!\nКод ошибки: {_user.Error.Message}\nИспользование Spotify невозможно",
                        "Error");
                    return;
                }

                var userToken = new UserToken
                {
                    TokenType = token.TokenType,
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken
                };
                Utility.SaveToken(userToken);
                SetupRefreshTimer();
                _spotifyPlayer.SetSpotifyApi(_spotify);
                SetUserName();
            };
            auth.Start();
            auth.OpenBrowser();
        }

        private void Pause(object sender, RoutedEventArgs e)
        {
            if (_queue?.CurrentTrack == null) return;
            switch (_paused)
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
            _paused = true;
            _localPlayer.Pause();
            _spotifyPlayer.Pause();
        }

        private void Unpause()
        {
            _paused = false;
            if (_queue.CurrentTrack.IsSpotifyTrack)
            {
                _spotifyPlayer.Unpause();
                return;
            }

            _localPlayer.Unpause();
        }

        private void PreviousTrack(object sender, RoutedEventArgs e)
        {
            if (_playlist == null) return;
            if (_queue == null) return;

            var prev = _queue.GetPreviousTrack();
            if (prev == null)
            {
                Utility.ShowErrorMessage("No previous track", "Error");
                return;
            }

            _paused = false;
            PlayTrack(prev);
        }

        private void NextTrack(object sender, RoutedEventArgs e)
        {
            Logger.Info("Next track, caused by button");
            NextTrack();
        }

        private void NextTrack()
        {
            if (_playlist == null) return;
            if (_queue == null) return;

            var next = _queue.GetNextTrack();
            if (next == null)
            {
                Utility.ShowErrorMessage("No next track", "Error");
                return;
            }

            _paused = false;
            PlayTrack(next);
        }

        private void AddPlaylist(object sender, RoutedEventArgs e)
        {
            var window = new CreatePlaylistWindow(_spotify);
            var res = window.ShowDialog();
            if (!res.HasValue || !res.Value) return;
            var name = window.PlaylistName;
            var creator = new PlaylistCreatorWindow(_spotify, name, window.Playlists, window.LocalFolders);
            var res2 = creator.ShowDialog();
            if (!res2.HasValue || !res2.Value) return;
            _playlist = creator.Playlist;
            _queue = new TrackQueue(_playlist);
            PlaylistListView.ItemsSource = _playlist.Tracks;
        }

        private void OpenPlaylist(object sender, RoutedEventArgs e)
        {
            var playlist = Utility.GetPlaylistFromFile();
            if (playlist == null) return;
            _playlist = playlist;
            _queue = new TrackQueue(_playlist);
            PlaylistListView.ItemsSource = _playlist.Tracks;
        }

        private void OnInputSearch(object sender, TextChangedEventArgs e)
        {
            if (_playlist == null) return;
            var input = SearchBar.Text.ToLower().Trim();
            if (input.Length == 0)
            {
                PlaylistListView.ItemsSource = _playlist.Tracks;
                PlaylistListView.SelectedItem = _queue.CurrentTrack;
                PlaylistListView.ScrollIntoView(_queue.CurrentTrack);
                return;
            }

            if (input.Equals("spotify"))
            {
                PlaylistListView.ItemsSource = _playlist.Tracks.Where(track => track.IsSpotifyTrack);
                return;
            }

            if (input.Equals("local"))
            {
                PlaylistListView.ItemsSource = _playlist.Tracks.Where(track => !track.IsSpotifyTrack);
                return;
            }

            PlaylistListView.ItemsSource = _playlist.Tracks.Where(track =>
                track.TrackName.ToLower().Contains(input) ||
                track.Artist.ToLower().Contains(input) ||
                track.Album.ToLower().Contains(input));
        }

        private void ClearSearch(object sender, RoutedEventArgs e)
        {
            SearchBar.Text = "";
        }

        private void AddToQueue(object sender, MouseButtonEventArgs e)
        {
            if (!(PlaylistListView.SelectedItem is Track track)) return;
            _queue.AddUserTrack(track);
        }
    }
}