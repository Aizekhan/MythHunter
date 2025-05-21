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


        UniTask<TView> ShowScreenAsync<TView>();
        void HideScreen<TView>();
        bool IsScreenActive<TView>();


    }
}
