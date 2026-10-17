// Path: Assets/_MythHunter/Code/Systems/Heroes/IRaceClassBonusSystem.cs
using MythHunter.Core.ECS;
using MythHunter.Entities.Heroes;

namespace MythHunter.Systems.Heroes
{
    public interface IRaceClassBonusSystem : ISystem
    {
        RacialBonuses GetRacialBonuses(HeroRace race);
        ClassBonuses GetClassBonuses(HeroClass heroClass);
    }
}
