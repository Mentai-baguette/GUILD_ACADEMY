using GuildAcademy.Core.Calendar;

namespace GuildAcademy.Core.Events
{
    /// <summary>
    /// イベントタイプ。優先度順: Forced > Main > SoulLink > Teacher > Random > Free
    /// </summary>
    public enum EventType
    {
        Forced,
        Main,
        SoulLink,
        Teacher,
        Random,
        Free
    }

    /// <summary>
    /// イベントデータ。スケジューラに登録して時間帯別に配置される。
    /// </summary>
    public class EventData
    {
        public string EventId { get; set; }
        public EventType Type { get; set; }
        public int Chapter { get; set; }
        public int Week { get; set; }
        public CalendarDay? Day { get; set; }
        public TimeOfDay TimeSlot { get; set; }
        public string[] PrerequisiteFlags { get; set; }
        public string TargetCharacterId { get; set; }
        public int RequiredTrust { get; set; }
        public string DialogueKey { get; set; }
        public string Location { get; set; }
        public bool IsRepeatable { get; set; }

        public EventData()
        {
            PrerequisiteFlags = System.Array.Empty<string>();
        }
    }
}
