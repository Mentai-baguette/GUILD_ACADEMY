using System.Collections.Generic;

namespace GuildAcademy.Core.Dialogue
{
    public class DialogueEntry
    {
        public string Id { get; set; }
        public string Speaker { get; set; }
        public string Text { get; set; }
        public string Next { get; set; }
        public List<DialogueChoice> Choices { get; set; }
    }

    public class DialogueChoice
    {
        public string Text { get; set; }
        public string Next { get; set; }
        public string Flag { get; set; }
        public Dictionary<string, int> TrustEffects { get; set; }
    }
}
