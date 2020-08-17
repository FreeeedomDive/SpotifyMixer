using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SpotifyMixer
{
    public class SpotifyData
    {
        #region Fields
        
        private string clientId = "";
        private string clientSecret = "";

        #endregion

        #region Properties
        
        public string ClientId => clientId;
        public string ClientSecret => clientSecret;

        #endregion

        public SpotifyData()
        {
            TryReadClientIdFromJson();
        }

        private void TryReadClientIdFromJson()
        {
            var text = File.ReadAllText("SpotifyApp.json");
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            dict.TryGetValue("client_id", out clientId);
            dict.TryGetValue("client_secret", out clientSecret);
        }
    }
}