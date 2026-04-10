using UnityEngine;
using GuildAcademy.MonoBehaviours.Events;

namespace GuildAcademy.MonoBehaviours.Field
{
    [RequireComponent(typeof(Collider2D))]
    public class AreaEventTrigger2D : MonoBehaviour
    {
        [SerializeField] private string _eventId = "";
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private string _debugMessage = "Event triggered";
        [SerializeField] private bool _oneShot = true;

        private bool _triggered;

        public string EventId => _eventId;
        public string DebugMessage => _debugMessage;
        public bool OneShot => _oneShot;

        private void Reset()
        {
            var collider2D = GetComponent<Collider2D>();
            if (collider2D != null)
                collider2D.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_triggered) return;
            if (!other.CompareTag(_playerTag)) return;

            if (!string.IsNullOrWhiteSpace(_debugMessage))
                Debug.Log($"[AreaEventTrigger2D] {_debugMessage}");

            var manager = InfoFlagEventManager.Instance;
            if (manager != null && manager.Registry != null && !string.IsNullOrWhiteSpace(_eventId))
            {
                if (manager.Registry.IsEventAvailable(_eventId))
                    manager.Registry.CompleteEvent(_eventId);
            }

            if (_oneShot)
                _triggered = true;
        }

        public void ResetTrigger()
        {
            _triggered = false;
        }
    }
}
