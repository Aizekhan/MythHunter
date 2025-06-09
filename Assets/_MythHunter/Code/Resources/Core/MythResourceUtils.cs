// MythResourceUtils.cs (папка Assets/_MythHunter/Code/Resources/Core/)
using UnityEngine;

namespace MythHunter.Resources.Core
{
    /// <summary>
    /// Утиліти для роботи з ресурсами, що запобігають конфліктам імен із UnityEngine.Resources
    /// </summary>
    public static class MythResourceUtils
    {
        public static T Load<T>(string path) where T : Object
        {
            return UnityEngine.Resources.Load<T>(path);
        }

        public static T[] LoadAll<T>(string path) where T : Object
        {
            return UnityEngine.Resources.LoadAll<T>(path);
        }

        public static void UnloadAsset(Object assetToUnload)
        {
            UnityEngine.Resources.UnloadAsset(assetToUnload);
        }

        public static AsyncOperation UnloadUnusedAssets()
        {
            return UnityEngine.Resources.UnloadUnusedAssets();
        }
    }
}
