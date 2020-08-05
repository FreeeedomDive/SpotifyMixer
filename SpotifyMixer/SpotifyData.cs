using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SpotifyMixer
{
    public class SpotifyData
    {
        private string _clientId = "";
        private string _clientSecret = "";

        public string ClientId => _clientId;
        public string ClientSecret => _clientSecret;

        public bool IsSpotifyAvailable = false;

        public SpotifyData()
        {
            TryReadClientIdFromJson();
            IsSpotifyAvailable = _clientId != null && _clientSecret != null && _clientId != "" && _clientSecret != "";
        }

        private void TryReadClientIdFromJson()
        {
            var text = File.ReadAllText("SpotifyApp.json");
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            dict.TryGetValue("client_id", out _clientId);
            dict.TryGetValue("client_secret", out _clientSecret);
        }
    }
}