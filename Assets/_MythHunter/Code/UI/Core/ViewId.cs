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
        MinimalLoading,
        Settings,
        // Діалоги та спеціальні представлення
        HeroCardSelector,
        ConfirmDialog,

        // Інші представлення (додавати тут)
        HeroCard, // Для карток героїв
        SelectedHeroCard,
        Inventory, // Для карток героїв
    }
}
