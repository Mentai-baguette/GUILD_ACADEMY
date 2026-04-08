using NUnit.Framework;
using GuildAcademy.Core.Calendar;

namespace GuildAcademy.Tests.EditMode.Calendar
{
    [TestFixture]
    public class CalendarManagerTests
    {
        private CalendarManager _calendar;

        [SetUp]
        public void SetUp()
        {
            _calendar = new CalendarManager();
        }

        // ===== 初期状態テスト =====

        [Test]
        public void InitialState_Week1MondayMorningChapter1()
        {
            Assert.AreEqual(1, _calendar.CurrentWeek);
            Assert.AreEqual(DayOfWeek.Monday, _calendar.CurrentDay);
            Assert.AreEqual(TimeOfDay.Morning, _calendar.CurrentTime);
            Assert.AreEqual(Chapter.Chapter1, _calendar.CurrentChapter);
            Assert.AreEqual(VacationType.None, _calendar.CurrentVacation);
            Assert.IsFalse(_calendar.IsFinished);
        }

        // ===== 時間帯進行テスト =====

        [Test]
        public void AdvanceTime_MorningToAfternoon()
        {
            _calendar.AdvanceTime();
            Assert.AreEqual(TimeOfDay.Afternoon, _calendar.CurrentTime);
            Assert.AreEqual(DayOfWeek.Monday, _calendar.CurrentDay);
        }

        [Test]
        public void AdvanceTime_AfternoonToNight()
        {
            _calendar.AdvanceTime(); // Morning -> Afternoon
            _calendar.AdvanceTime(); // Afternoon -> Night
            Assert.AreEqual(TimeOfDay.Night, _calendar.CurrentTime);
            Assert.AreEqual(DayOfWeek.Monday, _calendar.CurrentDay);
        }

        [Test]
        public void AdvanceTime_NightToNextDayMorning()
        {
            _calendar.AdvanceTime(); // Morning -> Afternoon
            _calendar.AdvanceTime(); // Afternoon -> Night
            _calendar.AdvanceTime(); // Night -> Next day Morning
            Assert.AreEqual(TimeOfDay.Morning, _calendar.CurrentTime);
            Assert.AreEqual(DayOfWeek.Tuesday, _calendar.CurrentDay);
        }

        [Test]
        public void AdvanceTime_FiresEvent()
        {
            TimeOfDay received = TimeOfDay.Morning;
            _calendar.OnTimeAdvanced += t => received = t;

            _calendar.AdvanceTime();
            Assert.AreEqual(TimeOfDay.Afternoon, received);
        }

        // ===== 日進行テスト =====

        [Test]
        public void AdvanceDay_MondayToTuesday()
        {
            _calendar.AdvanceDay();
            Assert.AreEqual(DayOfWeek.Tuesday, _calendar.CurrentDay);
            Assert.AreEqual(TimeOfDay.Morning, _calendar.CurrentTime);
        }

        [Test]
        public void AdvanceDay_SaturdayToSunday()
        {
            // 月→火→水→木→金→土
            for (int i = 0; i < 5; i++)
                _calendar.AdvanceDay();

            Assert.AreEqual(DayOfWeek.Saturday, _calendar.CurrentDay);

            _calendar.AdvanceDay();
            Assert.AreEqual(DayOfWeek.Sunday, _calendar.CurrentDay);
        }

        [Test]
        public void AdvanceDay_SundayAdvancesWeek()
        {
            // 月～日 = 6回AdvanceDay
            for (int i = 0; i < 6; i++)
                _calendar.AdvanceDay();

            Assert.AreEqual(DayOfWeek.Sunday, _calendar.CurrentDay);

            _calendar.AdvanceDay(); // Sunday -> next week Monday
            Assert.AreEqual(DayOfWeek.Monday, _calendar.CurrentDay);
            Assert.AreEqual(2, _calendar.CurrentWeek);
        }

        [Test]
        public void AdvanceDay_ResetsTimeToMorning()
        {
            _calendar.AdvanceTime(); // Morning -> Afternoon
            _calendar.AdvanceDay();
            Assert.AreEqual(TimeOfDay.Morning, _calendar.CurrentTime);
        }

        [Test]
        public void AdvanceDay_FiresEvent()
        {
            DayOfWeek received = DayOfWeek.Monday;
            _calendar.OnDayAdvanced += d => received = d;

            _calendar.AdvanceDay();
            Assert.AreEqual(DayOfWeek.Tuesday, received);
        }

        // ===== 週進行テスト =====

        [Test]
        public void AdvanceWeek_Week1ToWeek2()
        {
            _calendar.AdvanceWeek();
            Assert.AreEqual(2, _calendar.CurrentWeek);
            Assert.AreEqual(DayOfWeek.Monday, _calendar.CurrentDay);
            Assert.AreEqual(TimeOfDay.Morning, _calendar.CurrentTime);
        }

        [Test]
        public void AdvanceWeek_FiresEvent()
        {
            int received = 0;
            _calendar.OnWeekAdvanced += w => received = w;

            _calendar.AdvanceWeek();
            Assert.AreEqual(2, received);
        }

        [Test]
        public void AdvanceWeek_AtMaxWeek_SetsFinished()
        {
            // Advance to week 57
            for (int i = 1; i < CalendarManager.MAX_WEEK; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(57, _calendar.CurrentWeek);
            Assert.IsFalse(_calendar.IsFinished);

            _calendar.AdvanceWeek(); // Try to go past 57
            Assert.IsTrue(_calendar.IsFinished);
            Assert.AreEqual(57, _calendar.CurrentWeek); // stays at 57
        }

        [Test]
        public void AdvanceTime_WhenFinished_DoesNothing()
        {
            // Go to finished state
            for (int i = 1; i < CalendarManager.MAX_WEEK; i++)
                _calendar.AdvanceWeek();
            _calendar.AdvanceWeek();

            Assert.IsTrue(_calendar.IsFinished);

            _calendar.AdvanceTime();
            Assert.AreEqual(TimeOfDay.Morning, _calendar.CurrentTime);
        }

        // ===== 章判定テスト =====

        [Test]
        public void Chapter_Week1To12_IsChapter1()
        {
            for (int i = 1; i <= 12; i++)
            {
                Assert.AreEqual(Chapter.Chapter1, _calendar.CurrentChapter,
                    $"Week {_calendar.CurrentWeek} should be Chapter1");
                if (i < 12) _calendar.AdvanceWeek();
            }
        }

        [Test]
        public void Chapter_Week13To14_IsChapter1_ButVacation()
        {
            // Week 13-14 are Summer1 vacation; chapter stays as Chapter1
            for (int i = 1; i < 13; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(13, _calendar.CurrentWeek);
            Assert.AreEqual(Chapter.Chapter1, _calendar.CurrentChapter);
            Assert.AreEqual(VacationType.Summer1, _calendar.CurrentVacation);
        }

        [Test]
        public void Chapter_Week15To26_IsChapter2()
        {
            for (int i = 1; i < 15; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(15, _calendar.CurrentWeek);
            Assert.AreEqual(Chapter.Chapter2, _calendar.CurrentChapter);

            for (int i = 15; i < 26; i++)
            {
                _calendar.AdvanceWeek();
                Assert.AreEqual(Chapter.Chapter2, _calendar.CurrentChapter,
                    $"Week {_calendar.CurrentWeek} should be Chapter2");
            }
        }

        [Test]
        public void Chapter_Week30To41_IsChapter3()
        {
            for (int i = 1; i < 30; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(30, _calendar.CurrentWeek);
            Assert.AreEqual(Chapter.Chapter3, _calendar.CurrentChapter);
        }

        [Test]
        public void Chapter_Week44To55_IsChapter4()
        {
            for (int i = 1; i < 44; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(44, _calendar.CurrentWeek);
            Assert.AreEqual(Chapter.Chapter4, _calendar.CurrentChapter);
        }

        [Test]
        public void Chapter_Week56To57_IsChapter4()
        {
            for (int i = 1; i < 56; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(56, _calendar.CurrentWeek);
            Assert.AreEqual(Chapter.Chapter4, _calendar.CurrentChapter);

            _calendar.AdvanceWeek();
            Assert.AreEqual(57, _calendar.CurrentWeek);
            Assert.AreEqual(Chapter.Chapter4, _calendar.CurrentChapter);
        }

        [Test]
        public void ChapterChanged_FiresOnTransition()
        {
            Chapter received = Chapter.Chapter1;
            _calendar.OnChapterChanged += c => received = c;

            // Advance to week 15 (Chapter2 start)
            for (int i = 1; i < 15; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(Chapter.Chapter2, received);
        }

        // ===== 休暇判定テスト =====

        [Test]
        public void Vacation_Week13To14_IsSummer1()
        {
            for (int i = 1; i < 13; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(VacationType.Summer1, _calendar.CurrentVacation);
            Assert.IsTrue(_calendar.IsVacation());

            _calendar.AdvanceWeek();
            Assert.AreEqual(14, _calendar.CurrentWeek);
            Assert.AreEqual(VacationType.Summer1, _calendar.CurrentVacation);
        }

        [Test]
        public void Vacation_Week27To29_IsWinter()
        {
            for (int i = 1; i < 27; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(27, _calendar.CurrentWeek);
            Assert.AreEqual(VacationType.Winter, _calendar.CurrentVacation);
            Assert.IsTrue(_calendar.IsVacation());

            _calendar.AdvanceWeek();
            Assert.AreEqual(VacationType.Winter, _calendar.CurrentVacation);

            _calendar.AdvanceWeek();
            Assert.AreEqual(29, _calendar.CurrentWeek);
            Assert.AreEqual(VacationType.Winter, _calendar.CurrentVacation);
        }

        [Test]
        public void Vacation_Week42To43_IsSummer2()
        {
            for (int i = 1; i < 42; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(42, _calendar.CurrentWeek);
            Assert.AreEqual(VacationType.Summer2, _calendar.CurrentVacation);

            _calendar.AdvanceWeek();
            Assert.AreEqual(43, _calendar.CurrentWeek);
            Assert.AreEqual(VacationType.Summer2, _calendar.CurrentVacation);
        }

        [Test]
        public void Vacation_NonVacationWeek_IsNone()
        {
            Assert.AreEqual(VacationType.None, _calendar.CurrentVacation);
            Assert.IsFalse(_calendar.IsVacation());
        }

        [Test]
        public void VacationChanged_FiresOnTransition()
        {
            VacationType received = VacationType.None;
            _calendar.OnVacationChanged += v => received = v;

            for (int i = 1; i < 13; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(VacationType.Summer1, received);
        }

        // ===== 平日/休日判定テスト =====

        [Test]
        public void IsWeekday_MondayToFriday_ReturnsTrue()
        {
            // Monday
            Assert.IsTrue(_calendar.IsWeekday());

            for (int i = 0; i < 4; i++)
            {
                _calendar.AdvanceDay();
                Assert.IsTrue(_calendar.IsWeekday(),
                    $"{_calendar.CurrentDay} should be weekday");
            }
        }

        [Test]
        public void IsWeekend_SaturdayAndSunday_ReturnsTrue()
        {
            // Advance to Saturday
            for (int i = 0; i < 5; i++)
                _calendar.AdvanceDay();

            Assert.AreEqual(DayOfWeek.Saturday, _calendar.CurrentDay);
            Assert.IsTrue(_calendar.IsWeekend());
            Assert.IsFalse(_calendar.IsWeekday());

            _calendar.AdvanceDay(); // Sunday
            Assert.AreEqual(DayOfWeek.Sunday, _calendar.CurrentDay);
            Assert.IsTrue(_calendar.IsWeekend());
            Assert.IsFalse(_calendar.IsWeekday());
        }

        // ===== 章末判定テスト =====

        [Test]
        public void IsChapterEnd_Week12_ReturnsTrue()
        {
            for (int i = 1; i < 12; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(12, _calendar.CurrentWeek);
            Assert.IsTrue(_calendar.IsChapterEnd());
        }

        [Test]
        public void IsChapterEnd_Week26_ReturnsTrue()
        {
            for (int i = 1; i < 26; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(26, _calendar.CurrentWeek);
            Assert.IsTrue(_calendar.IsChapterEnd());
        }

        [Test]
        public void IsChapterEnd_Week41_ReturnsTrue()
        {
            for (int i = 1; i < 41; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(41, _calendar.CurrentWeek);
            Assert.IsTrue(_calendar.IsChapterEnd());
        }

        [Test]
        public void IsChapterEnd_Week55_ReturnsTrue()
        {
            for (int i = 1; i < 55; i++)
                _calendar.AdvanceWeek();

            Assert.AreEqual(55, _calendar.CurrentWeek);
            Assert.IsTrue(_calendar.IsChapterEnd());
        }

        [Test]
        public void IsChapterEnd_NonEndWeek_ReturnsFalse()
        {
            Assert.AreEqual(1, _calendar.CurrentWeek);
            Assert.IsFalse(_calendar.IsChapterEnd());
        }

        // ===== 57週全進行テスト =====

        [Test]
        public void FullProgression_57Weeks_AllStatesCorrect()
        {
            // Week 1 already set
            for (int week = 1; week <= 57; week++)
            {
                Assert.AreEqual(week, _calendar.CurrentWeek, $"Expected week {week}");

                // 章の検証
                if (week <= 14)
                    Assert.AreEqual(Chapter.Chapter1, _calendar.CurrentChapter, $"Week {week}");
                else if (week <= 29)
                    Assert.AreEqual(Chapter.Chapter2, _calendar.CurrentChapter, $"Week {week}");
                else if (week <= 43)
                    Assert.AreEqual(Chapter.Chapter3, _calendar.CurrentChapter, $"Week {week}");
                else
                    Assert.AreEqual(Chapter.Chapter4, _calendar.CurrentChapter, $"Week {week}");

                // 休暇の検証
                if (week >= 13 && week <= 14)
                    Assert.AreEqual(VacationType.Summer1, _calendar.CurrentVacation, $"Week {week}");
                else if (week >= 27 && week <= 29)
                    Assert.AreEqual(VacationType.Winter, _calendar.CurrentVacation, $"Week {week}");
                else if (week >= 42 && week <= 43)
                    Assert.AreEqual(VacationType.Summer2, _calendar.CurrentVacation, $"Week {week}");
                else
                    Assert.AreEqual(VacationType.None, _calendar.CurrentVacation, $"Week {week}");

                if (week < 57)
                    _calendar.AdvanceWeek();
            }

            Assert.IsFalse(_calendar.IsFinished);
            _calendar.AdvanceWeek();
            Assert.IsTrue(_calendar.IsFinished);
        }

        [Test]
        public void FullDayProgression_OneWeek_CorrectDaySequence()
        {
            DayOfWeek[] expected = {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday,
                DayOfWeek.Sunday
            };

            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i], _calendar.CurrentDay, $"Day index {i}");
                if (i < expected.Length - 1)
                    _calendar.AdvanceDay();
            }

            // Sunday -> AdvanceDay should move to next week Monday
            _calendar.AdvanceDay();
            Assert.AreEqual(DayOfWeek.Monday, _calendar.CurrentDay);
            Assert.AreEqual(2, _calendar.CurrentWeek);
        }

        [Test]
        public void FullTimeProgression_OneDay_CorrectTimeSequence()
        {
            Assert.AreEqual(TimeOfDay.Morning, _calendar.CurrentTime);
            _calendar.AdvanceTime();
            Assert.AreEqual(TimeOfDay.Afternoon, _calendar.CurrentTime);
            _calendar.AdvanceTime();
            Assert.AreEqual(TimeOfDay.Night, _calendar.CurrentTime);
            _calendar.AdvanceTime(); // wraps to next day morning
            Assert.AreEqual(TimeOfDay.Morning, _calendar.CurrentTime);
            Assert.AreEqual(DayOfWeek.Tuesday, _calendar.CurrentDay);
        }
    }
}
