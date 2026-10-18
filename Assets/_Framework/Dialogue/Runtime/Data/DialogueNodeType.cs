namespace RPGFramework.Dialogue
{
    /// <summary>
    /// Type of a dialogue node.
    /// </summary>
    public enum DialogueNodeType
    {
        /// <summary>
        /// A line of dialogue spoken by an NPC or the player.
        /// </summary>
        Line,

        /// <summary>
        /// A node with player choices.
        /// </summary>
        Choice,

        /// <summary>
        /// A branching node for conditional progression.
        /// </summary>
        Branch,

        /// <summary>
        /// An end node that concludes the dialogue.
        /// </summary>
        End
    }
}
