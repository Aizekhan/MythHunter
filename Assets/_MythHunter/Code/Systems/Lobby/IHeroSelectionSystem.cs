// Assets/_MythHunter/Code/Systems/Lobby/IHeroSelectionSystem.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.ECS;
using System.Collections.Generic;

namespace MythHunter.Systems.Lobby
{
    /// <summary>
    /// Інтерфейс системи вибору героїв
    /// </summary>
    public interface IHeroSelectionSystem : ISystem
    {
        /// <summary>
        /// Завантажує доступних героїв
        /// </summary>
        UniTask LoadAvailableHeroes();

        /// <summary>
        /// Отримує інформацію про героя за ідентифікатором архетипу
        /// </summary>
        HeroInfo GetHeroInfo(string archetypeId);

        /// <summary>
        /// Перевіряє, чи може гравець вибрати героя
        /// </summary>
        bool CanSelectHero(string archetypeId, int playerIndex, int remainingMana);

        /// <summary>
        /// Сортує героїв за категоріями для UI
        /// </summary>
        Dictionary<string, List<string>> GetHeroesByCategory();
    }
}
