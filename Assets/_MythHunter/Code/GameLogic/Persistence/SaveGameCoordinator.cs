using RPGFramework.Persistence;
using System;

namespace MythHunter.GameLogic.Persistence
{
    /// <summary>
    /// Coordinates saving and loading across all MythHunter game systems.
    /// This is the main entry point for save/load operations in the game.
    /// </summary>
    public class SaveGameCoordinator
    {
        private readonly ISaveSystem _saveSystem;
        private readonly string _gameVersion;

        public SaveGameCoordinator(ISaveSystem saveSystem, string gameVersion)
        {
            _saveSystem = saveSystem;
            _gameVersion = gameVersion;

            // Subscribe to save/load events if SaveSystem is the concrete implementation
            if (_saveSystem is SaveSystem concreteSystem)
            {
                concreteSystem.OnSaveLoaded += OnSaveLoaded;
                concreteSystem.OnSaveError += OnSaveError;
            }
        }

        /// <summary>
        /// Saves the complete game state to the specified slot.
        /// </summary>
        public SaveResult SaveGame(string slotId, string characterName, string locationName)
        {
            try
            {
                // Gather data from all game systems
                var saveData = GatherGameData();

                // Create metadata
                var metadata = new SaveMetadata(
                    _gameVersion,
                    characterName,
                    saveData.TotalPlaytime,
                    locationName,
                    saveData.PlayerLevel
                );

                // Add custom metadata
                metadata.CustomData["enemies_defeated"] = saveData.EnemiesDefeated.ToString();
                metadata.CustomData["quests_completed"] = saveData.QuestsCompleted.ToString();

                // Save to system
                var result = _saveSystem.Save(slotId, saveData, metadata);

                return result;
            }
            catch (Exception ex)
            {
                return SaveResult.CreateFailure(slotId, SaveError.IOError,
                    $"Failed to save game: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads the game state from the specified slot.
        /// </summary>
        public LoadResult LoadGame(string slotId)
        {
            try
            {
                var result = _saveSystem.Load(slotId);

                if (result.Success && result.GameState is MythHunterSaveData saveData)
                {
                    // Apply data to all game systems
                    ApplyGameData(saveData);
                }

                return result;
            }
            catch (Exception ex)
            {
                return LoadResult.CreateFailure(slotId, SaveError.IOError,
                    $"Failed to load game: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a quick save of the current game state.
        /// </summary>
        public SaveResult QuickSave(string characterName, string locationName)
        {
            return SaveGame("quicksave", characterName, locationName);
        }

        /// <summary>
        /// Loads the most recent quick save.
        /// </summary>
        public LoadResult QuickLoad()
        {
            return LoadGame("quicksave");
        }

        /// <summary>
        /// Deletes a save from the specified slot.
        /// </summary>
        public bool DeleteSave(string slotId)
        {
            return _saveSystem.DeleteSave(slotId);
        }

        /// <summary>
        /// Gets metadata for a save slot without loading the full save.
        /// </summary>
        public ISaveMetadata GetSaveMetadata(string slotId)
        {
            return _saveSystem.GetMetadata(slotId);
        }

        /// <summary>
        /// Gets all available save slots.
        /// </summary>
        public System.Collections.Generic.IReadOnlyList<ISaveSlot> GetAllSaveSlots()
        {
            return _saveSystem.Slots;
        }

        /// <summary>
        /// Enables or disables auto-save functionality.
        /// </summary>
        public void SetAutoSave(bool enabled, float intervalSeconds = 300f)
        {
            _saveSystem.AutoSaveEnabled = enabled;
            _saveSystem.AutoSaveIntervalSeconds = intervalSeconds;
        }

        /// <summary>
        /// Manually triggers an auto-save.
        /// </summary>
        public void TriggerAutoSave(string characterName, string locationName)
        {
            var saveData = GatherGameData();
            var metadata = new SaveMetadata(
                _gameVersion,
                characterName,
                saveData.TotalPlaytime,
                locationName,
                saveData.PlayerLevel
            );

            _saveSystem.TriggerAutoSave(saveData, metadata);
        }

        /// <summary>
        /// Gathers data from all game systems into a single save data structure.
        /// In a real implementation, this would query all game systems.
        /// </summary>
        private MythHunterSaveData GatherGameData()
        {
            var saveData = new MythHunterSaveData();

            // TODO: Implement actual data gathering from game systems
            // This is where you would:
            // - Query player stats from player system
            // - Get inventory data from inventory system
            // - Get quest data from quest system
            // - Get faction data from faction system
            // - etc.

            // For now, return empty save data as a placeholder
            return saveData;
        }

        /// <summary>
        /// Applies loaded save data to all game systems.
        /// In a real implementation, this would update all game systems.
        /// </summary>
        private void ApplyGameData(MythHunterSaveData saveData)
        {
            // TODO: Implement actual data application to game systems
            // This is where you would:
            // - Set player stats in player system
            // - Restore inventory items
            // - Restore quest states
            // - Restore faction reputations
            // - etc.

            // For now, this is a placeholder
        }

        private void OnSaveLoaded(SaveLoadedEvent evt)
        {
            // Handle save loaded event (e.g., show notification, update UI)
        }

        private void OnSaveError(SaveErrorEvent evt)
        {
            // Handle save error event (e.g., show error message to player)
        }
    }
}
