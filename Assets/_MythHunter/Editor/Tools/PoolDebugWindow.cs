// Шлях: Assets/_MythHunter/Code/Resources/Pool/Editor/PoolDebugWindow.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MythHunter.Resources.Pool;

namespace MythHunter.Resources.Pool.Editor
{
    /// <summary>
    /// Вікно для відлагодження пулів об'єктів
    /// </summary>
    public class PoolDebugWindow : EditorWindow
    {
        private IPoolManager _poolManager;
        private Vector2 _scrollPosition;
        private float _updateInterval = 1.0f;
        private float _lastUpdateTime;
        private Dictionary<string, PoolStatistics> _statistics = new Dictionary<string, PoolStatistics>();
        private bool _autoRefresh = true;
        private int _totalActiveObjects = 0;

        [MenuItem("MythHunter/Debug/Pool Monitor")]
        public static void ShowWindow()
        {
            GetWindow<PoolDebugWindow>("Pool Monitor");
        }

        private void OnGUI()
        {
            // Заголовок
            EditorGUILayout.LabelField("MythHunter Pool Monitor", EditorStyles.boldLabel);

            GUILayout.Space(10);

            // Кнопки та опції
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
            {
                RefreshStatistics();
            }

            _autoRefresh = EditorGUILayout.Toggle("Auto Refresh", _autoRefresh, GUILayout.Width(150));

            if (GUILayout.Button("Trim All Pools", GUILayout.Width(120)))
            {
                if (_poolManager != null)
                {
                    _poolManager.TrimExcessObjects();
                    RefreshStatistics();
                }
            }

            if (GUILayout.Button("Check For Leaks", GUILayout.Width(120)))
            {
                if (_poolManager != null)
                {
                    _poolManager.CheckForLeaks();
                    RefreshStatistics();
                }
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Загальна інформація
            EditorGUILayout.LabelField($"Total Active Objects: {_totalActiveObjects}", EditorStyles.boldLabel);

            GUILayout.Space(10);

            // Таблиця пулів
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Заголовок таблиці
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUILayout.LabelField("Pool Key", EditorStyles.toolbarButton, GUILayout.Width(150));
            EditorGUILayout.LabelField("Type", EditorStyles.toolbarButton, GUILayout.Width(100));
            EditorGUILayout.LabelField("Active", EditorStyles.toolbarButton, GUILayout.Width(80));
            EditorGUILayout.LabelField("Inactive", EditorStyles.toolbarButton, GUILayout.Width(80));
            EditorGUILayout.LabelField("Total", EditorStyles.toolbarButton, GUILayout.Width(80));
            EditorGUILayout.LabelField("Get/Return", EditorStyles.toolbarButton, GUILayout.Width(100));
            EditorGUILayout.EndHorizontal();

            // Рядки таблиці
            if (_statistics != null)
            {
                foreach (var pair in _statistics.OrderByDescending(p => p.Value.ActiveCount))
                {
                    string key = pair.Key;
                    var stats = pair.Value;

                    // Визначення кольору для активних об'єктів
                    Color originalColor = GUI.color;
                    if (stats.ActiveCount > 50)
                        GUI.color = new Color(1, 0.6f, 0.6f); // Червоний для багатьох активних
                    else if (stats.ActiveCount > 20)
                        GUI.color = new Color(1, 0.8f, 0.4f); // Жовтий для середньої кількості

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(key, GUILayout.Width(150));
                    EditorGUILayout.LabelField(stats.PoolType, GUILayout.Width(100));
                    EditorGUILayout.LabelField(stats.ActiveCount.ToString(), GUILayout.Width(80));
                    EditorGUILayout.LabelField(stats.InactiveCount.ToString(), GUILayout.Width(80));
                    EditorGUILayout.LabelField(stats.TotalSize.ToString(), GUILayout.Width(80));
                    EditorGUILayout.LabelField($"{stats.TotalGetCount}/{stats.TotalReturnCount}", GUILayout.Width(100));
                    EditorGUILayout.EndHorizontal();

                    // Відновлення кольору
                    GUI.color = originalColor;
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void Update()
        {
            // Оновлення статистики з частотою updateInterval
            if (_autoRefresh && Time.realtimeSinceStartup - _lastUpdateTime > _updateInterval)
            {
                RefreshStatistics();
                _lastUpdateTime = Time.realtimeSinceStartup;
                Repaint();
            }
        }

        private void RefreshStatistics()
        {
            // Отримання посилання на менеджер пулів
            if (_poolManager == null)
            {
                var gameBootstrapper = FindObjectOfType<MythHunter.Core.Game.GameBootstrapper>();
                if (gameBootstrapper != null)
                {
                    // Якщо ми в редакторі і є доступ до GameBootstrapper
                    _poolManager = gameBootstrapper.GetComponent<MythHunter.Core.Game.GameBootstrapper>()
                        .GetPoolManager();
                }
            }

            if (_poolManager != null)
            {
                _statistics = _poolManager.GetAllPoolsStatistics();
                _totalActiveObjects = _poolManager.GetTotalActiveObjects();
            }
            else
            {
                _statistics = new Dictionary<string, PoolStatistics>();
                _totalActiveObjects = 0;
            }
        }

        private void OnEnable()
        {
            _lastUpdateTime = Time.realtimeSinceStartup;
            RefreshStatistics();
        }
    }
}
#endif
