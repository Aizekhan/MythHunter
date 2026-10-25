namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Базовий клас для сутності
    /// </summary>
    public class Entity
    {
        public int Id { get; }
        
        public Entity(int id)
        {
            Id = id;
        }
    }
}