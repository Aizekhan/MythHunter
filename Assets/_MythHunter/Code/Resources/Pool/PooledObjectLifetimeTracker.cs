// Шлях: Assets/_MythHunter/Code/Resources/Pool/PooledObjectLifetimeTracker.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using MythHunter.Utils.Logging;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Система для відстеження часу життя об'єктів пулу
    /// </summary>
    public class PooledObjectLifetimeTracker
    {
        private readonly Dictionary<int, ObjectLifetimeInfo> _lifetimeInfo = new Dictionary<int, ObjectLifetimeInfo>();
        private readonly IMythLogger _logger;
        private readonly int _maxHistorySize = 50;

        // Інформація про час життя об'єкта пулу
        public class ObjectLifetimeInfo
        {
            // Основна інформація
            public string PoolKey
            {
                get; set;
            }
            public int InstanceId
            {
                get; set;
            }
            public Type ObjectType
            {
                get; set;
            }
            public DateTime ActivationTime
            {
                get; set;
            }
            public DateTime? DeactivationTime
            {
                get; set;
            }

            // Розширена статистика
            public List<LifetimeSession> Sessions { get; } = new List<LifetimeSession>();
            public int ReuseCount { get; set; } = 0;
            public float TotalActiveTime { get; set; } = 0;
            public string LastComponentPath { get; set; } = string.Empty; // Для GameObject - шлях до об'єкта

            // Додаткова інформація для GameObject
            public Vector3 LastPosition
            {
                get; set;
            }
            public string LastSceneName { get; set; } = string.Empty;

            public void AddSession(int maxHistorySize)
            {
                var session = new LifetimeSession
                {
                    StartTime = DateTime.Now
                };

                if (Sessions.Count >= maxHistorySize)
                {
                    Sessions.RemoveAt(0);
                }

                Sessions.Add(session);
                ReuseCount++;
            }

            public void EndCurrentSession()
            {
                if (Sessions.Count > 0)
                {
                    var session = Sessions[Sessions.Count - 1];
                    session.EndTime = DateTime.Now;
                    session.Duration = (float)(session.EndTime.Value - session.StartTime).TotalSeconds;
                    TotalActiveTime += session.Duration;
                }
            }

            // Отримання середнього часу активності
            public float GetAverageSessionTime()
            {
                if (ReuseCount == 0)
                    return 0;
                return TotalActiveTime / ReuseCount;
            }
        }

        // Сесія час життя об'єкту (для трекінгу історії активації/деактивації)
        public class LifetimeSession
        {
            public DateTime StartTime
            {
                get; set;
            }
            public DateTime? EndTime
            {
                get; set;
            }
            public float Duration
            {
                get; set;
            }
            public string ContextInfo { get; set; } = string.Empty; // Додаткова інформація про контекст
        }

        public PooledObjectLifetimeTracker(IMythLogger logger, int maxHistorySize = 50)
        {
            _logger = logger;
            _maxHistorySize = maxHistorySize;
        }

        // Реєстрація активації об'єкта
        public void TrackActivation(string poolKey, UnityEngine.Object instance, string contextInfo = "")
        {
            if (instance == null)
                return;
            GameObject go = instance as GameObject;
            int instanceId = instance.GetInstanceID();

            if (!_lifetimeInfo.TryGetValue(instanceId, out var info))
            {
                info = new ObjectLifetimeInfo
                {
                    PoolKey = poolKey,
                    InstanceId = instanceId,
                    ObjectType = instance.GetType(),
                    ActivationTime = DateTime.Now
                };

                _lifetimeInfo[instanceId] = info;

                // Додаткова інформація для GameObject
                if (go != null)
                {
                    info.LastComponentPath = GetGameObjectPath(go);
                    info.LastPosition = go.transform.position;
                    info.LastSceneName = go.scene.name;
                }
            }

            // Додати нову сесію
            info.AddSession(_maxHistorySize);

            // Оновлення інформації про GameObject
            if (go != null)
            {
                info.LastComponentPath = GetGameObjectPath(go);
                info.LastPosition = go.transform.position;
                info.LastSceneName = go.scene.name;
            }

            _logger.LogDebug($"Object {instanceId} activated from pool '{poolKey}', reuse count: {info.ReuseCount}", "LifetimeTracker");
        }

        // Реєстрація деактивації об'єкта
        public void TrackDeactivation(string poolKey, UnityEngine.Object instance)
        {
            if (instance == null)
                return;

            int instanceId = instance.GetInstanceID();

            if (_lifetimeInfo.TryGetValue(instanceId, out var info))
            {
                info.DeactivationTime = DateTime.Now;
                info.EndCurrentSession();

                // Оновлення інформації про GameObject
                if (instance is GameObject go)
                {
                    info.LastComponentPath = GetGameObjectPath(go);
                    info.LastPosition = go.transform.position;
                    info.LastSceneName = go.scene.name;
                }

                float activeTime = info.Sessions.Count > 0 ? info.Sessions[info.Sessions.Count - 1].Duration : 0;
                _logger.LogDebug($"Object {instanceId} deactivated from pool '{poolKey}', active time: {activeTime:F2}s", "LifetimeTracker");
            }
        }

        // Отримання статистики часу життя об'єктів
        public Dictionary<string, List<ObjectLifetimeInfo>> GetLifetimeStatistics()
        {
            var result = new Dictionary<string, List<ObjectLifetimeInfo>>();

            foreach (var info in _lifetimeInfo.Values)
            {
                if (!result.TryGetValue(info.PoolKey, out var poolStats))
                {
                    poolStats = new List<ObjectLifetimeInfo>();
                    result[info.PoolKey] = poolStats;
                }

                poolStats.Add(info);
            }

            return result;
        }

        // Отримання шляху до GameObject в ієрархії
        private string GetGameObjectPath(GameObject obj)
        {
            if (obj == null)
                return string.Empty;

            string path = obj.name;
            Transform parent = obj.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        // Аналіз потенційних витоків на основі часу життя
        public List<ObjectLifetimeInfo> DetectPotentialLeaks(float thresholdSeconds = 300f)
        {
            var potentialLeaks = new List<ObjectLifetimeInfo>();
            var now = DateTime.Now;

            foreach (var info in _lifetimeInfo.Values)
            {
                // Якщо об'єкт активний занадто довго
                if (info.DeactivationTime == null &&
                    (now - info.ActivationTime).TotalSeconds > thresholdSeconds)
                {
                    potentialLeaks.Add(info);
                }
                // Або якщо середній час активності надто великий
                else if (info.GetAverageSessionTime() > thresholdSeconds)
                {
                    potentialLeaks.Add(info);
                }
            }

            return potentialLeaks;
        }

        // Очищення інформації для знищених об'єктів
        public void CleanupDestroyed()
        {
            var idsToRemove = new List<int>();

            foreach (var pair in _lifetimeInfo)
            {
                if (pair.Value.ObjectType == typeof(GameObject))
                {
                     #if UNITY_EDITOR
                    var go = UnityEditor.EditorUtility.InstanceIDToObject(pair.Key) as GameObject;
                    if (go == null)
                    {
                        idsToRemove.Add(pair.Key);
                    }
                   #endif
                }
            }

            foreach (var id in idsToRemove)
            {
                _lifetimeInfo.Remove(id);
            }

            if (idsToRemove.Count > 0)
            {
                _logger.LogInfo($"Cleaned up lifetime info for {idsToRemove.Count} destroyed objects", "LifetimeTracker");
            }
        }
    }
}
