// Assets/_MythHunter/Code/Core/SceneManagement/ISceneDispatcher.cs
using Cysharp.Threading.Tasks;

namespace MythHunter.Core.SceneManagement
{
    /// <summary>
    /// Інтерфейс диспетчера сцен
    /// </summary>
    public interface ISceneDispatcher
    {
        /// <summary>
        /// Завантажує сцену за назвою
        /// </summary>
        UniTask LoadSceneAsync(string sceneName);

        /// <summary>
        /// Завантажує ігрову сцену з параметрами
        /// </summary>
        UniTask LoadGameSceneAsync(string[] selectedHeroArchetypes);
    }
}
