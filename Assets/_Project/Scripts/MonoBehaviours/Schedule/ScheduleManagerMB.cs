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

            // NPCControllerにゲーム全体のカレンダーを共有し、章別台詞切替を連動させる
            NPCController.SharedCalendar = calendar;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
