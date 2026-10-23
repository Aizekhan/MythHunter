using System;
using System.Collections.Generic;
using RPGFramework.Persistence;

namespace MythHunter.GameLogic.Persistence
{
    /// <summary>
    /// Complete save data structure for MythHunter game.
    /// Aggregates all game system data for saving/loading.
    /// </summary>
    [Serializable]
    public class MythHunterSaveData
    {
        // Player Data
        public string PlayerName { get; set; }
        public int PlayerLevel { get; set; }
        public float PlayerHealth { get; set; }
        public float PlayerMana { get; set; }
        public int PlayerExperience { get; set; }

        // Position Data (using simple struct for serialization)
        [Serializable]
        public struct Position
        {
            public float X;
            public float Y;
            public float Z;
        }
        public Position PlayerPosition { get; set; }
        public string CurrentScene { get; set; }

        // Inventory Data
        public List<string> InventoryItems { get; set; }
        public Dictionary<string, int> ItemQuantities { get; set; }
        public List<string> EquippedItems { get; set; }
        public int CurrencyAmount { get; set; }

        // Quest Data
        public List<string> ActiveQuests { get; set; }
        public List<string> CompletedQuests { get; set; }
        public Dictionary<string, int> QuestProgress { get; set; }

        // Dialogue Data
        public List<string> CompletedDialogues { get; set; }
        public Dictionary<string, int> DialogueChoiceHistory { get; set; }

        // Faction Data
        public Dictionary<string, int> FactionReputations { get; set; }
        public Dictionary<string, string> FactionStandings { get; set; }

        // Achievement Data
        public List<string> UnlockedAchievements { get; set; }
        public Dictionary<string, int> AchievementProgress { get; set; }

        // Guild Data
        public string CurrentGuild { get; set; }
        public int GuildRank { get; set; }
        public List<string> GuildMembers { get; set; }

        // Companion Data
        public List<string> ActiveCompanions { get; set; }
        public Dictionary<string, int> CompanionLoyalty { get; set; }

        // Housing Data
        public List<string> OwnedHouses { get; set; }
        public Dictionary<string, List<string>> HouseFurniture { get; set; }

        // Weather State
        public string CurrentWeather { get; set; }
        public float WeatherIntensity { get; set; }

        // Dynamic Events
        public List<string> ActiveEvents { get; set; }
        public List<string> CompletedEvents { get; set; }

        // Game Statistics
        public float TotalPlaytime { get; set; }
        public int EnemiesDefeated { get; set; }
        public int QuestsCompleted { get; set; }
        public int ItemsCrafted { get; set; }
        public int DistanceTraveled { get; set; }

        // Game Settings (optional - could be saved separately)
        public Dictionary<string, string> GameSettings { get; set; }

        public MythHunterSaveData()
        {
            // Initialize collections
            InventoryItems = new List<string>();
            ItemQuantities = new Dictionary<string, int>();
            EquippedItems = new List<string>();
            ActiveQuests = new List<string>();
            CompletedQuests = new List<string>();
            QuestProgress = new Dictionary<string, int>();
            CompletedDialogues = new List<string>();
            DialogueChoiceHistory = new Dictionary<string, int>();
            FactionReputations = new Dictionary<string, int>();
            FactionStandings = new Dictionary<string, string>();
            UnlockedAchievements = new List<string>();
            AchievementProgress = new Dictionary<string, int>();
            GuildMembers = new List<string>();
            ActiveCompanions = new List<string>();
            CompanionLoyalty = new Dictionary<string, int>();
            OwnedHouses = new List<string>();
            HouseFurniture = new Dictionary<string, List<string>>();
            ActiveEvents = new List<string>();
            CompletedEvents = new List<string>();
            GameSettings = new Dictionary<string, string>();

            // Set defaults
            PlayerHealth = 100f;
            PlayerMana = 100f;
            PlayerLevel = 1;
            CurrencyAmount = 0;
            TotalPlaytime = 0f;
        }
    }
}
