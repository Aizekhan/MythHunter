// Assets/_MythHunter/Code/Systems/Loading/ILoadingSystem.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.ECS;
using System.Collections.Generic;

namespace MythHunter.Systems.Loading
{
    /// <summary>
    /// Інтерфейс системи завантаження ресурсів та підготовки сцени
    /// </summary>
    public interface ILoadingSystem : ISystem
    {
        /// <summary>
        /// Підготувати та почати процес завантаження ігрової сцени
        /// </summary>
        /// <param name="selectedHeroArchetypes">Масив архетипів вибраних героїв</param>
        /// <param name="mapId">Ідентифікатор карти (опціонально)</param>
        UniTask<bool> StartLoadingGameAsync(string[] selectedHeroArchetypes, string mapId = "default");

        /// <summary>
        /// Отримати поточний прогрес завантаження (0-1)
        /// </summary>
        float GetLoadingProgress();

        /// <summary>
        /// Отримати поточний статус завантаження
        /// </summary>
        string GetLoadingStatus();

        /// <summary>
        /// Перевірити, чи завантаження завершено
        /// </summary>
        bool IsLoadingComplete();

        /// <summary>
        /// Завершити процес завантаження та перейти до ігрової сцени
        /// </summary>
        UniTask FinishLoadingAsync();

        /// <summary>
        /// Скасувати процес завантаження
        /// </summary>
        UniTask CancelLoadingAsync();
    }
}
