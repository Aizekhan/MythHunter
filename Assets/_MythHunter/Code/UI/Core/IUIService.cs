using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace MythHunter.UI.Core
{
    public interface IUIService
    {
        UniTask<TView> ShowScreenAsync<TView>() where TView : Component, IView;
        void HideScreen<TView>() where TView : Component, IView;

        // Додати метод для сумісності зі старим кодом
        void HideScreen(Type viewType);

        bool IsScreenActive<TView>() where TView : Component, IView;
    }
}
