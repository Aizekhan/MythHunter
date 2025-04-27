using UnityEngine;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Core.DI;
using MythHunter.Core.Game;

public class PhaseSystemTester : MonoBehaviour
{
    private IEventBus _eventBus;
    private bool _isInitialized = false;

    private void Start()
    {
        // Отримуємо посилання на GameBootstrapper
        var bootstrapper = FindFirstObjectByType<GameBootstrapper>();
        if (bootstrapper == null)
        {
            Debug.LogError("GameBootstrapper not found on scene");
            return;
        }

        // Чекаємо трохи, щоб всі системи ініціалізувались
        Invoke("Initialize", 0.5f);
    }

    private void Initialize()
    {
        // Отримуємо посилання на GameBootstrapper
        var bootstrapper = FindFirstObjectByType<GameBootstrapper>();
        if (bootstrapper.Container == null)
        {
            Debug.LogError("Container is not initialized");
            return;
        }

        // Отримуємо EventBus з контейнера
        _eventBus = bootstrapper.Container.Resolve<IEventBus>();
        if (_eventBus == null)
        {
            Debug.LogError("EventBus is not registered in DI container");
            return;
        }

        _isInitialized = true;
        Debug.Log("PhaseSystemTester initialized successfully");

        // Публікуємо подію початку гри
        _eventBus.Publish(new GameStartedEvent
        {
            Timestamp = System.DateTime.UtcNow
        });
    }

    private void Update()
    {
        // Додаємо можливість швидше міняти фази за допомогою клавіші Space
        if (_isInitialized && Input.GetKeyDown(KeyCode.Space))
        {
            _eventBus.Publish(new RequestNextPhaseEvent
            {
                CurrentPhase = GamePhase.None,
                Timestamp = System.DateTime.UtcNow
            });
            Debug.Log("Manual phase change requested");
        }
    }
}
