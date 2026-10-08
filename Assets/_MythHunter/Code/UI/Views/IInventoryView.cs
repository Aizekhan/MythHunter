using MythHunter.UI.Core;
using MythHunter.UI.Models;
using System.Collections.Generic;

namespace MythHunter.UI.Views
{
    public interface IInventoryView : IView
    {
        void UpdateItems(List<InventoryItem> items);
        void UpdateSelection(int selectedIndex);
    }
}
