using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Timers;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Http;
using SpotifyAPI.Web.Auth;
using System;
using System.Threading.Tasks;
using EmbedIO;
using System.Net;

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

        public SpotifyClient SpotifyApi
        {
            get => api;
            set
            {
                api = value;
                OnPropertyChanged();
            }
        }

        public IUserProfileClient SpotifyProfile
        {
            get => SpotifyApi.UserProfile;
            set
            {
                profile = value;
                OnPropertyChanged();
                var currentProfile = SpotifyApi.UserProfile.Current();
                currentProfile.Wait();
                UpdateProfileData?.Invoke(currentProfile.Result.DisplayName);
            }
        }

        #endregion

        #region Fields

        private string clientId;
        private string verifier;

        private SpotifyClient api;
        private IUserProfileClient profile;
        private PKCETokenResponse tokenResponse;

        private bool canConnect;

        private Timer tokenRefresherTimer;

        #endregion

        #region Delegates

        public delegate void UpdateProfileDataDelegate(string profileName);

        public UpdateProfileDataDelegate UpdateProfileData;

        public delegate void UpdateSpotifyApiDelegate(SpotifyClient api);

        public UpdateSpotifyApiDelegate UpdateSpotifyApi;

        public delegate void UpdateSpotifyAuthCodeDelegete(string code);

        private UpdateSpotifyAuthCodeDelegete UpdateSpotifyAuthCode;

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
            this.clientId = clientId;
            IsSpotifyAvailable = this.clientId != null && this.clientId != "";
        }

        private bool CheckAppData()
        {
            if (IsSpotifyAvailable) return true;

            TryReadClientIdFromJson();
            return IsSpotifyAvailable;
        }

        public async void Connect()
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

            var (verifier, challenge) = PKCEUtil.GenerateCodes();
            this.verifier = verifier;

            var loginRequest = new LoginRequest(
            new Uri("http://localhost:5000/callback"),
                    clientId,
                    LoginRequest.ResponseType.Code)
            {
                CodeChallengeMethod = "S256",
                CodeChallenge = challenge,
                Scope = new[] { Scopes.UserModifyPlaybackState, Scopes.UserReadPlaybackState, Scopes.AppRemoteControl },
            };

            var uri = loginRequest.ToUri();

            BrowserUtil.Open(uri);
            UpdateSpotifyAuthCode += OnGetAuthCode;
            var t = new Task(() => StartLocalWebServer(UpdateSpotifyAuthCode));
            t.Start();
        }

        private async void OnGetAuthCode(string code)
        {
            var initialResponse = await new OAuthClient().RequestToken(
                new PKCETokenRequest(clientId, code, new Uri("http://localhost:5000/callback"), verifier));

            tokenResponse = initialResponse;
            SpotifyApi = new SpotifyClient(initialResponse.AccessToken);
            SpotifyProfile = SpotifyApi.UserProfile;
        }

        private void StartLocalWebServer(UpdateSpotifyAuthCodeDelegete updateSpotifyAuthCode)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5000/callback/");
            listener.Start();
            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            updateSpotifyAuthCode(request.RawUrl.Substring(15));
            HttpListenerResponse response = context.Response;
            string responseStr = "<html><head><meta charset='utf8'></head><body>Авторизация успешна</body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseStr);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            listener.Stop();
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

            var authenticator = new PKCEAuthenticator(clientId, tokenResponse);

            var config = SpotifyClientConfig.CreateDefault()
              .WithAuthenticator(authenticator);
            SpotifyApi = new SpotifyClient(config);

            var currentToken = Utility.LoadToken();

            var user = SpotifyApi.UserProfile;

            SpotifyProfile = user;
            UpdateSpotifyApi?.Invoke(SpotifyApi);
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}