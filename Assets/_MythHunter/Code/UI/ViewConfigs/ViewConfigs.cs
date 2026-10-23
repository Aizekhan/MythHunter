using UnityEngine;
using MythHunter.UI.Core;

namespace MythHunter.UI.ViewConfigs
{
    [CreateAssetMenu(menuName = "UI/View Config", fileName = "ViewConfig")]
    public class ViewConfig : ScriptableObject
    {
        [Tooltip("Ідентифікатор представлення (має відповідати значенню з ViewId)")]
        public ViewId viewId = ViewId.None;

        [Tooltip("Шлях до префабу представлення")]
        public string prefabPath = "";

        [Tooltip("Чи є представлення спливаючим вікном")]
        public bool isPopup = false;

        [Tooltip("Чи кешувати представлення")]
        public bool isCached = true;

        // 🟢 ДОДАТИ ЦЕ ПОЛЕ
        [Tooltip("Категорія для групування завантаження")]
        public UICategory category = UICategory.Common;

        public string GetPoolKey() => prefabPath; // Єдиний ключ для всіх систем

        /// <summary>
        /// Перевіряє чи потрібно створювати пул для цього View
        /// </summary>
        public bool ShouldCreatePool() => isCached && !isPopup;
    }

    public enum UICategory
    {
        Common,
        Lobby,
        Gameplay,
        Loading,
        Debug,
        MainMenu,
        Settings
    }
}
