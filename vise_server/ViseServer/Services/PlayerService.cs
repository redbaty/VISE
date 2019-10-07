using System.Threading.Tasks;
using ViseServer.Interfaces;

namespace ViseServer.Services
{
    public class PlayerService
    {
        private static IPlayer Player { get; } = new MemoryPlayer();

        public Task<IPlayer> GetPlayerForDevice(string deviceId)
        {
            return Task.FromResult(Player);
        }
    }
}