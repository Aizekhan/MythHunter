// Шлях: Assets/_MythHunter/Code/UI/Core/UIViewBase.cs
using UnityEngine;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Базова реалізація IView для MonoBehaviour
    /// </summary>
    public abstract class UIViewBase : MonoBehaviour, IView
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
