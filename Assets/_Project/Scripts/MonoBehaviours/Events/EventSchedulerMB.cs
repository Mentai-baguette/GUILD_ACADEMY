using System;
using UnityEngine;
using GuildAcademy.Core.Calendar;
using GuildAcademy.Core.Events;

namespace GuildAcademy.MonoBehaviours.Events
{
    /// <summary>
    /// EventScheduler の MonoBehaviour ラッパー。
    /// CalendarManager.OnTimeAdvanced を購読し、時間帯が進むたびに次イベントを決定する。
    /// </summary>
    public class EventSchedulerMB : MonoBehaviour
    {
        private EventScheduler _scheduler;
        private CalendarManager _calendar;

        /// <summary>イベントが発火したときの通知</summary>
        public event Action<EventData> OnEventTriggered;

        /// <summary>
        /// 外部からスケジューラとカレンダーを注入して初期化する。
        /// </summary>
        public void Initialize(EventScheduler scheduler, CalendarManager calendar)
        {
            if (_calendar != null)
            {
                _calendar.OnTimeAdvanced -= HandleTimeAdvanced;
            }

            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _calendar = calendar ?? throw new ArgumentNullException(nameof(calendar));

            _calendar.OnTimeAdvanced += HandleTimeAdvanced;
        }

        private void OnDestroy()
        {
            if (_calendar != null)
            {
                _calendar.OnTimeAdvanced -= HandleTimeAdvanced;
            }
        }

        private void HandleTimeAdvanced(TimeOfDay time)
        {
            if (_scheduler == null || _calendar == null) return;

            var nextEvent = _scheduler.GetNextEvent(
                _calendar.CurrentWeek,
                _calendar.CurrentDay,
                time,
                _calendar.CurrentChapter
            );

            if (nextEvent != null)
            {
                OnEventTriggered?.Invoke(nextEvent);
            }
        }
    }
}
