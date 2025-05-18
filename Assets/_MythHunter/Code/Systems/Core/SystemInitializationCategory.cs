// Шлях: Assets/_MythHunter/Code/Systems/Core/SystemInitializationCategory.cs
namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Категорії ініціалізації систем
    /// </summary>
    public enum SystemInitializationCategory
    {
        /// <summary>
        /// Критичні системи, які ініціалізуються при запуску
        /// </summary>
        OnBoot,

        /// <summary>
        /// Системи, які ініціалізуються на вимогу при зміні стану
        /// </summary>
        OnDemand,

        /// <summary>
        /// Системи, які ініціалізуються лише при явному виклику
        /// </summary>
        Manual
    }
}
