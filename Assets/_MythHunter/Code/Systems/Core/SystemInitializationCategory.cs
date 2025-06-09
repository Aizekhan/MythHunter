// Шлях: Assets/_MythHunter/Code/Systems/Core/SystemInitializationCategory.cs
namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Категорії ініціалізації систем
    /// </summary>
    public enum SystemInitializationCategory
    {
        OnBoot,        // Ініціалізуються при запуску гри
        Menu,          // Ініціалізуються при переході в меню
        Lobby,         // Ініціалізуються при переході в лобі
        Gameplay,      // Ініціалізуються при початку геймплею
        Loading,       // Ініціалізуються при завантаженні
        OnDemand,      // Загальна категорія (за замовчуванням)
        Manual
    }
}
