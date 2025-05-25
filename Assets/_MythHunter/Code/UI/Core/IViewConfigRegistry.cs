// Шлях: Assets/_MythHunter/Code/UI/Core/IViewConfigRegistry.cs
using System.Collections.Generic;
using MythHunter.UI.ViewConfigs;
using UnityEngine;

namespace MythHunter.UI.Core
{
    public interface IViewConfigRegistry
    {
        ViewConfig Get(ViewId viewId);
        ViewConfig GetByTypeName(string viewTypeName);
        IReadOnlyList<ViewConfig> GetAll();

        // Залишається для зворотної сумісності
        [System.Obsolete("Використовуйте GetByTypeName замість GetByType для відповідності принципам архітектури")]
        ViewConfig GetByType<T>() where T : Component, IView;

    }
}
