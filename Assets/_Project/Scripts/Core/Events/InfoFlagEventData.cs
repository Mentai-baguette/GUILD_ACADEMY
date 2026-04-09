using System;
using System.Collections.Generic;

namespace GuildAcademy.Core.Events
{
    public class InfoFlagEventData
    {
        public string EventId { get; set; }
        public string FlagName { get; set; }
        public string DialogueEntryId { get; set; }
        public string Description { get; set; }
        public List<string> Prerequisites { get; set; }

        public InfoFlagEventData(string eventId, string flagName, string dialogueEntryId,
            string description = "", List<string> prerequisites = null)
        {
            EventId = eventId ?? throw new ArgumentNullException(nameof(eventId));
            FlagName = flagName ?? throw new ArgumentNullException(nameof(flagName));
            DialogueEntryId = dialogueEntryId ?? throw new ArgumentNullException(nameof(dialogueEntryId));
            Description = description ?? "";
            Prerequisites = prerequisites ?? new List<string>();
        }
    }
}
