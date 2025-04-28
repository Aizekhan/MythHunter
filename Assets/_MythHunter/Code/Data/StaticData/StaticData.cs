using System;
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
        
        public string Id => id;
        
        public virtual void Initialize() { }
        
        public virtual void Validate()
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"StaticData ID is empty for {GetType().Name}");
            }
        }
    }
}