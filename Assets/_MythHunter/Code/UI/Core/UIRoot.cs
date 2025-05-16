// Шлях: Assets/_MythHunter/Code/UI/Core/UIRoot.cs
using UnityEngine;

namespace MythHunter.UI.Core
{
    public class UIRoot : MonoBehaviour
    {
        public static Transform RootTransform
        {
            get; private set;
        }

        private void Awake()
        {
            RootTransform = transform;
        }
    }
}
