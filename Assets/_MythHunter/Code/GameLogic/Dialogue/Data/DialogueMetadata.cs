// Assets/_MythHunter/Code/GameLogic/Dialogue/Data/DialogueMetadata.cs

using System;

namespace MythHunter.GameLogic.Dialogue
{
    /// <summary>
    /// MythHunter-specific dialogue metadata struct.
    /// Contains UI/gameplay metadata that extends Framework DialogueData.
    ///
    /// This struct holds all visual, audio, and gameplay-specific information
    /// that is not part of the core dialogue progression system.
    /// Framework manages: Id, Title, Description, Nodes, Choices, Conditions
    /// This struct manages: Speaker visuals, Audio, Camera, Game time, Categories, Quest triggers
    /// </summary>
    [Serializable]
    public struct DialogueMetadata
    {
        /// <summary>
        /// Display name of the speaker (NPC name shown in dialogue window).
        /// Example: "Elder Graves", "Merchant Lyra", "Companion Aria"
        /// </summary>
        public string SpeakerName;

        /// <summary>
        /// Path to speaker's portrait image in Resources folder.
        /// Example: "Textures/Portraits/elder_graves", "Textures/Portraits/merchant_lyra"
        /// Used for character portrait display in dialogue UI.
        /// </summary>
        public string SpeakerPortraitPath;

        /// <summary>
        /// Voice actor ID for audio playback during dialogue.
        /// Example: "voice_actor_elder_graves", "voice_actor_lyra"
        /// Links dialogue lines to voice audio clips.
        /// Optional - may be empty for non-voice dialogues.
        /// </summary>
        public string SpeakerVoiceActorId;

        /// <summary>
        /// Path to background music/ambient audio to play during dialogue.
        /// Example: "Audio/Music/dialogue_ambient_village", "Audio/Music/dialogue_tense_choice"
        /// Optional - may be empty to keep current music.
        /// </summary>
        public string BackgroundMusicPath;

        /// <summary>
        /// Camera angle/framing for dialogue scene.
        /// Supported values: "Closeup", "Medium", "Wide"
        /// - Closeup: Focus on character face (emotional moments)
        /// - Medium: Character from chest up (normal dialogue)
        /// - Wide: Full character in environment (establishing shots)
        /// </summary>
        public string CameraAngle;

        /// <summary>
        /// Whether to pause game time during this dialogue.
        /// True for important story moments, false for ambient conversations.
        /// </summary>
        public bool PauseGameTime;

        /// <summary>
        /// Whether this dialogue should appear in dialogue log/history.
        /// False for repeated/NPC banter, True for story-important lines.
        /// </summary>
        public bool ShowInDialogueLog;

        /// <summary>
        /// Dialogue category for organizing and filtering conversations.
        /// Standard categories: "Main", "Side", "Ambient", "Quest"
        /// Custom categories allowed for game-specific types.
        /// Used for UI sorting, dialogue log filtering, statistics.
        /// </summary>
        public string Category;

        /// <summary>
        /// Quest ID to trigger or unlock when this dialogue completes.
        /// Optional - may be empty if dialogue doesn't trigger quests.
        /// Example: "quest_tutorial_goblin_kill", "quest_artifact_hunt"
        /// </summary>
        public string TriggerQuestId;

        // ============================================================
        // FACTORY METHODS
        // ============================================================

        /// <summary>
        /// Creates basic dialogue metadata with required fields only.
        /// Other fields default to empty/false.
        /// </summary>
        /// <param name="speakerName">Speaker name</param>
        /// <param name="speakerPortraitPath">Speaker portrait path</param>
        /// <param name="category">Dialogue category (Main, Side, etc.)</param>
        /// <returns>Basic dialogue metadata</returns>
        public static DialogueMetadata Create(
            string speakerName,
            string speakerPortraitPath,
            string category = "Ambient")
        {
            return new DialogueMetadata
            {
                SpeakerName = speakerName,
                SpeakerPortraitPath = speakerPortraitPath,
                SpeakerVoiceActorId = string.Empty,
                BackgroundMusicPath = string.Empty,
                CameraAngle = "Medium",
                PauseGameTime = false,
                ShowInDialogueLog = true,
                Category = category,
                TriggerQuestId = string.Empty
            };
        }

        /// <summary>
        /// Creates fully-specified dialogue metadata with all fields.
        /// </summary>
        /// <param name="speakerName">Speaker name</param>
        /// <param name="speakerPortraitPath">Speaker portrait path</param>
        /// <param name="speakerVoiceActorId">Voice actor ID</param>
        /// <param name="backgroundMusicPath">Background music path</param>
        /// <param name="cameraAngle">Camera angle</param>
        /// <param name="pauseGameTime">Pause game time flag</param>
        /// <param name="showInDialogueLog">Show in log flag</param>
        /// <param name="category">Dialogue category</param>
        /// <param name="triggerQuestId">Quest to trigger</param>
        /// <returns>Complete dialogue metadata</returns>
        public static DialogueMetadata CreateFull(
            string speakerName,
            string speakerPortraitPath,
            string speakerVoiceActorId,
            string backgroundMusicPath,
            string cameraAngle,
            bool pauseGameTime,
            bool showInDialogueLog,
            string category = "Ambient",
            string triggerQuestId = "")
        {
            return new DialogueMetadata
            {
                SpeakerName = speakerName,
                SpeakerPortraitPath = speakerPortraitPath,
                SpeakerVoiceActorId = speakerVoiceActorId,
                BackgroundMusicPath = backgroundMusicPath,
                CameraAngle = cameraAngle,
                PauseGameTime = pauseGameTime,
                ShowInDialogueLog = showInDialogueLog,
                Category = category,
                TriggerQuestId = triggerQuestId
            };
        }

        /// <summary>
        /// Creates dialogue metadata for main story dialogue.
        /// Main dialogues pause time and show in log by default.
        /// </summary>
        /// <param name="speakerName">Speaker name</param>
        /// <param name="speakerPortraitPath">Speaker portrait path</param>
        /// <param name="cameraAngle">Camera angle, default "Closeup" for story emphasis</param>
        /// <returns>Main story dialogue metadata</returns>
        public static DialogueMetadata CreateMainDialogue(
            string speakerName,
            string speakerPortraitPath,
            string cameraAngle = "Closeup")
        {
            return new DialogueMetadata
            {
                SpeakerName = speakerName,
                SpeakerPortraitPath = speakerPortraitPath,
                SpeakerVoiceActorId = string.Empty,
                BackgroundMusicPath = string.Empty,
                CameraAngle = cameraAngle,
                PauseGameTime = true,
                ShowInDialogueLog = true,
                Category = "Main",
                TriggerQuestId = string.Empty
            };
        }

        /// <summary>
        /// Creates dialogue metadata for side/optional dialogue.
        /// Side dialogues don't pause time but are logged.
        /// </summary>
        /// <param name="speakerName">Speaker name</param>
        /// <param name="speakerPortraitPath">Speaker portrait path</param>
        /// <returns>Side dialogue metadata</returns>
        public static DialogueMetadata CreateSideDialogue(
            string speakerName,
            string speakerPortraitPath)
        {
            return new DialogueMetadata
            {
                SpeakerName = speakerName,
                SpeakerPortraitPath = speakerPortraitPath,
                SpeakerVoiceActorId = string.Empty,
                BackgroundMusicPath = string.Empty,
                CameraAngle = "Medium",
                PauseGameTime = false,
                ShowInDialogueLog = true,
                Category = "Side",
                TriggerQuestId = string.Empty
            };
        }

        /// <summary>
        /// Creates dialogue metadata for quest-related dialogue.
        /// Quest dialogues pause time and may trigger quest changes.
        /// </summary>
        /// <param name="speakerName">Speaker name</param>
        /// <param name="speakerPortraitPath">Speaker portrait path</param>
        /// <param name="triggerQuestId">Quest ID to trigger</param>
        /// <param name="cameraAngle">Camera angle, default "Medium"</param>
        /// <returns>Quest dialogue metadata</returns>
        public static DialogueMetadata CreateQuestDialogue(
            string speakerName,
            string speakerPortraitPath,
            string triggerQuestId,
            string cameraAngle = "Medium")
        {
            return new DialogueMetadata
            {
                SpeakerName = speakerName,
                SpeakerPortraitPath = speakerPortraitPath,
                SpeakerVoiceActorId = string.Empty,
                BackgroundMusicPath = string.Empty,
                CameraAngle = cameraAngle,
                PauseGameTime = true,
                ShowInDialogueLog = true,
                Category = "Quest",
                TriggerQuestId = triggerQuestId
            };
        }

        // ============================================================
        // HELPER METHODS
        // ============================================================

        /// <summary>
        /// Checks if dialogue has all required metadata fields.
        /// Required: SpeakerName, SpeakerPortraitPath
        /// </summary>
        /// <returns>True if metadata is complete</returns>
        public readonly bool IsValid()
        {
            return !string.IsNullOrEmpty(SpeakerName) && !string.IsNullOrEmpty(SpeakerPortraitPath);
        }

        /// <summary>
        /// Checks if dialogue has background music configured.
        /// </summary>
        /// <returns>True if BackgroundMusicPath is not empty</returns>
        public readonly bool HasBackgroundMusic()
        {
            return !string.IsNullOrEmpty(BackgroundMusicPath);
        }

        /// <summary>
        /// Checks if dialogue has voice actor configured.
        /// </summary>
        /// <returns>True if SpeakerVoiceActorId is not empty</returns>
        public readonly bool HasVoiceActor()
        {
            return !string.IsNullOrEmpty(SpeakerVoiceActorId);
        }

        /// <summary>
        /// Gets category display name (with proper capitalization).
        /// </summary>
        /// <returns>Formatted category name</returns>
        public readonly string GetCategoryDisplayName()
        {
            return Category switch
            {
                "Main" => "Main Story",
                "Side" => "Side Conversation",
                "Ambient" => "Ambient",
                "Quest" => "Quest Dialogue",
                _ => Category
            };
        }
    }
}
