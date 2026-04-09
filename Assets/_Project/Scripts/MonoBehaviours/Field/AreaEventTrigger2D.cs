using UnityEngine;
using GuildAcademy.MonoBehaviours.Branch;
using GuildAcademy.MonoBehaviours.Events;

namespace GuildAcademy.MonoBehaviours.Field
{
    [RequireComponent(typeof(Collider2D))]
    public class AreaEventTrigger2D : MonoBehaviour
    {
        [SerializeField] private string _eventId = "event_default";
        [SerializeField] private bool _useInfoFlagEvent = true;
        [SerializeField] private string _targetFlagName;
        [SerializeField] private bool _targetFlagValue = true;
        [SerializeField] private string _playerTag = "Player";
        [SerializeField] private string _debugMessage = "Area event triggered";
        [SerializeField] private bool _oneShot = true;

        private bool _consumed;

        public string EventId => _eventId;
        public bool IsConsumed => _consumed;

        private void Reset()
        {
            var collider2D = GetComponent<Collider2D>();
            collider2D.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryTriggerByTag(other != null && other.CompareTag(_playerTag));
        }

        public bool TryTriggerByTag(bool isPlayer)
        {
            if (_oneShot && _consumed)
                return false;

            if (!isPlayer)
                return false;

            var handled = false;

            if (_useInfoFlagEvent && !string.IsNullOrWhiteSpace(_eventId))
            {
                var registry = InfoFlagEventManager.Instance?.Registry;
                if (registry != null)
                {
                    if (registry.IsEventAvailable(_eventId))
                    {
                        registry.CompleteEvent(_eventId);
                        handled = true;
                    }
                }
                else
                {
                    Debug.LogWarning("[AreaEventTrigger2D] InfoFlagEventManager is not initialized.", this);
                }
            }

            if (!handled && !string.IsNullOrWhiteSpace(_targetFlagName))
            {
                var branchManager = BranchManager.Instance;
                if (branchManager != null)
                {
                    try
                    {
                        branchManager.SetFlag(_targetFlagName, _targetFlagValue);
                        handled = true;
                    }
                    catch (System.ArgumentException ex)
                    {
                        Debug.LogError($"[AreaEventTrigger2D] Invalid target flag: {_targetFlagName}. {ex.Message}", this);
                    }
                }
                else
                {
                    Debug.LogWarning("[AreaEventTrigger2D] BranchManager is not initialized.", this);
                }
            }

            if (!handled)
                return false;

            _consumed = true;
            Debug.Log($"[AreaEventTrigger2D] EventId={_eventId}, Message={_debugMessage}", this);
            return true;
        }
    }
}