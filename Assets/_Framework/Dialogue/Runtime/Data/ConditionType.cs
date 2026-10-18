namespace RPGFramework.Dialogue
{
    /// <summary>
    /// Type of condition for dialogue branching and choice availability.
    /// </summary>
    public enum ConditionType
    {
        /// <summary>
        /// Check if a quest has been completed.
        /// </summary>
        QuestComplete,

        /// <summary>
        /// Check if a stat meets a certain value.
        /// </summary>
        StatCheck,

        /// <summary>
        /// Check if an item is owned.
        /// </summary>
        ItemOwned,

        /// <summary>
        /// Check a custom variable value.
        /// </summary>
        VariableCheck,

        /// <summary>
        /// Custom condition evaluated by external logic.
        /// </summary>
        Custom
    }
}
