using MythHunter.Core.ECS;

namespace MythHunter.Entities
{
    /// <summary>
    /// Інтерфейс для архетипів сутностей
    /// </summary>
    public interface IEntityArchetype
    {
        /// <summary>
        /// Унікальний ідентифікатор архетипу
        /// </summary>
        string ArchetypeId
        {
            get;
        }

        /// <summary>
        /// Створює сутність на основі архетипу
        /// </summary>
        int CreateEntity(IEntityManager entityManager);

        /// <summary>
        /// Перевіряє, чи належить сутність до цього архетипу
        /// </summary>
        bool MatchesArchetype(int entityId, IEntityManager entityManager);
    }
}
