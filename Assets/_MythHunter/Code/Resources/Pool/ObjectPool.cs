// ObjectPool.cs
namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Реалізація пулу об'єктів
    /// </summary>
    public class ObjectPool<T> : IObjectPool<T> where T : UnityEngine.Object
    {
        private readonly System.Collections.Generic.Stack<T> _inactiveObjects;
        private readonly System.Collections.Generic.HashSet<T> _activeObjects;
        private readonly T _prefab;

        public ObjectPool(T prefab, int initialSize)
        {
            _prefab = prefab;
            _inactiveObjects = new System.Collections.Generic.Stack<T>(initialSize);
            _activeObjects = new System.Collections.Generic.HashSet<T>();

            // Створення початкового пулу об'єктів
            for (int i = 0; i < initialSize; i++)
            {
                T obj = UnityEngine.Object.Instantiate(_prefab);

                if (obj is UnityEngine.GameObject gameObject)
                    gameObject.SetActive(false);

                _inactiveObjects.Push(obj);
            }
        }

        public T Get()
        {
            T obj;

            if (_inactiveObjects.Count > 0)
            {
                obj = _inactiveObjects.Pop();
            }
            else
            {
                obj = UnityEngine.Object.Instantiate(_prefab);
            }

            _activeObjects.Add(obj);

            // Активуємо об'єкт, якщо це GameObject
            if (obj is UnityEngine.GameObject gameObject)
                gameObject.SetActive(true);

            return obj;
        }

        public void Return(T instance)
        {
            if (_activeObjects.Remove(instance))
            {
                // Деактивуємо об'єкт, якщо це GameObject
                if (instance is UnityEngine.GameObject gameObject)
                    gameObject.SetActive(false);

                _inactiveObjects.Push(instance);
            }
        }

        public void Clear()
        {
            // Знищуємо всі активні об'єкти
            foreach (var obj in _activeObjects)
            {
                UnityEngine.Object.Destroy(obj);
            }

            // Знищуємо всі неактивні об'єкти
            foreach (var obj in _inactiveObjects)
            {
                UnityEngine.Object.Destroy(obj);
            }

            _activeObjects.Clear();
            _inactiveObjects.Clear();
        }

        public int CountActive => _activeObjects.Count;
        public int CountInactive => _inactiveObjects.Count;
    }
}
