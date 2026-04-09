using System;
using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Branch;

namespace GuildAcademy.Core.Events
{
    public class InfoFlagEventRegistry
    {
        private readonly List<InfoFlagEventData> _events = new List<InfoFlagEventData>();
        private readonly FlagSystem _flagSystem;

        public event Action<InfoFlagEventData> OnFlagEventCompleted;

        public InfoFlagEventRegistry(FlagSystem flagSystem)
        {
            _flagSystem = flagSystem ?? throw new ArgumentNullException(nameof(flagSystem));
        }

        public void Register(InfoFlagEventData eventData)
        {
            if (eventData == null) throw new ArgumentNullException(nameof(eventData));
            if (_events.Any(e => e.EventId == eventData.EventId))
                throw new InvalidOperationException($"Event '{eventData.EventId}' is already registered.");
            _events.Add(eventData);
        }

        public IReadOnlyList<InfoFlagEventData> GetAllEvents() => _events.AsReadOnly();

        public IReadOnlyList<InfoFlagEventData> GetAvailableEvents()
        {
            return _events.Where(e =>
                !_flagSystem.Get(e.FlagName) &&
                e.Prerequisites.All(p => _flagSystem.Get(p))
            ).ToList().AsReadOnly();
        }

        public IReadOnlyList<InfoFlagEventData> GetCompletedEvents()
        {
            return _events.Where(e => _flagSystem.Get(e.FlagName)).ToList().AsReadOnly();
        }

        public bool IsEventAvailable(string eventId)
        {
            var evt = _events.FirstOrDefault(e => e.EventId == eventId);
            if (evt == null) return false;
            return !_flagSystem.Get(evt.FlagName) &&
                   evt.Prerequisites.All(p => _flagSystem.Get(p));
        }

        public void CompleteEvent(string eventId)
        {
            var evt = _events.FirstOrDefault(e => e.EventId == eventId);
            if (evt == null)
                throw new ArgumentException($"Unknown event: {eventId}");

            if (!IsEventAvailable(eventId))
                throw new InvalidOperationException(
                    $"Event '{eventId}' is not available (already completed or prerequisites unmet).");

            _flagSystem.Set(evt.FlagName, true);
            OnFlagEventCompleted?.Invoke(evt);
        }

        public int CompletedCount => _events.Count(e => _flagSystem.Get(e.FlagName));
        public int TotalCount => _events.Count;
    }
}
