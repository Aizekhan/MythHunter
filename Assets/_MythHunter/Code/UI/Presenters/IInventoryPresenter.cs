using MythHunter.UI.Core;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Presenters
{
    public interface IInventoryPresenter : IPresenter
    {
        void SelectItem(int index);
        void UseSelectedItem();
        void DropSelectedItem();
    }
}
