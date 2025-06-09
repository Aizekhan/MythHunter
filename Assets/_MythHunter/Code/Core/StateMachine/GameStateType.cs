// Assets/_MythHunter/Code/Core/StateMachine/GameStateType.cs
using System;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Типи станів гри
    /// </summary>
    public enum GameStateType
    {
        None = 0,

        [NavigationBehavior(NavigationClearType.Always)]
        Boot,

        [NavigationBehavior(NavigationClearType.Always)]
        MainMenu,

        [NavigationBehavior(NavigationClearType.Never)]
        Loading,

        [NavigationBehavior(NavigationClearType.Never)]
        Lobby = 5,

        [NavigationBehavior(NavigationClearType.Never)]
        Profile, // ✅ НОВИЙ стан для профілю

        [NavigationBehavior(NavigationClearType.OnExit)]
        Gameplay,

        [NavigationBehavior(NavigationClearType.Never)]
        Pause,

        [NavigationBehavior(NavigationClearType.Always)]
        GameOver
    }

    /// <summary>
    /// Поведінка навігації для стану
    /// </summary>
    public enum NavigationClearType
    {
        Never,    // Ніколи не очищати навігацію
        Always,   // Завжди очищати при вході в цей стан
        OnExit    // Очищати при виході з цього стану
    }

    /// <summary>
    /// Атрибут для визначення поведінки навігації
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class NavigationBehaviorAttribute : Attribute
    {
        public NavigationClearType ClearType
        {
            get;
        }

        public NavigationBehaviorAttribute(NavigationClearType clearType)
        {
            ClearType = clearType;
        }
    }
}
