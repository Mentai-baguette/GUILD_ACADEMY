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

        // --- 立ち絵制御用（任意） ---
        /// <summary>感情差分: "normal", "happy", "angry", "sad", "surprised"</summary>
        public string Emotion { get; set; } = "normal";
        /// <summary>立ち絵の表示位置: "left" or "right"（null=自動配置）</summary>
        public string PortraitPosition { get; set; }
    }

    public class DialogueChoice
    {
        public string Text { get; set; }
        public string Next { get; set; }
        public string Flag { get; set; }
        public Dictionary<string, int> TrustEffects { get; set; }
    }
}
