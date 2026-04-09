using System;
using System.Collections.Generic;
using GuildAcademy.Core.Calendar;

namespace GuildAcademy.Core.Schedule
{
    public enum ActivityType
    {
        Lesson,
        FreeTime,
        DungeonExploration,
        DormEvent,
        NightExploration,
        Sleep
    }

    public class ScheduleManager
    {
        private readonly CalendarManager _calendar;
        private int _todayLessonCount;

        public CalendarManager Calendar => _calendar;
        public int TodayLessonCount => _todayLessonCount;

        public event Action<ActivityType> OnActivityCompleted;

        public ScheduleManager(CalendarManager calendar)
        {
            _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));
        }

        public void SetTodayLessonCount(int count)
        {
            _todayLessonCount = Math.Clamp(count, 0, 5);
        }

        /// <summary>
        /// 現在の時間帯で利用可能なアクティビティのリストを返す。
        /// </summary>
        public List<ActivityType> GetAvailableActivities()
        {
            var activities = new List<ActivityType>();

            if (_calendar.IsFinished) return activities;

            switch (_calendar.CurrentTime)
            {
                case TimeOfDay.Morning:
                    if (_calendar.IsWeekday() && !_calendar.IsVacation())
                        activities.Add(ActivityType.Lesson);
                    else
                        activities.Add(ActivityType.FreeTime);
                    break;

                case TimeOfDay.Afternoon:
                    activities.Add(ActivityType.FreeTime);
                    if (CanExploreDungeon())
                        activities.Add(ActivityType.DungeonExploration);
                    activities.Add(ActivityType.DormEvent);
                    break;

                case TimeOfDay.Night:
                    activities.Add(ActivityType.NightExploration);
                    activities.Add(ActivityType.Sleep);
                    break;
            }

            return activities;
        }

        /// <summary>
        /// 放課後にダンジョン探索可能かどうか。
        /// 5コマ受けた日は探索不可。
        /// </summary>
        public bool CanExploreDungeon()
        {
            return _todayLessonCount < 5;
        }

        /// <summary>
        /// 放課後の探索可能層数を計算する。
        /// 0コマ=4層, 1-3コマ=3層, 4コマ=2層, 5コマ=0層
        /// </summary>
        public int CalculateDungeonLayers()
        {
            return CalculateDungeonLayers(_todayLessonCount);
        }

        /// <summary>
        /// 指定コマ数から探索可能層数を計算する。
        /// </summary>
        public int CalculateDungeonLayers(int lessonCount)
        {
            if (lessonCount >= 5) return 0;
            if (lessonCount >= 4) return 2;
            if (lessonCount >= 1) return 3;
            return 4; // 0コマ（休日・休暇）
        }

        /// <summary>
        /// 夜ダンジョンのボーナス層数を返す（常に+1）。
        /// </summary>
        public int GetNightBonusLayers()
        {
            return 1;
        }

        /// <summary>
        /// 夜ダンジョンの敵強化倍率を返す。
        /// </summary>
        public float GetNightEnemyMultiplier()
        {
            return 1.5f;
        }

        /// <summary>
        /// 現在の時間帯が夜かどうか。
        /// </summary>
        public bool IsNight()
        {
            return _calendar.CurrentTime == TimeOfDay.Night;
        }

        /// <summary>
        /// アクティビティ完了を通知し、必要に応じて時間を進める。
        /// </summary>
        public void CompleteActivity(ActivityType activity)
        {
            OnActivityCompleted?.Invoke(activity);

            switch (activity)
            {
                case ActivityType.Lesson:
                    // 授業完了後 → 放課後に進む
                    if (_calendar.CurrentTime == TimeOfDay.Morning)
                        _calendar.AdvanceTime();
                    break;

                case ActivityType.DungeonExploration:
                case ActivityType.FreeTime:
                case ActivityType.DormEvent:
                    // 放課後アクティビティ完了 → 夜に進む
                    if (_calendar.CurrentTime == TimeOfDay.Afternoon)
                        _calendar.AdvanceTime();
                    break;

                case ActivityType.NightExploration:
                case ActivityType.Sleep:
                    // 夜アクティビティ完了 → 翌日に進む
                    if (_calendar.CurrentTime == TimeOfDay.Night)
                    {
                        _calendar.AdvanceTime(); // Night → next day Morning
                        _todayLessonCount = 0; // 翌日のコマ数リセット
                    }
                    break;
            }
        }
    }
}
