using UnityEngine;

namespace MythHunter.Authoring
{
    /// <summary>
    /// Базовий Authoring компонент для ентіті
    /// </summary>
    public class BaseAuthoring : MonoBehaviour
    {
        public virtual void ApplyData(int entityId)
        {
            // Override to inject components to the entity
        }
    }
}