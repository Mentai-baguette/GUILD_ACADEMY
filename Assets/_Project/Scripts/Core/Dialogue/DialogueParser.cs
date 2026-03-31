using System;
using System.Collections.Generic;

namespace GuildAcademy.Core.Dialogue
{
    public class DialogueParser
    {
        private readonly Dictionary<string, DialogueEntry> _entries = new Dictionary<string, DialogueEntry>();

        public void Load(List<DialogueEntry> entries)
        {
            _entries.Clear();
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Id))
                    throw new ArgumentException("DialogueEntry must have a non-empty Id");
                _entries[entry.Id] = entry;
            }
        }

        public DialogueEntry GetById(string id)
        {
            if (_entries.TryGetValue(id, out var entry))
                return entry;
            throw new KeyNotFoundException($"Dialogue entry not found: {id}");
        }

        public bool HasEntry(string id) => _entries.ContainsKey(id);
        public int EntryCount => _entries.Count;
    }
}
