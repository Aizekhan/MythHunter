// DebugPanel.cs
using System.Collections.Generic;
using UnityEngine;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Utils.Logging;

namespace MythHunter.Debug.UI
{
    /// <summary>
    /// Панель відлагодження для розробників
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        [SerializeField] private bool _showOnStart = false;
        [SerializeField] private KeyCode _toggleKey = KeyCode.F9;

        private bool _isVisible = false;
        private Vector2 _scrollPosition;
        private readonly Dictionary<string, bool> _sectionFoldouts = new Dictionary<string, bool>();

        // Сервіси, які будуть ін'єктовані через DI
        private IEntityManager _entityManager;
        private IEventBus _eventBus;
        private IMythLogger _logger;

        private int _selectedTab = 0;
        private readonly string[] _tabNames = { "Системи", "Сутності", "Події", "Мережа", "Профілювання" };

        // Стан для відображення
        private readonly List<string> _recentEvents = new List<string>();
        private readonly Dictionary<string, int> _systemUpdateTimes = new Dictionary<string, int>();
        private readonly Dictionary<int, string> _entityDetails = new Dictionary<int, string>();

        [MythHunter.Core.DI.Inject]
        public void Construct(IEntityManager entityManager, IEventBus eventBus, IMythLogger logger)
        {
            _entityManager = entityManager;
            _eventBus = eventBus;
            _logger = logger;

            // Підписуємось на події для логування
            SubscribeToEvents();
        }

        private void Start()
        {
            _isVisible = _showOnStart;
        }

        private void OnDestroy()
        {
            // Відписуємось від подій
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                _isVisible = !_isVisible;
            }
        }

        private void OnGUI()
        {
            if (!_isVisible)
                return;

            // Налаштування стилів
            InitializeStyles();

            // Панель відлагодження
            GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, Screen.height - 20));

            GUILayout.BeginVertical("box");
            GUILayout.Label("MythHunter Debug Panel", GUI.skin.box);

            // Вкладки
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(100)))
            {
                RefreshDebugData();
            }
            if (GUILayout.Button("Close", GUILayout.Width(100)))
            {
                _isVisible = false;
            }
            GUILayout.EndHorizontal();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            // Відображаємо вкладку
            switch (_selectedTab)
            {
                case 0:
                    DrawSystemsTab();
                    break;
                case 1:
                    DrawEntitiesTab();
                    break;
                case 2:
                    DrawEventsTab();
                    break;
                case 3:
                    DrawNetworkTab();
                    break;
                case 4:
                    DrawProfilingTab();
                    break;
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawSystemsTab()
        {
            GUILayout.Label("Активні системи:", GUI.skin.box);

            // Тут буде виведення інформації про системи
            // Через рефлексію можна отримати список всіх систем
            // або можна додати колбек з SystemRegistry
        }

        private void DrawEntitiesTab()
        {
            GUILayout.Label("Сутності:", GUI.skin.box);

            // Список всіх сутностей
            if (_entityManager != null)
            {
                int[] entities = _entityManager.GetAllEntities();
                GUILayout.Label($"Всього сутностей: {entities.Length}");

                foreach (int entityId in entities)
                {
                    if (GUILayout.Button($"Entity {entityId}"))
                    {
                        // Отримання деталей про сутність
                        if (_entityDetails.TryGetValue(entityId, out var details))
                        {
                            _entityDetails.Remove(entityId);
                        }
                        else
                        {
                            _entityDetails[entityId] = GetEntityDetails(entityId);
                        }
                    }

                    if (_entityDetails.TryGetValue(entityId, out var entityDetails))
                    {
                        GUILayout.TextArea(entityDetails);
                    }
                }
            }
            else
            {
                GUILayout.Label("EntityManager не доступний");
            }
        }

        private void DrawEventsTab()
        {
            GUILayout.Label("Останні події:", GUI.skin.box);

            foreach (var eventInfo in _recentEvents)
            {
                GUILayout.Label(eventInfo);
            }

            if (GUILayout.Button("Очистити"))
            {
                _recentEvents.Clear();
            }
        }

        private void DrawNetworkTab()
        {
            GUILayout.Label("Мережевий статус:", GUI.skin.box);

            // Тут буде інформація про мережеві з'єднання, метрики і т.д.
        }

        private void DrawProfilingTab()
        {
            GUILayout.Label("Профілювання:", GUI.skin.box);

            foreach (var kvp in _systemUpdateTimes)
            {
                GUILayout.Label($"{kvp.Key}: {kvp.Value} мс");
            }
        }

        private void InitializeStyles()
        {
            // Налаштування стилів GUI
        }

        private void RefreshDebugData()
        {
            // Оновлення даних для відображення
        }

        private string GetEntityDetails(int entityId)
        {
            string result = $"Entity ID: {entityId}\n";

            // Використовуємо рефлексію для отримання всіх компонентів
            // Це приклад, який потрібно розвинути для реального використання

            return result;
        }

        private void SubscribeToEvents()
        {
            if (_eventBus != null)
            {
                // Підписка на події через рефлексію або вручну
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_eventBus != null)
            {
                // Відписка від подій
            }
        }

        // Метод для додавання події у список останніх
        private void AddEventToLog(string eventName, string eventId)
        {
            _recentEvents.Insert(0, $"{System.DateTime.Now.ToString("HH:mm:ss.fff")} - {eventName} ({eventId})");
            if (_recentEvents.Count > 100)
            {
                _recentEvents.RemoveAt(_recentEvents.Count - 1);
            }
        }
    }
}
