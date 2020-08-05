using System.Threading.Tasks;
using SpotifyMixer.TracksClasses;

namespace SpotifyMixer.Players
{
    public interface IPlayer
    {
        Task<bool> Play(Track track);
        void Pause();
        void Unpause();

        Task<int> CurrentPosition();
    }
}