using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ViseServer.Hubs;
using ViseServer.Models;
using ViseServer.Services;

namespace ViseServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlayerController : ControllerBase
    {
        public PlayerController(PlayerService playerService, IHubContext<PhoneHub> phoneHubContext)
        {
            PlayerService = playerService;
            PhoneHubContext = phoneHubContext;
        }

        private IHubContext<PhoneHub> PhoneHubContext { get; }
        
        private PlayerService PlayerService { get; }

        [HttpGet("play")]
        public async Task Play()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);
            await playerForDevice.Play();
            await PhoneHubContext.Clients.All.SendAsync("play", await playerForDevice.GetStatus());
        }

        [HttpGet("pause")]
        public async Task Pause()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);
            await playerForDevice.Pause();
            await PhoneHubContext.Clients.All.SendAsync("pause", await playerForDevice.GetStatus());
        }

        [HttpGet("stop")]
        public async Task Stop()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);
            await playerForDevice.Stop();
            await PhoneHubContext.Clients.All.SendAsync("stop", await playerForDevice.GetStatus());
        }

        [HttpGet("next")]
        public async Task Next()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);
            var nextMedia = await playerForDevice.Next();

            if (nextMedia != null)
            {
                await PhoneHubContext.Clients.All.SendAsync("playlist_update", nextMedia);
            }
        }

        [HttpGet("previous")]
        public async Task Previous()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);
            var previousMedia = await playerForDevice.Previous();
            
            if (previousMedia != null)
            {
                await PhoneHubContext.Clients.All.SendAsync("playlist_update", previousMedia);
            }
        }
        
        [HttpGet("toggle-repeat")]
        public async Task<RepeatEnum> ToggleRepeat()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);
            var toggleRepeat = await playerForDevice.ToggleRepeat();
            await PhoneHubContext.Clients.All.SendAsync("toggle-repeat", await playerForDevice.GetStatus());
            return toggleRepeat;
        }
        
        [HttpGet("status")]
        public async Task<IPlayerStatus> GetStatus()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);
            return await playerForDevice.GetStatus();
        }
    }
}