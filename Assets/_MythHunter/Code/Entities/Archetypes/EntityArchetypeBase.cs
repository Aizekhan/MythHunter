// Шлях: Assets/_MythHunter/Code/Entities/Archetypes/EntityArchetypeBase.cs

using MythHunter.Core.ECS;
using System;
using System.Collections.Generic;

namespace MythHunter.Entities
{
    public abstract class EntityArchetypeBase : IEntityArchetype
    {
        private readonly List<Func<IEntityManager, int, bool>> _componentCheckers = new List<Func<IEntityManager, int, bool>>();

        public abstract string ArchetypeId
        {
            get;
        }

        protected EntityArchetypeBase()
        {
            DefineRequiredComponents();
        }

        protected abstract void DefineRequiredComponents();

        protected void AddRequiredComponent<T>() where T : struct, IComponent
        {
            // Замість зберігання типу, зберігаємо делегат для перевірки компонента
            _componentCheckers.Add((entityManager, entityId) => entityManager.HasComponent<T>(entityId));
        }

        public virtual int CreateEntity(IEntityManager entityManager)
        {
            int entityId = entityManager.CreateEntity();
            ApplyBaseComponents(entityId, entityManager);
            return entityId;
        }

        protected abstract void ApplyBaseComponents(int entityId, IEntityManager entityManager);

        public virtual bool MatchesArchetype(int entityId, IEntityManager entityManager)
        {
            // Використовуємо збережені делегати замість рефлексії
            foreach (var checker in _componentCheckers)
            {
                if (!checker(entityManager, entityId))
                    return false;
            }

            return true;
        }
    }
}
