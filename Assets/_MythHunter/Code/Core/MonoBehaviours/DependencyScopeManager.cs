// Файл: Assets/_MythHunter/Code/Core/MonoBehaviours/DependencyScopeManager.cs
using UnityEngine;

namespace MythHunter.Core.MonoBehaviours
{
    /// <summary>
    /// Менеджер для керування DependencyScope на сцені
    /// </summary>
    public class DependencyScopeManager : MonoBehaviour
    {
        [Header("Scene Scopes")]
        [SerializeField] private DependencyScope _mainScope;
        [SerializeField] private DependencyScope _uiScope;
        [SerializeField] private DependencyScope _debugScope;

        private void Awake()
        {
            EnsureScopeExists("MainScope", ref _mainScope);
            EnsureScopeExists("UIScope", ref _uiScope);
            EnsureScopeExists("DebugScope", ref _debugScope);
        }

        private void EnsureScopeExists(string name, ref DependencyScope scope)
        {
            if (scope == null)
            {
                var obj = new GameObject(name);
                obj.transform.SetParent(transform);
                scope = obj.AddComponent<DependencyScope>();
            }
        }
    }
}
