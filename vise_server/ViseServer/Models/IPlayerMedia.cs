using System;

namespace ViseServer.Models
{
    public interface IPlayerMedia
    {
        bool IsCurrent { get; }

        string Title { get; }

        string Artist { get; }

        string FileId { get; }

        string MediaImage { get; }

        TimeSpan CurrentTime { get; }

        TimeSpan Duration { get; }
    }
}