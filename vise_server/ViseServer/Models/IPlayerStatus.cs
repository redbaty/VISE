namespace ViseServer.Models
{
    public interface IPlayerStatus
    {
        bool IsPaused { get; }

        RepeatEnum Repeat { get; }
    }
}