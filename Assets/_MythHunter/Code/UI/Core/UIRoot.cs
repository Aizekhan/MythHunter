// Assets/_MythHunter/Code/UI/Core/UIRoot.cs
using UnityEngine;
using MythHunter.Utils.Logging;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Core.DI;

namespace MythHunter.UI.Core
{
    public class UIRoot : LazyMonoBehaviour
    {
        private static UIRoot _instance;

        [Inject] private IMythLogger _logger;

        public static Transform RootTransform
        {
            get
            {
                if (_instance == null)
                {
                    var root = FindFirstObjectByType<UIRoot>();
                    if (root != null)
                    {
                        _instance = root;
                    }
                    else
                    {
                        // Створюємо UIRoot програмно, якщо не знайдений
                        var newRootObject = new GameObject("UIRoot");
                        _instance = newRootObject.AddComponent<UIRoot>();

                        // Налаштовуємо як глобальний об'єкт
                        DontDestroyOnLoad(newRootObject);
                    }
                }

                return _instance.transform;
            }
        }

        protected override void OnInitialized()
        {
            if (_instance == null)
            {
                _instance = this;
                _logger.LogInfo($"Встановлено UIRoot.RootTransform на {transform.name}", "UIRoot");

                // Зберегти між сценами
                if (transform.parent == null) // тільки кореневі об'єкти
                    DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                _logger.LogWarning($"Виявлено дублікат UIRoot! Вже існує {_instance.name}, а це {name}", "UIRoot");
                Destroy(gameObject);
            }
        }
    }
}
