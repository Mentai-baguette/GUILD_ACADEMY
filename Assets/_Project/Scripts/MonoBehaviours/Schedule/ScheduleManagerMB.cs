using UnityEngine;
using GuildAcademy.Core.Calendar;
using GuildAcademy.Core.Schedule;
using GuildAcademy.MonoBehaviours.Field;

namespace GuildAcademy.MonoBehaviours.Schedule
{
    public class ScheduleManagerMB : MonoBehaviour
    {
        public static ScheduleManagerMB Instance { get; private set; }

        public ScheduleManager Schedule { get; private set; }
        public CalendarManager Calendar => Schedule?.Calendar;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            var calendar = new CalendarManager();
            Schedule = new ScheduleManager(calendar);

            // NPCControllerの章切替にCalendarManagerを共有
            NPCController.SharedCalendar = calendar;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
