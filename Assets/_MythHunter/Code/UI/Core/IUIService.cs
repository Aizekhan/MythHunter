using Cysharp.Threading.Tasks;
using UnityEngine;
using System;

namespace MythHunter.UI.Core
{
    public interface IUIService
    {
        // Основні методи з ViewId
        UniTask<IView> ShowScreenAsync(ViewId viewId);
        void HideScreen(ViewId viewId);
        bool IsScreenActive(ViewId viewId);

       
    }
}
