using MythHunter.UI.Models;
using UnityEngine;
/// <summary>
/// Розширення HeroCardModel з новими полями
/// </summary>
public static class HeroCardModelExtensions
{
    public static int GetLevel(this HeroCardModel model)
    {
        // Поки що статичний рівень, пізніше буде динамічний
        return 1;
    }
}
