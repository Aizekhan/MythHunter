using System;
using System.Collections.Generic;
using MythHunter.UI.ViewConfigs;
using UnityEngine;

namespace MythHunter.UI.Core
{
    public interface IViewConfigRegistry
    {
        ViewConfig Get(ViewId viewId);

        ViewConfig GetByType<T>() where T : Component, IView;

        IReadOnlyList<ViewConfig> GetAll();
    }
}
