// ICombatSystem.cs
namespace MythHunter.Systems.Combat
{
    /// <summary>
    /// Інтерфейс для бойової системи
    /// </summary>
    public interface ICombatSystem : MythHunter.Core.ECS.ISystem
    {
        void StartCombat(int attackerId, int targetId);
        void EndCombat(int combatId);
        bool IsInCombat(int entityId);
        int GetDamage(int attackerId, int targetId);
    }
}
