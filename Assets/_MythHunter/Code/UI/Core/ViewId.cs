namespace MythHunter.UI.Core
{
    /// <summary>
    /// Enum для ідентифікації представлень (замість string-based підходу)
    /// </summary>
    public enum ViewId
    {
        None = 0,

        // Головні екрани
        Lobby,
        GameplayUI,
        MainMenu,
        Loading,

        // Діалоги та спеціальні представлення
        HeroCardSelector,
        ConfirmDialog,

        // Інші представлення (додавати тут)
    }
}
