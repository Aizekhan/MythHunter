using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Events;

namespace MythHunter.Replay
{
    /// <summary>
    /// Інтерфейс для системи реплеїв
    /// </summary>
    public interface IReplaySystem
    {
        void StartRecording();
        void StopRecording();
        bool IsRecording { get; }
        
        UniTask SaveReplayAsync(string fileName);
        UniTask<bool> LoadReplayAsync(string fileName);
        
        void StartPlayback();
        void PausePlayback();
        void ResumePlayback();
        void StopPlayback();
        bool IsPlayingBack { get; }
        
        float PlaybackSpeed { get; set; }
        float CurrentTime { get; }
        float TotalDuration { get; }
        
        void RegisterEventType<T>() where T : struct, IEvent;
        void UnregisterEventType<T>() where T : struct, IEvent;
        
        event Action<IEvent> OnEventPlayback;
        event Action OnPlaybackStarted;
        event Action OnPlaybackPaused;
        event Action OnPlaybackResumed;
        event Action OnPlaybackStopped;
        event Action OnPlaybackCompleted;
    }
    
    /// <summary>
    /// Запис події для реплею
    /// </summary>
    public struct ReplayEventRecord
    {
        public float Timestamp;
        public string EventTypeId;
        public byte[] SerializedEvent;
        
        public ReplayEventRecord(float timestamp, string eventTypeId, byte[] serializedEvent)
        {
            Timestamp = timestamp;
            EventTypeId = eventTypeId;
            SerializedEvent = serializedEvent;
        }
    }
}