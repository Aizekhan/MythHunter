namespace MythHunter.Core.Game
{
    /// <summary>
    /// Типи станів гри
    /// </summary>
    public enum GameStateType
    {
        None = 0,
        Boot,
        MainMenu,
        Loading,
        Game,
        Pause,
        GameOver
    }
}