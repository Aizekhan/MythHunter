// Шлях: Assets/_MythHunter/Code/UI/Core/IUISystem.cs

using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;

public interface IUISystem
{
    // Використовуємо ViewId як основний параметр
    UniTask<IView> ShowViewAsync(ViewId viewId);
    void ShowView(ViewId viewId);
    void HideView(ViewId viewId);

    // Реєстрація з ViewId
    void RegisterView(ViewId viewId, IView view);
    void UnregisterView(ViewId viewId);

    // Методи для зворотної сумісності
    void RegisterView(IView view);
    void UnregisterView(IView view);

    // Отримання представлення
    IView GetView(ViewId viewId);
    bool IsViewActive(ViewId viewId);
}
