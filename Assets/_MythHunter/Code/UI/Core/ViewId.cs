// Assets/_MythHunter/Code/UI/Core/ViewId.cs
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
        NewLobby,           // ✅ ДОДАНО - новий лоббі
        GameplayUI,
        MainMenu,
        MinimalLoading,
        Settings,
        Profile,

        // Діалоги та спеціальні представлення
        HeroCardSelector,
        ConfirmDialog,

        // Карточки та слоти героїв
        HeroCard,
        SelectedHeroCard,
        HeroSlot,           // ✅ ДОДАНО - слот для вибраних героїв

        // Панель деталей героя
        HeroDetailPanel,    // ✅ ДОДАНО - основна панель деталей
        HeroMainTab,        // ✅ ДОДАНО - головна вкладка
        HeroStatsTab,       // ✅ ДОДАНО - вкладка статистики
        HeroAbilityTab,     // ✅ ДОДАНО - вкладка здібностей  
        HeroEquipTab,       // ✅ ДОДАНО - вкладка екіпування
        HeroQuestTab,       // ✅ ДОДАНО - вкладка квестів

        // Система екіпування
        EquipSlot,          // ✅ ДОДАНО - слот екіпування
        InventoryItem,      // ✅ ДОДАНО - предмет інвентаря
        MiniInventoryPanel, // ✅ ДОДАНО - міні-інвентар

        // Інші представлення
        Inventory,
        RuneSelectionPanel, // ✅ ДОДАНО - панель вибору рун
        ProfileMenu,        // ✅ ДОДАНО - спливаюче меню профілю
    }
}
