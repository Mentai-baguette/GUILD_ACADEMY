namespace GuildAcademy.Core.Dialogue
{
    /// <summary>
    /// 会話1行分のデータ（誰が何を言うか）
    /// </summary>
    public class DialogueLine
    {
        public string SpeakerName { get; } // 話者名（例: "レイ"）
        public string Message { get; }     // セリフ内容（例: "おはよう"）

        public DialogueLine(string speakerName, string message)
        {
            SpeakerName = speakerName;
            Message = message;
        }
    }
}
