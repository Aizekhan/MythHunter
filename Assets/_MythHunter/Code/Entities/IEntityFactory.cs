namespace MythHunter.Entities
{
    public interface IEntityFactory
    {
        int CreatePlayerCharacter(string characterName);
        int CreateEnemy(string enemyName, float health, float attackPower);

    }
}
