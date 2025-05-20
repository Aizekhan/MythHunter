// Шлях: Assets/_MythHunter/Code/UI/Navigation/IScreenTransition.cs

using Cysharp.Threading.Tasks;
using MythHunter.UI.Core;
using UnityEngine;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Інтерфейс для анімацій переходу між екранами
    /// </summary>
    public interface IScreenTransition
    {
        /// <summary>
        /// Відтворити анімацію переходу
        /// </summary>
        UniTask PlayTransitionAsync(GameObject fromScreen, GameObject toScreen, TransitionType type, bool isForward);
    }
}
