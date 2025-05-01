using MythHunter.Core.ECS;
using System;
using System.Collections.Generic;

namespace MythHunter.Entities
{
    /// <summary>
    /// Базова реалізація архетипу сутностей
    /// </summary>
    public abstract class EntityArchetypeBase : IEntityArchetype
    {
        private readonly List<Type> _requiredComponents = new List<Type>();

        public abstract string ArchetypeId
        {
            get;
        }

        protected EntityArchetypeBase()
        {
            DefineRequiredComponents();
        }

        /// <summary>
        /// Метод для визначення необхідних компонентів архетипу
        /// </summary>
        protected abstract void DefineRequiredComponents();

        /// <summary>
        /// Додає тип компонента як необхідний для цього архетипу
        /// </summary>
        protected void AddRequiredComponent<T>() where T : struct, IComponent
        {
            _requiredComponents.Add(typeof(T));
        }

        /// <summary>
        /// Створює сутність з базовими компонентами архетипу
        /// </summary>
        public virtual int CreateEntity(IEntityManager entityManager)
        {
            int entityId = entityManager.CreateEntity();
            ApplyBaseComponents(entityId, entityManager);
            return entityId;
        }

        /// <summary>
        /// Застосовує базові компоненти архетипу до сутності
        /// </summary>
        protected abstract void ApplyBaseComponents(int entityId, IEntityManager entityManager);

        /// <summary>
        /// Перевіряє, чи має сутність всі необхідні компоненти архетипу
        /// </summary>
        public virtual bool MatchesArchetype(int entityId, IEntityManager entityManager)
        {
            foreach (Type componentType in _requiredComponents)
            {
                // Використовуємо рефлексію для виклику HasComponent<T>
                var method = typeof(IEntityManager).GetMethod("HasComponent").MakeGenericMethod(componentType);
                bool hasComponent = (bool)method.Invoke(entityManager, new object[] { entityId });

                if (!hasComponent)
                    return false;
            }

            return true;
        }
    }
}
