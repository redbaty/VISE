using System;
using ViseServer.Models;

namespace ViseServer.Services
{
    internal class MemoryMedia : IPlayerMedia
    {
        public MemoryMedia()
        {
        }

        public MemoryMedia(string title, string artist, string fileId, string mediaImage, TimeSpan currentTime,
            TimeSpan duration, bool isCurrent)
        {
            Title = title;
            Artist = artist;
            FileId = fileId;
            MediaImage = mediaImage;
            CurrentTime = currentTime;
            Duration = duration;
            IsCurrent = isCurrent;
        }

        public override string ToString()
        {
            return $"{nameof(Title)}: {Title}, {nameof(Artist)}: {Artist}, {nameof(IsCurrent)}: {IsCurrent}";
        }

        public string Title { get; set; }
        public string Artist { get; set; }
        public string FileId { get; set; }
        public string MediaImage { get; set; }
        public TimeSpan CurrentTime { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsCurrent { get; set; }
    }
}