using System;
using System.Threading.Tasks;

namespace MythHunter.Replay
{
    /// <summary>
    /// Інтерфейс системи реплеїв
    /// </summary>
    public interface IReplaySystem
    {
        void StartRecording();
        void StopRecording();
        Task<string> SaveReplayAsync(string name);
        Task<bool> LoadReplayAsync(string replayId);
        Task<string[]> GetAvailableReplaysAsync();
        bool IsRecording { get; }
        bool IsPlaying { get; }
        void PlayReplay();
        void PauseReplay();
        void StopReplay();
        event Action<float> OnReplayProgress;
    }
}