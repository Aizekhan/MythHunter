// Шлях: Assets/_MythHunter/Code/UI/Core/IViewConfigRegistry.cs
using System.Collections.Generic;

namespace MythHunter.UI.Core
{
    public interface IViewConfigRegistry
    {
        ViewConfig Get(string viewId);
        IReadOnlyList<ViewConfig> GetAll();
    }
}
