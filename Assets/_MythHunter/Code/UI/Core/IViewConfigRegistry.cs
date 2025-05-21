using System.Collections.Generic;
using MythHunter.UI.ViewConfigs;

namespace MythHunter.UI.Core
{
    public interface IViewConfigRegistry
    {
        ViewConfig Get(ViewId viewId);
        ViewConfig GetByType<T>() where T : UnityEngine.Component, IView;
        IReadOnlyList<ViewConfig> GetAll();
    }
}
