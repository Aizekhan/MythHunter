// Шлях: Assets/_MythHunter/Code/Systems/Combat/ICombatDetectionSystem.cs
using MythHunter.Core.ECS;

namespace MythHunter.Systems.Combat
{
    /// <summary>
    /// Інтерфейс системи виявлення бойових ситуацій
    /// </summary>
    public interface ICombatDetectionSystem : ISystem
    {
        /// <summary>
        /// Перевірка можливості початку бою між сутностями
        /// </summary>
        /// <param name="attackerEntityId">ID потенційного атакуючого</param>
        /// <param name="defenderEntityId">ID потенційного захисника</param>
        /// <returns>true, якщо бій можливий</returns>
        bool CanInitiateCombat(int attackerEntityId, int defenderEntityId);

        /// <summary>
        /// Отримує всіх потенційних ворогів у зоні видимості сутності
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <returns>Масив ID потенційних ворогів</returns>
        int[] GetEnemiesInSight(int entityId);

        /// <summary>
        /// Отримує найближчого ворога для сутності
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <returns>ID найближчого ворога або -1, якщо ворогів немає</returns>
        int GetNearestEnemy(int entityId);

        /// <summary>
        /// Перевіряє, чи є сутності ворогами
        /// </summary>
        /// <param name="entityId1">ID першої сутності</param>
        /// <param name="entityId2">ID другої сутності</param>
        /// <returns>true, якщо сутності є ворогами</returns>
        bool AreEntitiesEnemies(int entityId1, int entityId2);
    }
}
