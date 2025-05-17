// Шлях: Assets/_MythHunter/Code/UI/Core/IUIService.cs
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Високорівневий сервіс для управління UI екранами
    /// </summary>
    public interface IUIService
    {
        /// <summary>
        /// Показує вказаний екран за його ідентифікатором
        /// </summary>
        UniTask<TView> ShowScreenAsync<TView>(string screenId) where TView : Component, IView;

        /// <summary>
        /// Ховає вказаний екран
        /// </summary>
        void HideScreen<TView>() where TView : Component, IView;

        /// <summary>
        /// Перевіряє чи екран активний
        /// </summary>
        bool IsScreenActive<TView>() where TView : Component, IView;
    }
}
