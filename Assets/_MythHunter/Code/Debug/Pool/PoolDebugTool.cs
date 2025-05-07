// Шлях: Assets/_MythHunter/Code/Debug/Pool/PoolDebugTool.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.DI;
using MythHunter.Debug.Core;
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MythHunter.Debug.Pool
{
    /// <summary>
    /// Розширений інструмент відлагодження для моніторингу пулів об'єктів
    /// </summary>
    public class PoolDebugTool : DebugToolBase
    {
        private readonly IPoolManager _poolManager;
        private readonly PoolMonitor _poolMonitor;
        private Vector2 _statsScrollPosition;
        private Vector2 _lifetimeScrollPosition;
        private Vector2 _sceneScrollPosition;
        private float _refreshInterval = 1f;
        private float _lastRefreshTime;
        private bool _showActiveObjects = true;
        private bool _showInactiveObjects = true;
        private bool _showPotentialLeaks = true;
        private int _trimThreshold = 20;
        private string _filterText = "";

        // Додаткові стани для UI
        private bool _showLifetimeSection = false;
        private bool _showSceneSection = false;
        private bool _collectDiagnostics = false;
        private List<DiagnosticSnapshot> _diagnosticHistory = new List<DiagnosticSnapshot>();

        // Статистика сцен
        private Dictionary<string, int> _poolsPerScene = new Dictionary<string, int>();
        private Dictionary<string, List<string>> _sceneAssociations = new Dictionary<string, List<string>>();

        [Inject]
        public PoolDebugTool(IPoolManager poolManager, IMythLogger logger)
            : base("Pool Monitor", "Performance", logger)
        {
            _poolManager = poolManager;
            _lastRefreshTime = Time.realtimeSinceStartup;

            // Отримуємо PoolMonitor через інтеграцію з OptimizedPoolManager
            var optimizedManager = _poolManager as OptimizedPoolManager;
            if (optimizedManager != null)
            {
                // Тут потрібно забезпечити, щоб _poolMonitor був доступний через OptimizedPoolManager
                // Можливо, додати публічний метод GetPoolMonitor в OptimizedPoolManager
            }

            // Підписуємося на події сцен для моніторингу
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Оновлюємо статистику сцен
            if (!_poolsPerScene.ContainsKey(scene.name))
            {
                _poolsPerScene[scene.name] = 0;
            }

            AddLogEntry($"Scene loaded: {scene.name}");
            RefreshData();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            AddLogEntry($"Scene unloaded: {scene.name}");
            RefreshData();
        }

        public override void Update()
        {
            if (!IsEnabled)
                return;

            if (Time.realtimeSinceStartup - _lastRefreshTime > _refreshInterval)
            {
                RefreshData();
                _lastRefreshTime = Time.realtimeSinceStartup;

                // Збираємо діагностику, якщо увімкнено
                if (_collectDiagnostics)
                {
                    CollectDiagnostics();
                }
            }
        }

        protected override void RefreshData()
        {
            // Отримуємо статистику від менеджера пулів
            var stats = _poolManager.GetAllPoolsStatistics();

            // Оновлюємо загальну статистику
            int totalActive = 0;
            int totalInactive = 0;
            int totalPools = stats.Count;

            // Очищуємо статистику сцен і потім оновлюємо її
            foreach (var scene in _sceneAssociations.Keys.ToList())
            {
                _sceneAssociations[scene].Clear();
            }

            foreach (var stat in stats.Values)
            {
                totalActive += stat.ActiveCount;
                totalInactive += stat.InactiveCount;

                // Спроба отримати інформацію про сцену
                var poolKey = stat.PoolKey;
                if (!string.IsNullOrEmpty(poolKey) && !string.IsNullOrEmpty(stat.SceneName))
                {
                    if (!_sceneAssociations.ContainsKey(stat.SceneName))
                    {
                        _sceneAssociations[stat.SceneName] = new List<string>();
                    }

                    _sceneAssociations[stat.SceneName].Add(poolKey);

                    if (_poolsPerScene.ContainsKey(stat.SceneName))
                    {
                        _poolsPerScene[stat.SceneName]++;
                    }
                }
            }

            UpdateStatistic("TotalPools", totalPools);
            UpdateStatistic("TotalActiveObjects", totalActive);
            UpdateStatistic("TotalInactiveObjects", totalInactive);
            UpdateStatistic("TotalScenes", _sceneAssociations.Count);

            // Виявлення потенційних проблем
            if (totalActive > 1000)
            {
                AddLogEntry($"Увага: велика кількість активних об'єктів: {totalActive}");
            }

            // Отримання інформації про життєвий цикл, якщо доступно
            if (_poolManager is OptimizedPoolManager optimizedManager)
            {
                var lifetimeStats = optimizedManager.GetLifetimeStatistics();
                if (lifetimeStats != null && lifetimeStats.Count > 0)
                {
                    int totalTracked = 0;
                    foreach (var poolStats in lifetimeStats.Values)
                    {
                        totalTracked += poolStats.Count;
                    }

                    UpdateStatistic("TrackedObjects", totalTracked);
                }
            }

            base.RefreshData();
        }

        // Додавання методу для збору діагностики
        private void CollectDiagnostics()
        {
            var stats = _poolManager.GetAllPoolsStatistics();

            // Створюємо знімок поточного стану
            _diagnosticHistory.Add(new DiagnosticSnapshot
            {
                Timestamp = DateTime.Now,
                ActiveCount = (int)_statistics["TotalActiveObjects"],
                InactiveCount = (int)_statistics["TotalInactiveObjects"],
                PoolStats = new Dictionary<string, PoolStatistics>(stats)
            });

            // Обмежуємо кількість збережених знімків
            if (_diagnosticHistory.Count > 100)
                _diagnosticHistory.RemoveAt(0);
        }

        protected override void RenderContent()
        {
            // Налаштування відображення
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Оновити", GUILayout.Width(100)))
            {
                RefreshData();
            }

            if (GUILayout.Button("Перевірити витоки", GUILayout.Width(150)))
            {
                _poolManager.CheckForLeaks();
                RefreshData();
                AddLogEntry("Перевірка витоків завершена");
            }

            if (GUILayout.Button("Очистити зайве", GUILayout.Width(150)))
            {
                _poolManager.TrimExcessObjects(_trimThreshold);
                RefreshData();
                AddLogEntry($"Очищення завершено з порогом {_trimThreshold}");
            }

            GUILayout.EndHorizontal();

            // Налаштування фільтрів і пошук
            GUILayout.BeginHorizontal();
            GUILayout.Label("Фільтр:", GUILayout.Width(50));
            string newFilter = GUILayout.TextField(_filterText, GUILayout.Width(150));
            if (newFilter != _filterText)
            {
                _filterText = newFilter;
                RefreshData();
            }
            GUILayout.EndHorizontal();

            // Налаштування відображення
            GUILayout.BeginHorizontal();
            _showActiveObjects = GUILayout.Toggle(_showActiveObjects, "Активні об'єкти", GUILayout.Width(150));
            _showInactiveObjects = GUILayout.Toggle(_showInactiveObjects, "Неактивні об'єкти", GUILayout.Width(150));
            _showPotentialLeaks = GUILayout.Toggle(_showPotentialLeaks, "Потенційні витоки", GUILayout.Width(150));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Поріг очищення:", GUILayout.Width(120));
            string trimText = GUILayout.TextField(_trimThreshold.ToString(), GUILayout.Width(50));
            if (int.TryParse(trimText, out int newTrim))
            {
                _trimThreshold = Mathf.Max(0, newTrim);
            }
            GUILayout.EndHorizontal();

            // Загальна статистика
            GUILayout.Label($"Всього пулів: {_statistics["TotalPools"]}", GUI.skin.box);
            GUILayout.Label($"Активних об'єктів: {_statistics["TotalActiveObjects"]}", GUI.skin.box);
            GUILayout.Label($"Неактивних об'єктів: {_statistics["TotalInactiveObjects"]}", GUI.skin.box);

            // Статистика по сценах
            if (_statistics.ContainsKey("TotalScenes"))
            {
                GUILayout.Label($"Сцен з пулами: {_statistics["TotalScenes"]}", GUI.skin.box);
            }

            // Статистика відстеження
            if (_statistics.ContainsKey("TrackedObjects"))
            {
                GUILayout.Label($"Відстежуваних об'єктів: {_statistics["TrackedObjects"]}", GUI.skin.box);
            }

            // Деталі пулів
            if (DrawFoldout("Деталі пулів"))
            {
                var stats = _poolManager.GetAllPoolsStatistics();

                _statsScrollPosition = GUILayout.BeginScrollView(_statsScrollPosition,
                    GUILayout.Height(300), GUILayout.ExpandWidth(true));

                foreach (var pair in stats.OrderBy(p => p.Key))
                {
                    string key = pair.Key;
                    PoolStatistics poolStat = pair.Value;

                    // Фільтрація 
                    if (!string.IsNullOrEmpty(_filterText) && !key.Contains(_filterText))
                        continue;

                    // Фільтрація за типами об'єктів
                    if ((!_showActiveObjects && poolStat.ActiveCount > 0) ||
                        (!_showInactiveObjects && poolStat.InactiveCount > 0))
                        continue;

                    // Фільтрація витоків
                    bool isPotentialLeak = poolStat.ActiveCount > poolStat.InactiveCount * 3;
                    if (isPotentialLeak && !_showPotentialLeaks)
                        continue;

                    GUILayout.BeginVertical(GUI.skin.box);

                    // Заголовок пулу з відповідним кольором
                    string poolColor = isPotentialLeak ? "orange" : "white";
                    GUILayout.Label($"<color={poolColor}><b>Пул: {key}</b> ({poolStat.PoolType})</color>");

                    // Деталі пулу
                    GUILayout.Label($"Активних: {poolStat.ActiveCount}, " +
                        $"Неактивних: {poolStat.InactiveCount}, " +
                        $"Всього: {poolStat.TotalSize}");

                    GUILayout.Label($"Отримано: {poolStat.TotalGetCount}, " +
                        $"Повернуто: {poolStat.TotalReturnCount}");

                    // Сцена, якщо доступна
                    if (!string.IsNullOrEmpty(poolStat.SceneName))
                    {
                        GUILayout.Label($"Сцена: {poolStat.SceneName}");
                    }

                    GUILayout.Label($"Створено: {DateTime.Now.Subtract(poolStat.CreationTime).ToString(@"hh\:mm\:ss")} тому");

                    GUILayout.EndVertical();
                }

                GUILayout.EndScrollView();
            }

            // Новий розділ - відстеження життєвого циклу об'єктів
            _showLifetimeSection = DrawFoldout("Життєвий цикл об'єктів");
            if (_showLifetimeSection && _poolManager is OptimizedPoolManager optimizedManager)
            {
                var lifetimeStats = optimizedManager.GetLifetimeStatistics();

                if (lifetimeStats != null && lifetimeStats.Count > 0)
                {
                    _lifetimeScrollPosition = GUILayout.BeginScrollView(_lifetimeScrollPosition,
                        GUILayout.Height(300), GUILayout.ExpandWidth(true));

                    foreach (var pair in lifetimeStats.OrderBy(p => p.Key))
                    {
                        string poolKey = pair.Key;
                        var objInfos = pair.Value;

                        // Фільтрація
                        if (!string.IsNullOrEmpty(_filterText) && !poolKey.Contains(_filterText))
                            continue;

                        GUILayout.BeginVertical(GUI.skin.box);
                        GUILayout.Label($"<b>Пул: {poolKey}</b> ({objInfos.Count} об'єктів)");

                        // Виводимо топ 5 об'єктів з найдовшим часом життя
                        var topObjects = objInfos
                            .Where(o => o.DeactivationTime == null) // Тільки активні
                            .OrderByDescending(o => (DateTime.Now - o.ActivationTime).TotalSeconds)
                            .Take(5);

                        foreach (var obj in topObjects)
                        {
                            float activeSeconds = (float)(DateTime.Now - obj.ActivationTime).TotalSeconds;

                            // Якщо об'єкт активний занадто довго, підсвічуємо його
                            string color = activeSeconds > 300 ? "red" : (activeSeconds > 60 ? "yellow" : "white");

                            GUILayout.Label($"<color={color}>Об'єкт {obj.InstanceId}, активний {activeSeconds:F1} сек" +
                                (string.IsNullOrEmpty(obj.LastSceneName) ? "" : $", сцена: {obj.LastSceneName}") +
                                $", повторно використаний {obj.ReuseCount} разів</color>");
                        }

                        GUILayout.EndVertical();
                    }

                    GUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.Label("Немає даних про життєвий цикл об'єктів", GUI.skin.box);
                }
            }

            // Новий розділ - асоціації сцен
            _showSceneSection = DrawFoldout("Асоціації сцен");
            if (_showSceneSection)
            {
                _sceneScrollPosition = GUILayout.BeginScrollView(_sceneScrollPosition,
                    GUILayout.Height(200), GUILayout.ExpandWidth(true));

                foreach (var pair in _sceneAssociations.OrderBy(p => p.Key))
                {
                    string sceneName = pair.Key;
                    var poolKeys = pair.Value;

                    // Фільтрація
                    if (!string.IsNullOrEmpty(_filterText) && !sceneName.Contains(_filterText))
                        continue;

                    GUILayout.BeginVertical(GUI.skin.box);

                    // Визначаємо, чи ця сцена активна зараз
                    bool isActiveScene = false;
                    for (int i = 0; i < SceneManager.sceneCount; i++)
                    {
                        if (SceneManager.GetSceneAt(i).name == sceneName)
                        {
                            isActiveScene = true;
                            break;
                        }
                    }

                    string sceneColor = isActiveScene ? "green" : "gray";
                    GUILayout.Label($"<color={sceneColor}><b>Сцена: {sceneName}</b> ({poolKeys.Count} пулів)</color>");

                    // Виводимо пули для цієї сцени
                    if (poolKeys.Count > 0)
                    {
                        string poolsList = string.Join(", ", poolKeys.Take(5));
                        if (poolKeys.Count > 5)
                        {
                            poolsList += $"... і ще {poolKeys.Count - 5}";
                        }

                        GUILayout.Label($"Пули: {poolsList}");
                    }
                    else
                    {
                        GUILayout.Label("Немає асоційованих пулів");
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.EndScrollView();
            }

            // Кнопка для вмикання/вимикання діагностики
            if (GUILayout.Button(_collectDiagnostics ? "Зупинити діагностику" : "Почати діагностику",
                GUILayout.Width(150)))
            {
                _collectDiagnostics = !_collectDiagnostics;
                AddLogEntry(_collectDiagnostics ? "Діагностика запущена" : "Діагностика зупинена");
            }

            // Відображення діагностики
            if (_collectDiagnostics && _diagnosticHistory.Count > 0)
            {
                GUILayout.Label("Динаміка зміни:");

                // Виводимо перший і останній знімок для порівняння
                var firstSnapshot = _diagnosticHistory.First();
                var lastSnapshot = _diagnosticHistory.Last();

                GUILayout.Label($"Початок: Активних: {firstSnapshot.ActiveCount}, Неактивних: {firstSnapshot.InactiveCount}");
                GUILayout.Label($"Поточний: Активних: {lastSnapshot.ActiveCount}, Неактивних: {lastSnapshot.InactiveCount}");

                // Різниця
                int activeDiff = lastSnapshot.ActiveCount - firstSnapshot.ActiveCount;
                int inactiveDiff = lastSnapshot.InactiveCount - firstSnapshot.InactiveCount;

                string activeColor = activeDiff > 0 ? "red" : (activeDiff < 0 ? "green" : "white");
                string inactiveColor = inactiveDiff > 0 ? "green" : (inactiveDiff < 0 ? "red" : "white");

                GUILayout.Label($"Зміна: Активних: <color={activeColor}>{activeDiff:+0;-0;0}</color>, " +
                    $"Неактивних: <color={inactiveColor}>{inactiveDiff:+0;-0;0}</color>");
            }

            // Відображення логів
            base.RenderContent();
        }

        // Деструктор - відписуємося від подій
        public override void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            base.Dispose();
        }

        // Структура для історичних даних
        private struct DiagnosticSnapshot
        {
            public DateTime Timestamp;
            public int ActiveCount;
            public int InactiveCount;
            public Dictionary<string, PoolStatistics> PoolStats;
        }
    }
}
