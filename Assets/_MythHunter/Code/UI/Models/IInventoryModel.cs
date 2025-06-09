// IInventoryModel.cs
namespace MythHunter.UI.Models
{
    /// <summary>
    /// Інтерфейс моделі інвентаря
    /// </summary>
    public interface IInventoryModel : MythHunter.UI.Core.IModel
    {
        System.Collections.Generic.List<InventoryItem> Items
        {
            get;
        }
        int SelectedItemIndex
        {
            get; set;
        }
        bool IsInventoryFull
        {
            get;
        }
        void AddItem(InventoryItem item);
        void RemoveItem(int index);
        void UseItem(int index);
    }
}
