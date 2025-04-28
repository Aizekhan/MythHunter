// InventoryModel.cs
namespace MythHunter.UI.Models
{
    /// <summary>
    /// Модель даних інвентаря
    /// </summary>
    public class InventoryModel : IInventoryModel
    {
        private const int MAX_ITEMS = 10;

        private System.Collections.Generic.List<InventoryItem> _items = new System.Collections.Generic.List<InventoryItem>();

        public System.Collections.Generic.List<InventoryItem> Items => _items;
        public int SelectedItemIndex { get; set; } = -1;
        public bool IsInventoryFull => _items.Count >= MAX_ITEMS;

        public void AddItem(InventoryItem item)
        {
            if (!IsInventoryFull)
            {
                _items.Add(item);
            }
        }

        public void RemoveItem(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                _items.RemoveAt(index);
                if (SelectedItemIndex == index)
                {
                    SelectedItemIndex = -1;
                }
                else if (SelectedItemIndex > index)
                {
                    SelectedItemIndex--;
                }
            }
        }

        public void UseItem(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                var item = _items[index];
                // TODO: Застосувати ефект предмета
                if (item.IsConsumable)
                {
                    RemoveItem(index);
                }
            }
        }
    }

    /// <summary>
    /// Клас предмета інвентаря
    /// </summary>
    public class InventoryItem
    {
        public string Id
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public string Description
        {
            get; set;
        }
        public bool IsConsumable
        {
            get; set;
        }
        public int Quantity { get; set; } = 1;
    }
}
