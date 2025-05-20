// Шлях: Assets/_MythHunter/Code/UI/ViewConfigs/LobbyViewConfig.cs
using UnityEngine;

namespace MythHunter.UI.Core
{
    [CreateAssetMenu(menuName = "UI/View Config", fileName = "LobbyViewConfig")]
    public class LobbyViewConfig : ScriptableObject
    {
        public string ViewId = "Lobby";
        public string PrefabPath = "UI/Lobby/LobbyView";
        public bool IsPopup;
        public bool IsCached = true;
        public string ViewTypeName; // = typeof(LobbyView).AssemblyQualifiedName;
    }
}
