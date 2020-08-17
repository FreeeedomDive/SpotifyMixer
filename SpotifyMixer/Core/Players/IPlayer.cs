using System.Threading.Tasks;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Core.Players
{
    public interface IPlayer
    {
        bool Finished { get; set; }
    
        Task<bool> Play(Track current);
        void Pause();
        void Unpause();

        Task<int> CurrentPosition();
    }
}