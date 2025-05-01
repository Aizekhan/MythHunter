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
        private readonly IMythLogger _logger;
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
            }
            else
            {
                _logger.LogError($"Failed to inject dependencies into {GetType().Name}: GameBootstrapper instance not found");
            }
        }

        /// <summary>
        /// Цей метод має бути імплементований з атрибутом [Inject] для ін'єкції залежностей
        /// </summary>
        [Inject]
        protected virtual void Construct()
        {
            // Порожня реалізація, яку можна перевизначити в дочірніх класах
        }
    }
}
