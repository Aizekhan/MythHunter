using UnityEngine;

namespace MythHunter.UI.Core
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

        [Tooltip("Повне ім'я типу представлення (з простором імен)")]
        public string viewTypeName = "";

        // Backward compatibility для старого коду
        public string ViewId => viewId.ToString();
    }
}
