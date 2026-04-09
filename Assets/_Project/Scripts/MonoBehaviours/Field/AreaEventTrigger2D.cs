using UnityEngine;

namespace GuildAcademy.MonoBehaviours.Field
{
    [RequireComponent(typeof(Collider2D))]
    public class AreaEventTrigger2D : MonoBehaviour
    {
        [SerializeField] private string _eventId = "event_default";
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private string _debugMessage = "Area event triggered";
        [SerializeField] private bool _oneShot = true;

        private bool _consumed;

        public string EventId => _eventId;

        private void Reset()
        {
            var collider2D = GetComponent<Collider2D>();
            collider2D.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_oneShot && _consumed)
                return;

            if (!other.CompareTag(_playerTag))
                return;

            _consumed = true;
            Debug.Log($"[AreaEventTrigger2D] EventId={_eventId}, Message={_debugMessage}", this);
        }
    }
}