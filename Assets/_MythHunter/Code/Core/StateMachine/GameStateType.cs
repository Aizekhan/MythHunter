namespace MythHunter.Core.Game
{
    /// <summary>
    /// Типи станів гри
    /// </summary>
    public enum GameStateType
    {
        None = 0,
        Lobby = 5,
        Boot,
        MainMenu,
        Loading,
        Gameplay,
        Pause,
        GameOver
    }
}
