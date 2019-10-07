using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ViseServer.Models;
using ViseServer.Services;

namespace ViseServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaylistController : ControllerBase
    {
        public PlaylistController(PlayerService playerService)
        {
            PlayerService = playerService;
        }

        private PlayerService PlayerService { get; }
        
        [HttpGet("current")]
        public async Task<IPlayerMedia> GetPlaylistCurrent()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);

            await foreach (var playerPlaylistMedia in playerForDevice.GetPlaylist())
                if (playerPlaylistMedia.IsCurrent)
                    return playerPlaylistMedia;

            return null;
        }

        [HttpGet("previous")]
        public async Task<IPlayerMedia> GetPlaylistPrevious()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);

            IPlayerMedia previousMedia = null;

            await foreach (var playerPlaylistMedia in playerForDevice.GetPlaylist())
            {
                if (playerPlaylistMedia.IsCurrent) return previousMedia;

                previousMedia = playerPlaylistMedia;
            }

            return null;
        }

        [HttpGet("next")]
        public async Task<IPlayerMedia> GetPlaylistNext()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);

            var returnNext = false;
            await foreach (var playerPlaylistMedia in playerForDevice.GetPlaylist())
            {
                if (returnNext)
                    return playerPlaylistMedia;

                if (playerPlaylistMedia.IsCurrent) returnNext = true;
            }

            return null;
        }

        [HttpGet]
        public async IAsyncEnumerable<IPlayerMedia> GetPlaylist()
        {
            var playerForDevice = await PlayerService.GetPlayerForDevice(null);

            await foreach (var playerPlaylistMedia in playerForDevice.GetPlaylist()) yield return playerPlaylistMedia;
        }
    }
}