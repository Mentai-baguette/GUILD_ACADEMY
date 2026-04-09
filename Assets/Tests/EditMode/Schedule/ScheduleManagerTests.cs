using NUnit.Framework;
using GuildAcademy.Core.Calendar;
using GuildAcademy.Core.Schedule;

namespace GuildAcademy.Tests.EditMode.Schedule
{
    [TestFixture]
    public class ScheduleManagerTests
    {
        private CalendarManager _calendar;
        private ScheduleManager _schedule;

        [SetUp]
        public void SetUp()
        {
            _calendar = new CalendarManager();
            _schedule = new ScheduleManager(_calendar);
        }

        // --- CalculateDungeonLayers ---

        [Test]
        public void CalculateDungeonLayers_0Lessons_Returns4()
        {
            Assert.AreEqual(4, _schedule.CalculateDungeonLayers(0));
        }

        [Test]
        public void CalculateDungeonLayers_1Lesson_Returns3()
        {
            Assert.AreEqual(3, _schedule.CalculateDungeonLayers(1));
        }

        [Test]
        public void CalculateDungeonLayers_3Lessons_Returns3()
        {
            Assert.AreEqual(3, _schedule.CalculateDungeonLayers(3));
        }

        [Test]
        public void CalculateDungeonLayers_4Lessons_Returns2()
        {
            Assert.AreEqual(2, _schedule.CalculateDungeonLayers(4));
        }

        [Test]
        public void CalculateDungeonLayers_5Lessons_Returns0()
        {
            Assert.AreEqual(0, _schedule.CalculateDungeonLayers(5));
        }

        // --- CanExploreDungeon ---

        [Test]
        public void CanExploreDungeon_Under5Lessons_ReturnsTrue()
        {
            _schedule.SetTodayLessonCount(3);
            Assert.IsTrue(_schedule.CanExploreDungeon());
        }

        [Test]
        public void CanExploreDungeon_5Lessons_ReturnsFalse()
        {
            _schedule.SetTodayLessonCount(5);
            Assert.IsFalse(_schedule.CanExploreDungeon());
        }

        // --- Night ---

        [Test]
        public void GetNightBonusLayers_Returns1()
        {
            Assert.AreEqual(1, _schedule.GetNightBonusLayers());
        }

        [Test]
        public void GetNightEnemyMultiplier_Returns1Point5()
        {
            Assert.AreEqual(1.5f, _schedule.GetNightEnemyMultiplier());
        }

        [Test]
        public void IsNight_MorningTime_ReturnsFalse()
        {
            Assert.IsFalse(_schedule.IsNight());
        }

        [Test]
        public void IsNight_NightTime_ReturnsTrue()
        {
            _calendar.AdvanceTime(); // Morning -> Afternoon
            _calendar.AdvanceTime(); // Afternoon -> Night
            Assert.IsTrue(_schedule.IsNight());
        }

        // --- GetAvailableActivities ---

        [Test]
        public void GetAvailableActivities_WeekdayMorning_ContainsLesson()
        {
            // Week1, Monday, Morning (weekday)
            var activities = _schedule.GetAvailableActivities();
            Assert.Contains(ActivityType.Lesson, activities);
        }

        [Test]
        public void GetAvailableActivities_Afternoon_ContainsFreeTimeAndDungeon()
        {
            _calendar.AdvanceTime(); // -> Afternoon
            _schedule.SetTodayLessonCount(2);
            var activities = _schedule.GetAvailableActivities();
            Assert.Contains(ActivityType.FreeTime, activities);
            Assert.Contains(ActivityType.DungeonExploration, activities);
            Assert.Contains(ActivityType.DormEvent, activities);
        }

        [Test]
        public void GetAvailableActivities_Afternoon5Lessons_NoDungeon()
        {
            _calendar.AdvanceTime(); // -> Afternoon
            _schedule.SetTodayLessonCount(5);
            var activities = _schedule.GetAvailableActivities();
            Assert.IsFalse(activities.Contains(ActivityType.DungeonExploration));
        }

        [Test]
        public void GetAvailableActivities_Night_ContainsNightExplorationAndSleep()
        {
            _calendar.AdvanceTime(); // -> Afternoon
            _calendar.AdvanceTime(); // -> Night
            var activities = _schedule.GetAvailableActivities();
            Assert.Contains(ActivityType.NightExploration, activities);
            Assert.Contains(ActivityType.Sleep, activities);
        }

        // --- CompleteActivity ---

        [Test]
        public void CompleteActivity_Lesson_AdvancesToAfternoon()
        {
            _schedule.CompleteActivity(ActivityType.Lesson);
            Assert.AreEqual(TimeOfDay.Afternoon, _calendar.CurrentTime);
        }

        [Test]
        public void CompleteActivity_DungeonExploration_AdvancesToNight()
        {
            _calendar.AdvanceTime(); // -> Afternoon
            _schedule.CompleteActivity(ActivityType.DungeonExploration);
            Assert.AreEqual(TimeOfDay.Night, _calendar.CurrentTime);
        }

        [Test]
        public void CompleteActivity_Sleep_AdvancesToNextDayMorning()
        {
            _calendar.AdvanceTime(); // -> Afternoon
            _calendar.AdvanceTime(); // -> Night
            _schedule.SetTodayLessonCount(3);

            _schedule.CompleteActivity(ActivityType.Sleep);

            Assert.AreEqual(TimeOfDay.Morning, _calendar.CurrentTime);
            Assert.AreEqual(CalendarDay.Tuesday, _calendar.CurrentDay);
            Assert.AreEqual(0, _schedule.TodayLessonCount);
        }

        [Test]
        public void CompleteActivity_OnActivityCompleted_EventFires()
        {
            ActivityType firedActivity = default;
            _schedule.OnActivityCompleted += a => firedActivity = a;

            _schedule.CompleteActivity(ActivityType.Lesson);

            Assert.AreEqual(ActivityType.Lesson, firedActivity);
        }

        // --- SetTodayLessonCount ---

        [Test]
        public void SetTodayLessonCount_ClampsBetween0And5()
        {
            _schedule.SetTodayLessonCount(-1);
            Assert.AreEqual(0, _schedule.TodayLessonCount);

            _schedule.SetTodayLessonCount(10);
            Assert.AreEqual(5, _schedule.TodayLessonCount);
        }

        // --- Weekend / Vacation ---

        [Test]
        public void GetAvailableActivities_WeekendMorning_ContainsFreeTime()
        {
            // Advance to Saturday
            for (int i = 0; i < 5; i++)
                _calendar.AdvanceDay(); // Mon -> Tue -> Wed -> Thu -> Fri -> Sat

            var activities = _schedule.GetAvailableActivities();
            Assert.Contains(ActivityType.FreeTime, activities);
            Assert.IsFalse(activities.Contains(ActivityType.Lesson));
        }
    }
}
