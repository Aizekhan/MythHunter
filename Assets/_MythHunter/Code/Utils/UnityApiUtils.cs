// Шлях: Assets/_MythHunter/Code/Utils/UnityApiUtils.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MythHunter.Utils
{
    /// <summary>
    /// Утиліти для роботи з Unity API
    /// </summary>
    public static class UnityApiUtils
    {
        /// <summary>
        /// Знаходить перший об'єкт вказаного типу на сцені з використанням сучасного API
        /// </summary>
        public static T FindFirstObjectOfType<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindFirstObjectByType<T>();
        }

        /// <summary>
        /// Знаходить всі об'єкти вказаного типу на сцені з використанням сучасного API
        /// </summary>
        public static T[] FindObjectsOfType<T>() where T : UnityEngine.Object
        {
            return UnityEngine.Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        }

        /// <summary>
        /// Знаходить компонент вказаного типу на об'єкті або в його дочірніх об'єктах
        /// </summary>
        public static T GetComponentInChildren<T>(GameObject gameObject, bool includeInactive = false) where T : Component
        {
            return gameObject.GetComponentInChildren<T>(includeInactive);
        }

        /// <summary>
        /// Знищує об'єкт безпечно
        /// </summary>
        public static void SafeDestroy(UnityEngine.Object obj)
        {
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(obj);
            else
                UnityEngine.Object.DestroyImmediate(obj);
        }
    }
}
