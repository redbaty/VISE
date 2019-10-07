using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViseServer.Models;

namespace ViseServer.Interfaces
{
    public interface IPlayer
    {
        Task Play();

        Task Pause();

        Task Stop();

        Task<RepeatEnum> ToggleRepeat();
        
        Task<IPlayerMedia> Next();

        Task<IPlayerMedia> Previous();

        Task Seek(TimeSpan duration);

        Task<IPlayerStatus> GetStatus();

        IAsyncEnumerable<IPlayerMedia> GetPlaylist();
    }
}