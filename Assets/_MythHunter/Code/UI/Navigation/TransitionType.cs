// Шлях: Assets/_MythHunter/Code/UI/Navigation/TransitionType.cs

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Типи анімацій переходу між екранами
    /// </summary>
    public enum TransitionType
    {
        Default,        // Стандартний перехід (визначений у системі)
        None,           // Без анімації
        Fade,           // Поступове затухання/поява
        SlideLeft,      // Зсув ліворуч
        SlideRight,     // Зсув праворуч
        SlideUp,        // Зсув вгору
        SlideDown,      // Зсув вниз
        Scale,          // Масштабування
        Custom          // Користувацький перехід (визначений у представленні)
    }
}
