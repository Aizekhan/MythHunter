// Шлях: Assets/_MythHunter/Code/UI/ViewConfigs/ViewConfig.cs
using UnityEngine;

namespace MythHunter.UI.Core
{
    [CreateAssetMenu(menuName = "UI/View Config", fileName = "LobbyViewConfig")]
    public class ViewConfig : ScriptableObject
    {
        public string ViewId = "Lobby";
        public string PrefabPath = "UI/Lobby/LobbyView";
        public bool IsPopup;
        public bool IsCached = true;
        public string ViewTypeName; // = typeof(LobbyView).AssemblyQualifiedName;
    }
}
