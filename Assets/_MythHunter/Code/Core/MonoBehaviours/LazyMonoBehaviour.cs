// Шлях: Assets/_MythHunter/Code/Core/MonoBehaviours/LazyMonoBehaviour.cs
using System;
using UnityEngine;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.MonoBehaviours
{
    /// <summary>
    /// MonoBehaviour з підтримкою лінивої ін'єкції залежностей
    /// </summary>
    public abstract class LazyMonoBehaviour : MonoBehaviour
    {
        private bool _injectionPerformed = false;
        private bool _awakePerformed = false;
        private readonly IMythLogger _logger;
        /// <summary>
        /// Чи потрібно виконувати автоматичну ін'єкцію в Awake
        /// </summary>
        protected virtual bool AutoInjectInAwake => true;

        /// <summary>
        /// Викликається перед ін'єкцією залежностей
        /// </summary>
        protected virtual void BeforeInjection()
        {
        }

        /// <summary>
        /// Викликається після ін'єкції залежностей
        /// </summary>
        protected virtual void AfterInjection()
        {
        }

        /// <summary>
        /// Викликається після успішної ін'єкції та Awake
        /// </summary>
        protected virtual void OnInitialized()
        {
        }

        protected virtual void Awake()
        {
            _awakePerformed = true;

            // Автоматична ін'єкція в Awake, якщо потрібно
            if (AutoInjectInAwake)
            {
                EnsureDependenciesInjected();
            }
        }

        /// <summary>
        /// Гарантує, що залежності ін'єктовані
        /// </summary>
        public void EnsureDependenciesInjected()
        {
            if (_injectionPerformed)
                return;

            try
            {
                BeforeInjection();

                // Отримуємо контейнер з GameBootstrapper
                var bootstrapper = GameBootstrapper.Instance;
                if (bootstrapper != null)
                {
                    bootstrapper.InjectInto(this);
                    _injectionPerformed = true;
                    AfterInjection();

                    // Виклик OnInitialized, якщо Awake вже було виконано
                    if (_awakePerformed)
                    {
                        OnInitialized();
                    }
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"GameBootstrapper not found when injecting dependencies into {GetType().Name}", this);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error injecting dependencies into {GetType().Name}: {ex.Message}", this);
            }
        }

        /// <summary>
        /// Перевіряє, чи були ін'єктовані залежності
        /// </summary>
        public bool AreDependenciesInjected => _injectionPerformed;
    }
}
