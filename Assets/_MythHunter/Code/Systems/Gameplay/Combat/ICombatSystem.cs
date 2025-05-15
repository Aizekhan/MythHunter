// Шлях: Assets/_MythHunter/Code/Systems/Combat/ICombatSystem.cs
using MythHunter.Core.ECS;
using MythHunter.Components.Combat;
using Cysharp.Threading.Tasks;

namespace MythHunter.Systems.Combat
{
    /// <summary>
    /// Інтерфейс системи бою
    /// </summary>
    public interface ICombatSystem : ISystem
    {
        /// <summary>
        /// Ініціює бій між двома сутностями
        /// </summary>
        /// <param name="attackerEntityId">ID атакуючої сутності</param>
        /// <param name="defenderEntityId">ID сутності, що захищається</param>
        /// <returns>ID створеного бою</returns>
        int StartCombat(int attackerEntityId, int defenderEntityId);

        /// <summary>
        /// Завершує вказаний бій
        /// </summary>
        /// <param name="combatId">ID бою</param>
        /// <param name="reason">Причина завершення</param>
        void EndCombat(int combatId, MythHunter.Events.Domain.CombatEndReason reason);

        /// <summary>
        /// Асинхронний процес проведення бою
        /// </summary>
        /// <param name="combatId">ID бою</param>
        UniTask ProcessCombatAsync(int combatId);

        /// <summary>
        /// Перевіряє, чи знаходиться сутність у бою
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <returns>true, якщо в бою</returns>
        bool IsEntityInCombat(int entityId);

        /// <summary>
        /// Отримує ID цілі, з якою б'ється сутність
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <returns>ID цілі або -1, якщо не в бою</returns>
        int GetEntityCombatTarget(int entityId);

        /// <summary>
        /// Нанесення пошкодження сутності
        /// </summary>
        /// <param name="sourceEntityId">ID джерела пошкодження</param>
        /// <param name="targetEntityId">ID цілі</param>
        /// <param name="amount">Кількість пошкодження</param>
        /// <param name="damageType">Тип пошкодження</param>
        /// <returns>Фактично нанесене пошкодження</returns>
        float ApplyDamage(int sourceEntityId, int targetEntityId, float amount, MythHunter.Events.Domain.DamageType damageType);

        /// <summary>
        /// Використання активної здібності
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <param name="targetEntityId">ID цілі (опціонально)</param>
        /// <returns>true, якщо здібність успішно використана</returns>
        bool UseActiveAbility(int entityId, int targetEntityId = -1);

        /// <summary>
        /// Зміна бойової стійки
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <param name="newStance">Нова стійка</param>
        void ChangeCombatStance(int entityId, CombatStance newStance);

        /// <summary>
        /// Отримує поточну стійку сутності
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <returns>Поточна стійка</returns>
        CombatStance GetEntityCombatStance(int entityId);

        /// <summary>
        /// Оновлення характеристик бою відповідно до стійки
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        void UpdateCombatStatsBasedOnStance(int entityId);

        /// <summary>
        /// Обмін люті на концентрацію
        /// </summary>
        /// <param name="entityId">ID сутності</param>
        /// <param name="rageAmount">Кількість люті для обміну</param>
        /// <returns>Кількість отриманої концентрації</returns>
        float ExchangeRageForConcentration(int entityId, float rageAmount);
    }
}
