// Шлях: Assets/_MythHunter/Code/UI/Core/IUIComponentFactory.cs
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс фабрики компонентів UI
    /// </summary>
    public interface IUIComponentFactory
    {
        /// <summary>
        /// Створює компонент із ViewId та визначає батьківський трансформ
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <param name="parent">Батьківський трансформ (необов'язковий)</param>
        /// <returns>Створений компонент</returns>
        UniTask<GameObject> CreateComponentAsync(ViewId viewId, Transform parent = null);

        /// <summary>
        /// Створює компонент із пулу за ViewId
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <param name="parent">Батьківський трансформ (необов'язковий)</param>
        /// <returns>Створений компонент з пулу</returns>
        UniTask<GameObject> CreateFromPoolAsync(ViewId viewId, Transform parent = null);

        /// <summary>
        /// Повертає компонент у пул
        /// </summary>
        /// <param name="viewId">Ідентифікатор представлення</param>
        /// <param name="gameObject">Ігровий об'єкт для повернення</param>
        void ReturnToPool(ViewId viewId, GameObject gameObject);

        // Методи для зворотної сумісності
        [System.Obsolete("Використовуйте CreateComponentAsync(ViewId, Transform) для відповідності архітектурним принципам")]
        T CreateComponent<T>(GameObject prefab, Transform parent = null) where T : MonoBehaviour;

        [System.Obsolete("Використовуйте CreateComponentAsync(ViewId, Transform) для відповідності архітектурним принципам")]
        UniTask<T> CreateComponentAsync<T>(string prefabPath, Transform parent = null) where T : MonoBehaviour;

        [System.Obsolete("Використовуйте CreateFromPoolAsync(ViewId, Transform) для відповідності архітектурним принципам")]
        UniTask<T> CreateFromPoolAsync<T>(string prefabPath, Transform parent = null) where T : MonoBehaviour;

        [System.Obsolete("Використовуйте ReturnToPool(ViewId, GameObject) для відповідності архітектурним принципам")]
        void ReturnToPool<T>(T component) where T : MonoBehaviour;
    }
}
