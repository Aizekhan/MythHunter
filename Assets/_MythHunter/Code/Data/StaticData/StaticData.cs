using System;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Data.StaticData
{
    /// <summary>
    /// Базовий клас для статичних даних
    /// </summary>
    [Serializable]
    public abstract class StaticData
    {
        [SerializeField] private string id;
        [Inject] private IMythLogger _logger; // Додаємо логер через DI
        public string Id => id;
        
        public virtual void Initialize() { }
        
        public virtual void Validate()
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning($"StaticData ID is empty for {GetType().Name}");
            }
        }
    }
}
