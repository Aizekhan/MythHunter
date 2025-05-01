// Шлях: Assets/_MythHunter/Code/Core/MonoBehaviours/InjectableMonoBehaviour.cs
using UnityEngine;
using MythHunter.Core.Game;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.MonoBehaviours
{
    /// <summary>
    /// Базовий MonoBehaviour з підтримкою ін'єкції залежностей
    /// </summary>
    public abstract class InjectableMonoBehaviour : MonoBehaviour
    {
        protected bool IsInjected
        {
            get; private set;
        }

        // Поле для логера, яке буде заповнено через ін'єкцію
        [Inject] protected IMythLogger _logger;

        protected virtual void Awake()
        {
            // Автоматично реєструємо для ін'єкції
            RegisterForInjection();
        }

        protected void RegisterForInjection()
        {
            if (GameBootstrapper.Instance != null)
            {
                GameBootstrapper.Instance.RegisterForInjection(this);
                IsInjected = true;

                // Викликаємо метод після успішної ін'єкції
                OnInjectionsCompleted();
            }
            else
            {
                // Використання статичного методу з нашої фабрики логерів
                MythLoggerFactory.GetDefaultLogger().LogError($"Failed to inject dependencies into {GetType().Name}: GameBootstrapper instance not found", "DI");
            }
        }

        /// <summary>
        /// Викликається після успішної ін'єкції всіх залежностей
        /// </summary>
        protected virtual void OnInjectionsCompleted()
        {
            // Порожня реалізація, яку можна перевизначити в дочірніх класах
        }
    }
}
