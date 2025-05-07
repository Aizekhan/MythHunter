using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Debug.Core;
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Debug.Pool
{
    /// <summary>
    /// Інструмент відлагодження для моніторингу пулів об'єктів
    /// </summary>
    public class PoolDebugTool : DebugToolBase
    {
        private readonly IPoolManager _poolManager;
        private Vector2 _statsScrollPosition;
        private float _refreshInterval = 1f;
        private float _lastRefreshTime;
        private bool _showActiveObjects = false;
        private bool _showInactiveObjects = true;
        private int _trimThreshold = 20;

        [Inject]
        public PoolDebugTool(IPoolManager poolManager, IMythLogger logger)
            : base("Pool Monitor", "Performance", logger)
        {
            _poolManager = poolManager;
            _lastRefreshTime = Time.realtimeSinceStartup;
        }

        public override void Update()
        {
            if (Time.realtimeSinceStartup - _lastRefreshTime > _refreshInterval)
            {
                RefreshData();
                _lastRefreshTime = Time.realtimeSinceStartup;
            }
        }

        protected override void RefreshData()
        {
            // Отримуємо статистику від менеджера пулів
            var stats = _poolManager.GetAllPoolsStatistics();

            // Оновлюємо загальну статистику
            int totalActive = 0;
            int totalInactive = 0;

            foreach (var stat in stats.Values)
            {
                totalActive += stat.ActiveCount;
                totalInactive += stat.InactiveCount;
            }

            UpdateStatistic("TotalPools", stats.Count);
            UpdateStatistic("TotalActiveObjects", totalActive);
            UpdateStatistic("TotalInactiveObjects", totalInactive);

            // Логуємо потенційні проблеми
            if (totalActive > 1000)
            {
                AddLogEntry($"Увага: велика кількість активних об'єктів: {totalActive}");
            }

            base.RefreshData();
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

            // Налаштування фільтрів
            GUILayout.BeginHorizontal();
            _showActiveObjects = GUILayout.Toggle(_showActiveObjects, "Активні об'єкти", GUILayout.Width(150));
            _showInactiveObjects = GUILayout.Toggle(_showInactiveObjects, "Неактивні об'єкти", GUILayout.Width(150));
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

            // Деталі пулів
            if (DrawFoldout("Деталі пулів"))
            {
                var stats = _poolManager.GetAllPoolsStatistics();

                _statsScrollPosition = GUILayout.BeginScrollView(_statsScrollPosition,
                    GUILayout.Height(300), GUILayout.ExpandWidth(true));

                foreach (var pair in stats)
                {
                    string key = pair.Key;
                    PoolStatistics poolStat = pair.Value;

                    // Фільтрація за типами об'єктів
                    if ((!_showActiveObjects && poolStat.ActiveCount > 0) ||
                        (!_showInactiveObjects && poolStat.InactiveCount > 0))
                        continue;

                    GUILayout.BeginVertical(GUI.skin.box);

                    // Заголовок пулу (з кольором для потенційних проблем)
                    string poolColor = poolStat.ActiveCount > 100 ? "orange" : "white";
                    GUILayout.Label($"<color={poolColor}><b>Пул: {key}</b> ({poolStat.PoolType})</color>");

                    // Деталі пулу
                    GUILayout.Label($"Активних: {poolStat.ActiveCount}, " +
                        $"Неактивних: {poolStat.InactiveCount}, " +
                        $"Всього: {poolStat.TotalSize}");

                    GUILayout.Label($"Отримано: {poolStat.TotalGetCount}, " +
                        $"Повернуто: {poolStat.TotalReturnCount}");

                    GUILayout.Label($"Створено: {DateTime.Now.Subtract(poolStat.CreationTime).ToString(@"hh\:mm\:ss")} тому");

                    GUILayout.EndVertical();
                }

                GUILayout.EndScrollView();
            }
            // Додайте в RenderContent() кнопку для вмикання/вимикання діагностики
            if (GUILayout.Button(_collectDiagnostics ? "Зупинити діагностику" : "Почати діагностику",
                GUILayout.Width(150)))
            {
                _collectDiagnostics = !_collectDiagnostics;
                AddLogEntry(_collectDiagnostics ? "Діагностика запущена" : "Діагностика зупинена");
            }
            // Відображення логів
            base.RenderContent();
        }


        private bool _collectDiagnostics = false;
        private List<DiagnosticSnapshot> _diagnosticHistory = new List<DiagnosticSnapshot>();

        // Структура для історичних даних
        private struct DiagnosticSnapshot
        {
            public DateTime Timestamp;
            public int ActiveCount;
            public int InactiveCount;
            public Dictionary<string, PoolStatistics> PoolStats;
        }

        // Додайте метод для запису діагностики
        private void CollectDiagnostics()
        {
            if (!_collectDiagnostics)
                return;

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


     }
}
