// Assets/_MythHunter/Code/Services/Heroes/IHeroDataService.cs

using Cysharp.Threading.Tasks;
using MythHunter.Entities.Heroes;

namespace MythHunter.Services.Heroes
{
    /// <summary>
    /// Інтерфейс сервісу для взаємодії з даними героїв
    /// </summary>
    public interface IHeroDataService
    {
        /// <summary>
        /// Отримує дані героя за ідентифікатором
        /// </summary>
        UniTask<HeroDataModel> GetHeroDataAsync(string heroId);

        /// <summary>
        /// Зберігає дані героя
        /// </summary>
        UniTask SaveHeroDataAsync(HeroDataModel heroData);

        /// <summary>
        /// Видаляє героя за ідентифікатором
        /// </summary>
        UniTask DeleteHeroAsync(string heroId);

        /// <summary>
        /// Отримує список доступних героїв користувача
        /// </summary>
        UniTask<string[]> GetUserHeroIdsAsync(string userId);
    }
}
