using System;

namespace SpotifyMixer
{
    [Serializable]
    public class UserToken
    {
        public string TokenType;
        public string AccessToken;
        public string RefreshToken;
    }
}