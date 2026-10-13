// Assets/_MythHunter/Tests/Runtime/ComponentCacheThreadSafetyTests.cs
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.TestTools;
using MythHunter.Core.ECS;
using RPGFramework.ECS;
using Unity.Jobs;

namespace MythHunter.Tests.Runtime
{
    /// <summary>
    /// EPIC 08 Phase 3: Thread safety tests for ComponentCache
    /// Tests concurrent read/write operations on ComponentCache
    /// </summary>
    public class ComponentCacheThreadSafetyTests
    {
        private class TestEntityManager : IEntityManager
        {
            private readonly Dictionary<int, Dictionary<System.Type, object>> _entities = new();
            private readonly object _lock = new object();
            private int _nextId = 1;

            public int CreateEntity()
            {
                lock (_lock)
                {
                    int id = _nextId++;
                    _entities[id] = new Dictionary<System.Type, object>();
                    return id;
                }
            }

            public void DestroyEntity(int entityId)
            {
                lock (_lock)
                {
                    _entities.Remove(entityId);
                }
            }

            public void AddComponent<TComponent>(int entityId, TComponent component) where TComponent : IComponent
            {
                lock (_lock)
                {
                    if (!_entities.ContainsKey(entityId))
                        _entities[entityId] = new Dictionary<System.Type, object>();
                    _entities[entityId][typeof(TComponent)] = component;
                }
            }

            public void RemoveComponent<TComponent>(int entityId) where TComponent : IComponent
            {
                lock (_lock)
                {
                    if (_entities.ContainsKey(entityId))
                        _entities[entityId].Remove(typeof(TComponent));
                }
            }

            public TComponent GetComponent<TComponent>(int entityId) where TComponent : IComponent
            {
                lock (_lock)
                {
                    if (_entities.ContainsKey(entityId) && _entities[entityId].TryGetValue(typeof(TComponent), out var component))
                        return (TComponent)component;
                    return default;
                }
            }

            public bool TryGetComponent<TComponent>(int entityId, out TComponent component) where TComponent : struct, IComponent
            {
                lock (_lock)
                {
                    component = default;
                    if (_entities.ContainsKey(entityId) && _entities[entityId].TryGetValue(typeof(TComponent), out var comp))
                    {
                        component = (TComponent)comp;
                        return true;
                    }
                    return false;
                }
            }

            public bool HasComponent<TComponent>(int entityId) where TComponent : IComponent
            {
                lock (_lock)
                {
                    return _entities.ContainsKey(entityId) && _entities[entityId].ContainsKey(typeof(TComponent));
                }
            }

            public int[] GetAllEntities()
            {
                lock (_lock)
                {
                    return _entities.Keys.ToArray();
                }
            }

            public int[] GetEntitiesWith<TComponent>() where TComponent : IComponent
            {
                lock (_lock)
                {
                    var result = new List<int>();
                    foreach (var kvp in _entities)
                    {
                        if (kvp.Value.ContainsKey(typeof(TComponent)))
                            result.Add(kvp.Key);
                    }
                    return result.ToArray();
                }
            }

            public void UpdateComponent<T>(int entityId, T component) where T : struct, IComponent
            {
                AddComponent(entityId, component);
            }

            public void ClearComponent<T>(int entityId) where T : struct, IComponent
            {
                RemoveComponent<T>(entityId);
            }
        }

        private struct TestComponent : IComponent
        {
            public int Value;
            public JobHandle JobHandle { get; set; }
        }

        private TestEntityManager _entityManager;
        private ComponentCache<TestComponent> _cache;

        [SetUp]
        public void Setup()
        {
            _entityManager = new TestEntityManager();
            _cache = new ComponentCache<TestComponent>(_entityManager);
        }

        [TearDown]
        public void TearDown()
        {
            _cache?.Dispose();
        }

        [Test]
        public void ConcurrentReads_DoNotThrow()
        {
            // Arrange: Create test entities
            int[] entityIds = new int[100];
            for (int i = 0; i < entityIds.Length; i++)
            {
                entityIds[i] = _entityManager.CreateEntity();
                _entityManager.AddComponent(entityIds[i], new TestComponent { Value = i });
            }

            _cache.Update();

            // Act: Concurrent reads
            var tasks = new Task[10];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        int entityId = entityIds[j % entityIds.Length];
                        var component = _cache.Get(entityId);
                        Assert.That(component.Value, Is.GreaterThanOrEqualTo(0));
                    }
                });
            }

            // Assert: All tasks complete without exception
            Assert.DoesNotThrow(() => Task.WaitAll(tasks));
        }

        [Test]
        public void ConcurrentWrites_DoNotThrow()
        {
            // Arrange: Create test entities
            int[] entityIds = new int[100];
            for (int i = 0; i < entityIds.Length; i++)
            {
                entityIds[i] = _entityManager.CreateEntity();
            }

            // Act: Concurrent writes
            var tasks = new Task[10];
            for (int i = 0; i < tasks.Length; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < 100; j++)
                    {
                        int entityId = entityIds[j];
                        _cache.Add(entityId, new TestComponent { Value = threadId * 100 + j });
                    }
                });
            }

            // Assert: All tasks complete without exception
            Assert.DoesNotThrow(() => Task.WaitAll(tasks));
        }

        [Test]
        public void ConcurrentReadWrite_DoNotThrow()
        {
            // Arrange: Create test entities
            int[] entityIds = new int[100];
            for (int i = 0; i < entityIds.Length; i++)
            {
                entityIds[i] = _entityManager.CreateEntity();
                _entityManager.AddComponent(entityIds[i], new TestComponent { Value = i });
            }

            _cache.Update();

            // Act: Mixed concurrent operations
            var tasks = new Task[20];
            for (int i = 0; i < tasks.Length; i++)
            {
                int taskId = i;
                if (i < 10)
                {
                    // Read tasks
                    tasks[i] = Task.Run(() =>
                    {
                        for (int j = 0; j < 500; j++)
                        {
                            int entityId = entityIds[j % entityIds.Length];
                            _cache.Get(entityId);
                            _cache.Contains(entityId);
                        }
                    });
                }
                else
                {
                    // Write tasks
                    tasks[i] = Task.Run(() =>
                    {
                        for (int j = 0; j < 50; j++)
                        {
                            int entityId = entityIds[j % entityIds.Length];
                            _cache.Add(entityId, new TestComponent { Value = taskId * 100 + j });
                        }
                    });
                }
            }

            // Assert: All tasks complete without exception
            Assert.DoesNotThrow(() => Task.WaitAll(tasks));
        }

        [Test]
        public void ConcurrentUpdateAndRead_DataIntegrity()
        {
            // Arrange: Create test entities
            int[] entityIds = new int[10];
            for (int i = 0; i < entityIds.Length; i++)
            {
                entityIds[i] = _entityManager.CreateEntity();
                _entityManager.AddComponent(entityIds[i], new TestComponent { Value = i });
            }

            _cache.Update();

            var readValues = new List<int>();
            var readLock = new object();

            // Act: One update task, multiple read tasks
            var updateTask = Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    _cache.Update();
                    Thread.Sleep(1);
                }
            });

            var readTasks = new Task[5];
            for (int i = 0; i < readTasks.Length; i++)
            {
                readTasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < 1000; j++)
                    {
                        int entityId = entityIds[j % entityIds.Length];
                        var component = _cache.Get(entityId);

                        lock (readLock)
                        {
                            readValues.Add(component.Value);
                        }
                    }
                });
            }

            // Assert: All tasks complete without exception
            Assert.DoesNotThrow(() => Task.WaitAll(new[] { updateTask }.Concat(readTasks).ToArray()));

            // Assert: All read values are valid
            Assert.That(readValues.Count, Is.GreaterThan(0));
            Assert.That(readValues.All(v => v >= 0 && v < 10), Is.True, "All values should be in valid range");
        }

        [Test]
        public void ConcurrentAddRemove_NoDeadlock()
        {
            // Arrange
            var entityIds = new List<int>();
            for (int i = 0; i < 50; i++)
            {
                int id = _entityManager.CreateEntity();
                entityIds.Add(id);
                _entityManager.AddComponent(id, new TestComponent { Value = i });
            }

            _cache.Update();

            // Act: Concurrent add/remove operations
            var addTask = Task.Run(() =>
            {
                for (int i = 0; i < 200; i++)
                {
                    int entityId = entityIds[i % entityIds.Count];
                    _cache.Add(entityId, new TestComponent { Value = i });
                    Thread.Sleep(1);
                }
            });

            var removeTask = Task.Run(() =>
            {
                for (int i = 0; i < 200; i++)
                {
                    int entityId = entityIds[i % entityIds.Count];
                    _cache.Remove(entityId);
                    Thread.Sleep(1);
                }
            });

            var readTask = Task.Run(() =>
            {
                for (int i = 0; i < 500; i++)
                {
                    int entityId = entityIds[i % entityIds.Count];
                    _cache.Contains(entityId);
                }
            });

            // Assert: All tasks complete within reasonable time (no deadlock)
            bool completed = Task.WaitAll(new[] { addTask, removeTask, readTask }, TimeSpan.FromSeconds(10));
            Assert.That(completed, Is.True, "Tasks should complete without deadlock");
        }

        [Test]
        public void GetEntityIdsArray_ThreadSafe()
        {
            // Arrange
            for (int i = 0; i < 50; i++)
            {
                int id = _entityManager.CreateEntity();
                _entityManager.AddComponent(id, new TestComponent { Value = i });
            }

            _cache.Update();

            // Act: Concurrent GetEntityIdsArray calls
            var tasks = new Task<int[]>[10];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    var arrays = new List<int[]>();
                    for (int j = 0; j < 100; j++)
                    {
                        arrays.Add(_cache.GetEntityIdsArray());
                    }
                    return arrays[arrays.Count - 1];
                });
            }

            // Assert: All tasks complete without exception
            Assert.DoesNotThrow(() => Task.WaitAll(tasks));

            // Assert: All arrays have correct count
            foreach (var task in tasks)
            {
                Assert.That(task.Result.Length, Is.EqualTo(50));
            }
        }

        // NOTE: UnityTest attribute requires assembly definition with TestAssemblies reference.
        // Temporarily commented out until MythHunter.Code has proper assembly definition.
        // [UnityTest]
        // public IEnumerator Dispose_ReleasesLocks()
        // {
        //     // Arrange
        //     for (int i = 0; i < 10; i++)
        //     {
        //         int id = _entityManager.CreateEntity();
        //         _entityManager.AddComponent(id, new TestComponent { Value = i });
        //     }
        //
        //     _cache.Update();
        //
        //     // Act: Dispose while operations might be in progress
        //     var readTask = Task.Run(() =>
        //     {
        //         try
        //         {
        //             for (int i = 0; i < 100; i++)
        //             {
        //                 _cache.GetStatistics();
        //                 Thread.Sleep(10);
        //             }
        //         }
        //         catch (System.ObjectDisposedException)
        //         {
        //             // Expected if disposed during operation
        //         }
        //     });
        //
        //     yield return null;
        //
        //     _cache.Dispose();
        //
        //     // Assert: Task completes (not stuck on lock)
        //     bool completed = readTask.Wait(TimeSpan.FromSeconds(5));
        //     Assert.That(completed, Is.True, "Task should complete after Dispose");
        // }
    }
}
