namespace RPGFramework.Dialogue
{
    /// <summary>
    /// State of a dialogue conversation.
    /// </summary>
    public enum DialogueState
    {
        /// <summary>
        /// Dialogue has not been started yet.
        /// </summary>
        NotStarted,

        /// <summary>
        /// Dialogue is currently active and in progress.
        /// </summary>
        Active,

        /// <summary>
        /// Dialogue has been successfully completed.
        /// </summary>
        Completed,

        /// <summary>
        /// Dialogue was started but interrupted (e.g., by combat or cutscene).
        /// </summary>
        Interrupted
    }
}
