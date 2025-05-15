// Assets/_MythHunter/Code/Events/Domain/Gameplay/HeroEvents.cs
using System;
using MythHunter.Components.Combat;
using MythHunter.Entities.Archetypes;

namespace MythHunter.Events.Domain.Gameplay
{
    /// <summary>
    /// Подія для запиту на створення героя
    /// </summary>
    public struct CreateHeroRequestEvent : IEvent
    {
        public string ArchetypeId;
        public string HeroName;
        public int TeamId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія успішного створення героя
    /// </summary>
    public struct HeroCreatedEvent : IEvent
    {
        public int EntityId;
        public string ArchetypeId;
        public string HeroName;
        public int TeamId;
        public HeroClass Class;
        public HeroRace Race;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія активації здібності героя
    /// </summary>
    public struct HeroAbilityActivatedEvent : IEvent
    {
        public int EntityId;
        public string AbilityId;
        public int TargetEntityId;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.High;
    }

    /// <summary>
    /// Подія зміни стійки героя
    /// </summary>
    public struct HeroStanceChangedEvent : IEvent
    {
        public int EntityId;
        public CombatStance OldStance;
        public CombatStance NewStance;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }

    /// <summary>
    /// Подія отримання досвіду героєм
    /// </summary>
    public struct HeroExperienceGainedEvent : IEvent
    {
        public int EntityId;
        public float Experience;
        public string Source;
        public DateTime Timestamp;

        public string GetEventId() => $"{GetType().Name}_{Guid.NewGuid()}";
        public EventPriority GetPriority() => EventPriority.Normal;
    }
}
