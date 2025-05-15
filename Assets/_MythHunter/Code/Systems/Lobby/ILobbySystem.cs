using Cysharp.Threading.Tasks;
using MythHunter.Core.ECS;
using System.Collections.Generic;

public interface ILobbySystem : ISystem
{
    void InitializeLobby(int playerCount);
    bool SelectHero(string archetypeId);
    void ConfirmSelection();
    UniTask<bool> StartGameAsync();
    List<string> GetAvailableHeroes();
    List<string> GetSelectedHeroes();
    bool AreAllPlayersReady();

    // ➕ Додай:
    int GetRemainingManaForCurrentPlayer();
}
