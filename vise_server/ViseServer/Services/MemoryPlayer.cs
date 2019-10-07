using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Bogus;
using ViseServer.Interfaces;
using ViseServer.Models;

namespace ViseServer.Services
{
    public class MemoryPlayer : IPlayer, IPlayerStatus
    {
        private IDisposable _timerSubscription;
        private static readonly IObservable<long> TimerObservable = Observable.Interval(TimeSpan.FromSeconds(1));

        public MemoryPlayer()
        {
            Repeat = RepeatEnum.NoRepeat;
            Playlist = new Faker<MemoryMedia>()
                .RuleFor(i => i.Title, f => f.Person.FullName)
                .RuleFor(i => i.Artist, f => f.Person.FullName)
                .RuleFor(i => i.Duration, f => TimeSpan.FromMinutes(f.Random.Int(1, 6)))
                .RuleFor(i => i.CurrentTime, f => TimeSpan.Zero)
                .RuleFor(i => i.FileId, f => f.System.FilePath())
                .Generate(10);
            Playlist[0].IsCurrent = true;
        }

        private List<MemoryMedia> Playlist { get; }

        public Task Play()
        {
            IsPaused = false;

            _timerSubscription = TimerObservable.Subscribe(async _ =>
            {
                var current = await GetCurrentMedia();

                if (current != null)
                {
                    var newCurrentTime = current.CurrentTime.Add(TimeSpan.FromSeconds(1));

                    if (newCurrentTime > current.Duration)
                    {
                        _timerSubscription?.Dispose();
                        return;
                    }

                    current.CurrentTime = newCurrentTime;
                }
            });

            return Task.CompletedTask;
        }

        public Task Pause()
        {
            IsPaused = true;
            _timerSubscription?.Dispose();
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            IsPaused = true;
            _timerSubscription?.Dispose();
            return Task.CompletedTask;
        }

        public Task<RepeatEnum> ToggleRepeat()
        {
            switch (Repeat)
            {
                case RepeatEnum.NoRepeat:
                    Repeat = RepeatEnum.Repeat;
                    break;
                case RepeatEnum.Repeat:
                    Repeat = RepeatEnum.RepeatOne;
                    break;
                case RepeatEnum.RepeatOne:
                    Repeat = RepeatEnum.NoRepeat;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.FromResult(Repeat);
        }

        public async Task<IPlayerMedia> Next()
        {
            var currentMedia = await GetCurrentMedia();

            if (currentMedia == null)
            {
                await SetCurrent(Playlist[0]);
                return Playlist[0];
            }
            
            if (currentMedia == Playlist.Last()) return null;
            
            var lastIndexOf = Playlist.IndexOf(currentMedia);
            await SetCurrent(Playlist[lastIndexOf + 1]);
            return await GetCurrentMedia();
        }

        public async Task<IPlayerMedia> Previous()
        {
            var currentMedia = await GetCurrentMedia();
            if (currentMedia == null)
            {
                await SetCurrent(Playlist[0]);
                return Playlist[0];
            }

            var lastIndexOf = Playlist.IndexOf(currentMedia);

            if (lastIndexOf == 0)
            {
                await Pause();
                return null;
            }

            await SetCurrent(Playlist[lastIndexOf - 1]);
            return await GetCurrentMedia();
        }

        public async Task Seek(TimeSpan duration)
        {
            var currentMedia = await GetCurrentMedia();
            currentMedia.CurrentTime = currentMedia.CurrentTime.Add(duration);
        }

        private async Task<MemoryMedia> GetCurrentMedia()
        {
            await foreach (var playerPlaylistMedia in GetPlaylist())
                if (playerPlaylistMedia.IsCurrent)
                    return (MemoryMedia) playerPlaylistMedia;

            return null;
        }

        public Task<IPlayerStatus> GetStatus()
        {
            return Task.FromResult((IPlayerStatus) this);
        }

        public async IAsyncEnumerable<IPlayerMedia> GetPlaylist()
        {
            await Task.CompletedTask;

            foreach (var memoryMedia in Playlist) yield return memoryMedia;
        }

        public bool IsPaused { get; private set; }
        public RepeatEnum Repeat { get; private set; }

        private async Task SetCurrent(IPlayerMedia media)
        {
            var currentMedia = await GetCurrentMedia();
            
            if (currentMedia != null)
            {
                currentMedia.IsCurrent = false;
                currentMedia.CurrentTime = TimeSpan.Zero;
            }

            if (media is MemoryMedia memoryMedia)
            {
                memoryMedia.IsCurrent = true;
                memoryMedia.CurrentTime = TimeSpan.Zero;
            }
        }
    }
}