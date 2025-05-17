// Assets/_MythHunter/Code/UI/Core/UIRoot.cs
using UnityEngine;
using MythHunter.Utils.Logging;

namespace MythHunter.UI.Core
{
    public class UIRoot : MonoBehaviour
    {
        private static UIRoot _instance;
        private static IMythLogger _logger;

        public static Transform RootTransform
        {
            get
            {
                if (_instance == null)
                {
                    if (_logger == null)
                    {
                        _logger = MythLoggerFactory.GetDefaultLogger();
                    }

                    var root = FindFirstObjectByType<UIRoot>();
                    if (root != null)
                    {
                        _instance = root;
                        _logger.LogInfo($"Знайдено існуючий UIRoot ({root.name})", "UIRoot");
                    }
                    else
                    {
                        // Створюємо UIRoot програмно, якщо не знайдений
                        var newRootObject = new GameObject("UIRoot");
                        _instance = newRootObject.AddComponent<UIRoot>();

                        // Налаштовуємо як глобальний об'єкт
                        DontDestroyOnLoad(newRootObject);
                        _logger.LogInfo("Створено новий UIRoot програмно", "UIRoot");
                    }
                }

                return _instance.transform;
            }
        }

        private void Awake()
        {
            if (_logger == null)
            {
                _logger = MythLoggerFactory.GetDefaultLogger();
            }

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
