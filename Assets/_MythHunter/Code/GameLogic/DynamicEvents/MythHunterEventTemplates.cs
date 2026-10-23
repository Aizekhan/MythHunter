using System.Collections.Generic;
using RPGFramework.DynamicEvents.Runtime;

namespace MythHunter.GameLogic.DynamicEvents
{
    /// <summary>
    /// MythHunter-specific event templates and factory.
    /// Demonstrates integration of the Dynamic Events System with 5 sample events.
    /// </summary>
    public static class MythHunterEventTemplates
    {
        /// <summary>
        /// Registers all MythHunter event templates with the event manager.
        /// </summary>
        public static void RegisterAll(IEventManager manager)
        {
            manager.RegisterEventFactory("village_under_attack", CreateVillageUnderAttack);
            manager.RegisterEventFactory("dragon_sighting", CreateDragonSighting);
            manager.RegisterEventFactory("caravan_escort", CreateCaravanEscort);
            manager.RegisterEventFactory("demon_invasion_1", CreateDemonInvasionWave1);
            manager.RegisterEventFactory("demon_invasion_2", CreateDemonInvasionWave2);
            manager.RegisterEventFactory("demon_invasion_3", CreateDemonInvasionWave3);
            manager.RegisterEventFactory("ancient_ruins", CreateAncientRuinsDiscovered);
        }

        /// <summary>
        /// Event 1: Village Under Attack - Defense Event
        /// A horde of monsters is attacking a nearby village. Players must defend it.
        /// </summary>
        private static IDynamicEvent CreateVillageUnderAttack(string eventId, string locationId)
        {
            var objectives = new List<IEventObjective>
            {
                new EventObjective(
                    $"{eventId}_obj1",
                    eventId,
                    "Defeat attacking monsters",
                    "Kill",
                    "monster_goblin",
                    50,
                    isOptional: false,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj2",
                    eventId,
                    "Protect village structures",
                    "Defend",
                    "structure_house",
                    5,
                    isOptional: false,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj3",
                    eventId,
                    "Rescue trapped villagers",
                    "Rescue",
                    "npc_villager",
                    10,
                    isOptional: true,
                    tracksContribution: true)
            };

            return new DynamicEvent(
                eventId,
                EventType.Defense,
                EventDifficulty.Normal,
                "Village Under Attack!",
                "A horde of monsters is attacking the village! Defend the structures and rescue villagers!",
                locationId,
                participationRadius: 200f,
                maxDuration: 900f, // 15 minutes
                objectives,
                new[] { "combat", "defense", "heroic" });
        }

        /// <summary>
        /// Event 2: Dragon Sighting - Boss Event
        /// A powerful dragon has been spotted. Requires coordinated effort to defeat.
        /// </summary>
        private static IDynamicEvent CreateDragonSighting(string eventId, string locationId)
        {
            var objectives = new List<IEventObjective>
            {
                new EventObjective(
                    $"{eventId}_obj1",
                    eventId,
                    "Defeat the Ancient Dragon",
                    "Kill",
                    "boss_ancient_dragon",
                    1,
                    isOptional: false,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj2",
                    eventId,
                    "Destroy dragon's nest",
                    "Destroy",
                    "object_dragon_nest",
                    3,
                    isOptional: true,
                    tracksContribution: true)
            };

            return new DynamicEvent(
                eventId,
                EventType.Boss,
                EventDifficulty.Epic,
                "Dragon Sighting!",
                "An Ancient Dragon has emerged from its lair! Coordinate with other heroes to bring it down!",
                locationId,
                participationRadius: 300f,
                maxDuration: 1200f, // 20 minutes
                objectives,
                new[] { "boss", "epic", "dragon", "legendary_reward" });
        }

        /// <summary>
        /// Event 3: Caravan Escort - Escort Event
        /// A merchant caravan needs protection while traveling through dangerous territory.
        /// </summary>
        private static IDynamicEvent CreateCaravanEscort(string eventId, string locationId)
        {
            var objectives = new List<IEventObjective>
            {
                new EventObjective(
                    $"{eventId}_obj1",
                    eventId,
                    "Escort caravan to destination",
                    "Escort",
                    "npc_caravan_master",
                    100, // Progress based on distance traveled
                    isOptional: false,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj2",
                    eventId,
                    "Defeat ambushing bandits",
                    "Kill",
                    "enemy_bandit",
                    25,
                    isOptional: false,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj3",
                    eventId,
                    "Keep all caravan wagons intact",
                    "Defend",
                    "object_wagon",
                    4,
                    isOptional: true,
                    tracksContribution: false)
            };

            return new DynamicEvent(
                eventId,
                EventType.Escort,
                EventDifficulty.Normal,
                "Caravan Escort",
                "A merchant caravan needs protection through bandit territory. Keep them safe!",
                locationId,
                participationRadius: 150f,
                maxDuration: 600f, // 10 minutes
                objectives,
                new[] { "escort", "protection", "trade" });
        }

        /// <summary>
        /// Event 4a: Demon Invasion - Wave 1 (Invasion Chain Event)
        /// First wave of a demon invasion. Success leads to wave 2, failure to a fallback event.
        /// </summary>
        private static IDynamicEvent CreateDemonInvasionWave1(string eventId, string locationId)
        {
            var objectives = new List<IEventObjective>
            {
                new EventObjective(
                    $"{eventId}_obj1",
                    eventId,
                    "Close demon portals",
                    "Interact",
                    "object_demon_portal",
                    3,
                    isOptional: false,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj2",
                    eventId,
                    "Defeat invading demons",
                    "Kill",
                    "enemy_demon",
                    30,
                    isOptional: false,
                    tracksContribution: true)
            };

            var demonEvent = new DynamicEvent(
                eventId,
                EventType.Invasion,
                EventDifficulty.Hard,
                "Demon Invasion - Wave 1",
                "Demons are pouring through portals! Close them before the invasion overwhelms us!",
                locationId,
                participationRadius: 250f,
                maxDuration: 600f, // 10 minutes
                objectives,
                new[] { "invasion", "demon", "chain" });

            demonEvent.NextEventId = "demon_invasion_2"; // Chain to wave 2 on success

            return demonEvent;
        }

        /// <summary>
        /// Event 4b: Demon Invasion - Wave 2
        /// Second wave with increased difficulty. The invasion intensifies.
        /// </summary>
        private static IDynamicEvent CreateDemonInvasionWave2(string eventId, string locationId)
        {
            var objectives = new List<IEventObjective>
            {
                new EventObjective(
                    $"{eventId}_obj1",
                    eventId,
                    "Defeat Demon Lieutenants",
                    "Kill",
                    "boss_demon_lieutenant",
                    3,
                    isOptional: false,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj2",
                    eventId,
                    "Destroy demon summoning circles",
                    "Destroy",
                    "object_summoning_circle",
                    5,
                    isOptional: false,
                    tracksContribution: true)
            };

            var demonEvent = new DynamicEvent(
                eventId,
                EventType.Invasion,
                EventDifficulty.Epic,
                "Demon Invasion - Wave 2",
                "The invasion intensifies! Demon Lieutenants are leading the assault!",
                locationId,
                participationRadius: 300f,
                maxDuration: 900f, // 15 minutes
                objectives,
                new[] { "invasion", "demon", "chain", "boss" });

            demonEvent.NextEventId = "demon_invasion_3"; // Chain to final wave

            return demonEvent;
        }

        /// <summary>
        /// Event 4c: Demon Invasion - Wave 3 (Final)
        /// Final wave with the Demon Lord. Victory ends the invasion chain.
        /// </summary>
        private static IDynamicEvent CreateDemonInvasionWave3(string eventId, string locationId)
        {
            var objectives = new List<IEventObjective>
            {
                new EventObjective(
                    $"{eventId}_obj1",
                    eventId,
                    "Defeat the Demon Lord",
                    "Kill",
                    "boss_demon_lord",
                    1,
                    isOptional: false,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj2",
                    eventId,
                    "Seal the main portal",
                    "Interact",
                    "object_main_portal",
                    1,
                    isOptional: false,
                    tracksContribution: true)
            };

            return new DynamicEvent(
                eventId,
                EventType.Invasion,
                EventDifficulty.Legendary,
                "Demon Invasion - Final Battle",
                "The Demon Lord himself has arrived! This is the final battle to save the realm!",
                locationId,
                participationRadius: 400f,
                maxDuration: 1800f, // 30 minutes
                objectives,
                new[] { "invasion", "demon", "chain", "boss", "legendary", "world_changing" });
        }

        /// <summary>
        /// Event 5: Ancient Ruins Discovered - Exploration Event
        /// Ancient ruins have been discovered. Explore and uncover their secrets.
        /// </summary>
        private static IDynamicEvent CreateAncientRuinsDiscovered(string eventId, string locationId)
        {
            var objectives = new List<IEventObjective>
            {
                new EventObjective(
                    $"{eventId}_obj1",
                    eventId,
                    "Explore ruin chambers",
                    "Explore",
                    "location_chamber",
                    8,
                    isOptional: false,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj2",
                    eventId,
                    "Collect ancient artifacts",
                    "Collect",
                    "item_ancient_artifact",
                    15,
                    isOptional: false,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj3",
                    eventId,
                    "Defeat awakened guardians",
                    "Kill",
                    "enemy_stone_guardian",
                    10,
                    isOptional: true,
                    tracksContribution: true),
                new EventObjective(
                    $"{eventId}_obj4",
                    eventId,
                    "Decipher ancient inscriptions",
                    "Interact",
                    "object_inscription",
                    5,
                    isOptional: true,
                    tracksContribution: true)
            };

            return new DynamicEvent(
                eventId,
                EventType.Exploration,
                EventDifficulty.Normal,
                "Ancient Ruins Discovered",
                "Ancient ruins have emerged from the earth! Explore them to uncover lost knowledge and treasure!",
                locationId,
                participationRadius: 200f,
                maxDuration: 1200f, // 20 minutes
                objectives,
                new[] { "exploration", "discovery", "ancient", "lore" });
        }

        /// <summary>
        /// Creates the demon invasion event chain.
        /// </summary>
        public static IEventChain CreateDemonInvasionChain()
        {
            var chain = new EventChain("demon_invasion_chain", "Demon Invasion", 3);

            chain.AddChainLink(
                "demon_invasion_1",
                "demon_invasion_2",
                "", // No failure chain, just ends
                "First wave of demons emerges");

            chain.AddChainLink(
                "demon_invasion_2",
                "demon_invasion_3",
                "",
                "Demon Lieutenants lead the assault");

            chain.AddChainLink(
                "demon_invasion_3",
                "", // End of chain
                "",
                "The Demon Lord arrives");

            chain.SetInitialEvent("demon_invasion_1");

            return chain;
        }
    }
}
