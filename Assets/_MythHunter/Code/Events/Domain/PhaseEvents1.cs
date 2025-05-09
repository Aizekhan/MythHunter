// Файл: Assets/_MythHunter/Code/Events/Domain/PhaseEvents.cs (додати новий тип події)

using MythHunter.Events.Domain;
using MythHunter.Events;

using System;

/// <summary>
/// Подія оновлення стану фази
/// </summary>
public struct PhaseUpdateEvent : IEvent
{
    public GamePhase Phase;
    public float ElapsedTime;
    public float RemainingTime;
    public float TotalDuration;
    public DateTime Timestamp;

    public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";

    public EventPriority GetPriority() => EventPriority.Normal;
}
