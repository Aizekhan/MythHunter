// Шлях: Assets/_MythHunter/Code/UI/Core/IUIService.cs
using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

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
        UniTask<IView> ShowScreenAsync(Type viewType, string prefabPath);

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
