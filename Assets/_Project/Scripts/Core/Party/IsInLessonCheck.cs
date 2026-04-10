using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Party
{
    /// <summary>
    /// キャラクターが授業中かどうかを判定するデリゲート。
    /// ScheduleManagerとの連携に使用。
    /// </summary>
    public delegate bool IsInLessonCheck(CharacterStats member);
}
