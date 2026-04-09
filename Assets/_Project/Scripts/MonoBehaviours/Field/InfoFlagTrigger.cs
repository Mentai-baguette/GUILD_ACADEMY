using UnityEngine;
using GuildAcademy.Core.Events;
using GuildAcademy.MonoBehaviours.Branch;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class InfoFlagTrigger : MonoBehaviour
    {
        [SerializeField] private string _eventId;

        private InfoFlagEventRegistry _registry;

        public string EventId => _eventId;

        public void Initialize(InfoFlagEventRegistry registry)
        {
            _registry = registry;
        }

        public bool IsAvailable()
        {
            return _registry != null && _registry.IsEventAvailable(_eventId);
        }

        /// <summary>
        /// Call when NPC conversation completes. Sets the flag if event is available.
        /// Returns true if the flag was newly set.
        /// </summary>
        public bool TryComplete()
        {
            if (!IsAvailable()) return false;

            _registry.CompleteEvent(_eventId);
            return true;
        }
    }
}
