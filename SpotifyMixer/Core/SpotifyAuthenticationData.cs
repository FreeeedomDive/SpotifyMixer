using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Enums;
using SpotifyAPI.Web.Models;

namespace SpotifyMixer.Core
{
    public class SpotifyAuthenticationData : INotifyPropertyChanged
    {
        #region Properties

        public bool IsSpotifyAvailable
        {
            get => canConnect;
            set
            {
                canConnect = value;
                OnPropertyChanged();
            }
        }

        public SpotifyWebAPI SpotifyApi
        {
            get => api;
            set
            {
                api = value;
                OnPropertyChanged();
            }
        }

        public PrivateProfile SpotifyProfile
        {
            get => profile;
            set
            {
                profile = value;
                OnPropertyChanged();
                UpdateProfileData?.Invoke(profile.DisplayName);
            }
        }

        #endregion

        #region Fields

        private string clientId;
        private string clientSecret;

        private SpotifyWebAPI api;
        private PrivateProfile profile;

        private bool canConnect;

        private Timer tokenRefresherTimer;

        #endregion

        #region Delegates

        public delegate void UpdateProfileDataDelegate(string profileName);

        public UpdateProfileDataDelegate UpdateProfileData;

        public delegate void UpdateSpotifyApiDelegate(SpotifyWebAPI api);

        public UpdateSpotifyApiDelegate UpdateSpotifyApi;

        #endregion

        public SpotifyAuthenticationData()
        {
            TryReadClientIdFromJson();
        }

        #region Methods

        private void TryReadClientIdFromJson()
        {
            var text = File.ReadAllText("SpotifyApp.json");
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            dict.TryGetValue("client_id", out var clientId);
            dict.TryGetValue("client_secret", out var clientSecret);
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            IsSpotifyAvailable = this.clientId != null && this.clientSecret != null && this.clientId != "" &&
                                 this.clientSecret != "";
        }

        private bool CheckAppData()
        {
            if (IsSpotifyAvailable) return true;

            TryReadClientIdFromJson();
            return IsSpotifyAvailable;
        }

        public void Connect()
        {
            if (!CheckAppData())
            {
                Utility.ShowErrorMessage(
                    "Введены неверные данные о приложении Spotify, использование Spotify невозможно", "Error");
                return;
            }

            if (Utility.IsUserExists())
            {
                RefreshToken();
                SetupAutoSwitcherTimer();
                return;
            }

            var auth = new AuthorizationCodeAuth(
                clientId,
                clientSecret,
                "http://localhost:1234",
                "http://localhost:1234",
                Scope.UserModifyPlaybackState | Scope.UserReadPlaybackState
            );

            auth.AuthReceived += async (sender, payload) =>
            {
                auth.Stop();
                var token = await auth.ExchangeCode(payload.Code);
                SpotifyApi = new SpotifyWebAPI
                {
                    TokenType = token.TokenType,
                    AccessToken = token.AccessToken
                };
                var user = await SpotifyApi.GetPrivateProfileAsync();
                if (user.HasError())
                {
                    Utility.ShowErrorMessage(
                        $"Возникла ошибка при авторизации!\nКод ошибки: {user.Error.Message}\nИспользование Spotify невозможно",
                        "Error");
                    return;
                }

                SpotifyProfile = user;
                UpdateSpotifyApi?.Invoke(SpotifyApi);

                var userToken = new UserToken
                {
                    TokenType = token.TokenType,
                    AccessToken = token.AccessToken,
                    RefreshToken = token.RefreshToken
                };
                Utility.SaveToken(userToken);
                SetupAutoSwitcherTimer();
            };
            auth.Start();
            auth.OpenBrowser();
        }

        private void SetupAutoSwitcherTimer()
        {
            tokenRefresherTimer = new Timer(1000 * 60 * 55);
            tokenRefresherTimer.Elapsed += RefreshTokenEvent;
            tokenRefresherTimer.AutoReset = true;
            tokenRefresherTimer.Enabled = true;
        }

        private void RefreshTokenEvent(object source, ElapsedEventArgs e)
        {
            RefreshToken();
        }

        private async void RefreshToken()
        {
            if (!CheckAppData())
            {
                Utility.ShowErrorMessage(
                    "Введены неверные данные о приложении Spotify, использование Spotify невозможно", "Error");
                return;
            }

            var auth = new AuthorizationCodeAuth(
                clientId,
                clientSecret,
                "http://localhost:1234",
                "http://localhost:1234",
                Scope.UserModifyPlaybackState | Scope.UserReadPlaybackState
            );
            var currentToken = Utility.LoadToken();
            var newToken = await auth.RefreshToken(currentToken.RefreshToken);
            if (newToken.HasError())
            {
                Utility.ShowErrorMessage(
                    $"Возникла ошибка при повторной авторизации!\nКод ошибки: {newToken.Error}\nИспользование Spotify невозможно",
                    "Error");
                return;
            }
            SpotifyApi = new SpotifyWebAPI
            {
                AccessToken = newToken.AccessToken,
                TokenType = newToken.TokenType
            };

            var user = await SpotifyApi.GetPrivateProfileAsync();
            if (user.HasError())
            {
                Utility.ShowErrorMessage(
                    $"Возникла ошибка при обновлении профиля!\nКод ошибки: {user.Error.Message}\nИспользование Spotify невозможно",
                    "Error");
                return;
            }

            SpotifyProfile = user;
            UpdateSpotifyApi?.Invoke(SpotifyApi);

            var userToken = new UserToken
            {
                TokenType = newToken.TokenType,
                AccessToken = newToken.AccessToken,
                RefreshToken = currentToken.RefreshToken
            };
            Utility.SaveToken(userToken);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}