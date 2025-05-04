// Файл: Assets/_MythHunter/Editor/AddDependencyScopes.cs
using UnityEngine;
using UnityEditor;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Editor
{
    public class AddDependencyScopes
    {
        [MenuItem("Tools/Add Dependency Scopes to Scene")]
       
        private static void AddScopesToScene()
        {
            // Додаємо до основних об'єктів
            AddScopeToObject("UI", "UI Root");
            AddScopeToObject("Canvas", "Canvas");
            AddScopeToObject("DebugDashboard", "Debug");
            AddScopeToObject("PerformanceMonitor", "Performance");
            AddScopeToObject("MythHunter_DebugDashboard", "Debug");

           // _logger.Log("Added DependencyScope to scene objects");
        }

        private static void AddScopeToObject(string objectName, string scopeName)
        {
            var obj = GameObject.Find(objectName);
            if (obj != null)
            {
                if (obj.GetComponent<DependencyScope>() == null)
                {
                    obj.AddComponent<DependencyScope>();
                   // _logger.Log($"Added DependencyScope to {objectName}");
                }
            }
        }
    }
}
