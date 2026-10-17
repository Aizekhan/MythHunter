// Assets/_MythHunter/Code/Core/MonoBehaviours/MovementView.cs
using UnityEngine;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Components.Movement;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.MonoBehaviours
{
    /// <summary>
    /// MonoBehaviour для відображення руху сутності
    /// </summary>
    public class MovementView : LazyMonoBehaviour, IEventSubscriber
    {
        [SerializeField] private int _entityId;
        [SerializeField] private Transform _transform;
        [SerializeField] private Transform _lookDirectionIndicator;

        [Inject] private IMythLogger _logger;
        [Inject] private IEventBus _eventBus;

        private bool _isSubscribed = false;

        protected override void OnInject()
        {
            base.OnInject();

            if (_transform == null)
                _transform = transform;

            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        public void SetEntityId(int entityId)
        {
            _entityId = entityId;
        }

        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<PositionUpdatedEvent>(OnPositionUpdated);
            _eventBus.Subscribe<LookDirectionChangedEvent>(OnLookDirectionChanged);

            _isSubscribed = true;
        }

        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<PositionUpdatedEvent>(OnPositionUpdated);
            _eventBus.Unsubscribe<LookDirectionChangedEvent>(OnLookDirectionChanged);

            _isSubscribed = false;
        }

        private void OnPositionUpdated(PositionUpdatedEvent evt)
        {
            // Обробляємо подію лише для відповідної сутності
            if (evt.EntityId != _entityId)
                return;

            // Оновлюємо позицію та обертання
            _transform.position = evt.Position;
            _transform.rotation = evt.Rotation;
        }

        private void OnLookDirectionChanged(LookDirectionChangedEvent evt)
        {
            // Обробляємо подію лише для відповідної сутності
            if (evt.EntityId != _entityId)
                return;

            // Оновлюємо індикатор напрямку, якщо він є
            if (_lookDirectionIndicator != null)
            {
                _lookDirectionIndicator.forward = evt.Direction;
            }
        }
    }
}
