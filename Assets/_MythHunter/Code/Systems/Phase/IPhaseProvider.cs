// Шлях: Assets/_MythHunter/Code/Core/ECS/IPhaseProvider.cs
using System;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Інтерфейс для провайдера фазової інформації
    /// </summary>
    public interface IPhaseProvider
    {
        /// <summary>
        /// Отримує поточну фазу як рядок
        /// </summary>
        string GetCurrentPhaseId();

        /// <summary>
        /// Перевіряє, чи поточна фаза відповідає заданому ідентифікатору
        /// </summary>
        bool IsCurrentPhase(string phaseId);

        /// <summary>
        /// Підписатися на зміну фази
        /// </summary>
        void SubscribeToPhaseChange(Action<string, string> onPhaseChanged);

        /// <summary>
        /// Відписатися від зміни фази
        /// </summary>
        void UnsubscribeFromPhaseChange(Action<string, string> onPhaseChanged);

        /// <summary>
        /// Отримати список усіх доступних фаз
        /// </summary>
        string[] GetAllPhaseIds();
    }
}
