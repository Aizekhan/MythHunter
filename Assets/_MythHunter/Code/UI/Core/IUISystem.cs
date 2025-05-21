// Шлях: Assets/_MythHunter/Code/UI/Core/IUISystem.cs

using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;

public interface IUISystem
{
    // Змінюємо методи, щоб працювали з ViewId замість Type
    UniTask<IView> ShowViewAsync(ViewId viewId);
    void ShowView(ViewId viewId);
    void HideView(ViewId viewId);

    // Реєстрація представлень тепер з ViewId
    void RegisterView(ViewId viewId, IView view);
    void UnregisterView(ViewId viewId);

    // Отримання представлення за ViewId
    IView GetView(ViewId viewId);
    bool IsViewActive(ViewId viewId);
}
