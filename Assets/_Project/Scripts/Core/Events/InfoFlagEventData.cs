using System;
using System.Collections.Generic;

namespace GuildAcademy.Core.Events
{
    public class InfoFlagEventData
    {
        public string EventId { get; }
        public string FlagName { get; }
        public string DialogueEntryId { get; }
        public string Description { get; }
        public IReadOnlyList<string> Prerequisites { get; }

        public InfoFlagEventData(string eventId, string flagName, string dialogueEntryId,
            string description = "", List<string> prerequisites = null)
        {
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            FlagName = flagName ?? throw new ArgumentNullException(nameof(flagName));
            DialogueEntryId = dialogueEntryId ?? throw new ArgumentNullException(nameof(dialogueEntryId));
            Description = description ?? "";
            Prerequisites = new List<string>(prerequisites ?? new List<string>()).AsReadOnly();
        }
    }
}
