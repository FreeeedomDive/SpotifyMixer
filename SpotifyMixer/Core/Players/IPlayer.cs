using System.Threading.Tasks;
using SpotifyMixer.Core.TracksClasses;

namespace SpotifyMixer.Core.Players
{
    public interface IPlayer
    {
        Task<bool> Play(Track track);
        void Pause();
        void Unpause();

        Task<bool> GetState();
        Task<int> CurrentPosition();
    }
}