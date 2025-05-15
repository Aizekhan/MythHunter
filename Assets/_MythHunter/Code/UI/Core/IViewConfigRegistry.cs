// Шлях: Assets/_MythHunter/Code/UI/Core/IViewConfigRegistry.cs
using System.Collections.Generic;

namespace MythHunter.UI.Core
{
    public interface IViewConfigRegistry
    {
        LobbyViewConfig Get(string viewId);
        IReadOnlyList<LobbyViewConfig> GetAll();
    }
}
