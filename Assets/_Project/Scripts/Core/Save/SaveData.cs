using System;
using System.Collections.Generic;

namespace GuildAcademy.Core.Save
{
    [Serializable]
    public class SaveData
    {
        public string SaveId { get; set; }
        public string Timestamp { get; set; }
        public int PlayTimeSeconds { get; set; }

        // Flags
        public Dictionary<string, bool> Flags { get; set; } = new Dictionary<string, bool>();

        // Trust
        public Dictionary<string, int> Trust { get; set; } = new Dictionary<string, int>();

        // Erosion
        public int ErosionValue { get; set; }

        // Soul Link
        public Dictionary<string, int> BondPoints { get; set; } = new Dictionary<string, int>();

        // Progress
        public string CurrentScene { get; set; }
        public string CurrentDialogueId { get; set; }
        public int ChapterNumber { get; set; }
    }
}
