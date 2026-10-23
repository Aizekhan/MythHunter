// Шлях: Assets/_MythHunter/Code/Systems/Combat/ICombatAbilitySystem.cs
using MythHunter.Core.ECS;

namespace MythHunter.Systems.Combat
{
    /// <summary>
    /// Інтерфейс системи бойових здібностей
    /// </summary>
    public interface ICombatAbilitySystem : ISystem
    {
        /// <summary>
        /// Використання активної здібності
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <param name="targetEntityId">ID цілі (опціонально)</param>
        /// <returns>true, якщо здібність успішно використана</returns>
        bool UseActiveAbility(int entityId, int targetEntityId = -1);

        /// <summary>
        /// Перевірка можливості використання активної здібності
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <returns>true, якщо здібність можна використати</returns>
        bool CanUseActiveAbility(int entityId);

        /// <summary>
        /// Скидання стану використання активної здібності
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        void ResetActiveAbilityUse(int entityId);

        /// <summary>
        /// Отримання ID активної здібності сутності
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <returns>ID активної здібності</returns>
        string GetEntityActiveAbilityId(int entityId);

        /// <summary>
        /// Оновлення пасивних ефектів для сутності
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        void UpdatePassiveEffects(int entityId);
    }
}
