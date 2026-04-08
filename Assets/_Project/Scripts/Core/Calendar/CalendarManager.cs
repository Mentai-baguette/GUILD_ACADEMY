using System;

namespace GuildAcademy.Core.Calendar
{
    public enum Chapter { Chapter1, Chapter2, Chapter3, Chapter4 }
    public enum DayOfWeek { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday }
    public enum TimeOfDay { Morning, Afternoon, Night }
    public enum VacationType { None, Summer1, Winter, Summer2 }

    /// <summary>
    /// 2年制4章×12週+長期休暇=57週のカレンダー管理。
    /// Week 1-12: 1章, Week 13-14: 夏休み1, Week 15-26: 2章,
    /// Week 27-29: 冬休み, Week 30-41: 3章, Week 42-43: 夏休み2,
    /// Week 44-55: 4章, Week 56-57: 予備/エンディング期間。
    /// </summary>
    public class CalendarManager
    {
        public const int MAX_WEEK = 57;

        // 章の週範囲
        private const int CHAPTER1_START = 1;
        private const int CHAPTER1_END = 12;
        private const int SUMMER1_START = 13;
        private const int SUMMER1_END = 14;
        private const int CHAPTER2_START = 15;
        private const int CHAPTER2_END = 26;
        private const int WINTER_START = 27;
        private const int WINTER_END = 29;
        private const int CHAPTER3_START = 30;
        private const int CHAPTER3_END = 41;
        private const int SUMMER2_START = 42;
        private const int SUMMER2_END = 43;
        private const int CHAPTER4_START = 44;
        private const int CHAPTER4_END = 55;
        // Week 56-57: 予備/エンディング期間（Chapter4扱い）

        /// <summary>現在の週 (1-57)</summary>
        public int CurrentWeek { get; private set; }

        /// <summary>現在の曜日</summary>
        public DayOfWeek CurrentDay { get; private set; }

        /// <summary>現在の時間帯</summary>
        public TimeOfDay CurrentTime { get; private set; }

        /// <summary>現在の章</summary>
        public Chapter CurrentChapter { get; private set; }

        /// <summary>現在の長期休暇種別（休暇中でなければNone）</summary>
        public VacationType CurrentVacation { get; private set; }

        /// <summary>カレンダーが最終週を超えて終了したかどうか</summary>
        public bool IsFinished { get; private set; }

        /// <summary>時間帯が進行したときに発火するイベント</summary>
        public event Action<TimeOfDay> OnTimeAdvanced;

        /// <summary>日が進行したときに発火するイベント</summary>
        public event Action<DayOfWeek> OnDayAdvanced;

        /// <summary>週が進行したときに発火するイベント</summary>
        public event Action<int> OnWeekAdvanced;

        /// <summary>章が切り替わったときに発火するイベント</summary>
        public event Action<Chapter> OnChapterChanged;

        /// <summary>休暇状態が変わったときに発火するイベント</summary>
        public event Action<VacationType> OnVacationChanged;

        public CalendarManager()
        {
            CurrentWeek = 1;
            CurrentDay = DayOfWeek.Monday;
            CurrentTime = TimeOfDay.Morning;
            CurrentChapter = DetermineChapter(CurrentWeek);
            CurrentVacation = DetermineVacation(CurrentWeek);
            IsFinished = false;
        }

        /// <summary>
        /// 時間帯を1つ進める。朝→放課後→夜→翌日朝。
        /// 夜から進めると翌日に遷移する。
        /// </summary>
        public void AdvanceTime()
        {
            if (IsFinished) return;

            switch (CurrentTime)
            {
                case TimeOfDay.Morning:
                    CurrentTime = TimeOfDay.Afternoon;
                    OnTimeAdvanced?.Invoke(CurrentTime);
                    break;
                case TimeOfDay.Afternoon:
                    CurrentTime = TimeOfDay.Night;
                    OnTimeAdvanced?.Invoke(CurrentTime);
                    break;
                case TimeOfDay.Night:
                    AdvanceDay();
                    break;
            }
        }

        /// <summary>
        /// 翌日に進める。時間帯は朝にリセットされる。
        /// 日曜から進めると翌週月曜に遷移する。
        /// </summary>
        public void AdvanceDay()
        {
            if (IsFinished) return;

            if (CurrentDay == DayOfWeek.Sunday)
            {
                AdvanceWeek();
                return;
            }

            CurrentDay = CurrentDay + 1;
            CurrentTime = TimeOfDay.Morning;
            OnTimeAdvanced?.Invoke(CurrentTime);
            OnDayAdvanced?.Invoke(CurrentDay);
        }

        /// <summary>
        /// 翌週の月曜朝に進める。最終週を超えるとIsFinished=trueになる。
        /// </summary>
        public void AdvanceWeek()
        {
            if (IsFinished) return;

            if (CurrentWeek >= MAX_WEEK)
            {
                IsFinished = true;
                return;
            }

            var previousChapter = CurrentChapter;
            var previousVacation = CurrentVacation;

            CurrentWeek++;
            CurrentDay = DayOfWeek.Monday;
            CurrentTime = TimeOfDay.Morning;
            CurrentChapter = DetermineChapter(CurrentWeek);
            CurrentVacation = DetermineVacation(CurrentWeek);

            OnTimeAdvanced?.Invoke(CurrentTime);
            OnDayAdvanced?.Invoke(CurrentDay);
            OnWeekAdvanced?.Invoke(CurrentWeek);

            if (CurrentChapter != previousChapter)
            {
                OnChapterChanged?.Invoke(CurrentChapter);
            }
            if (CurrentVacation != previousVacation)
            {
                OnVacationChanged?.Invoke(CurrentVacation);
            }
        }

        /// <summary>平日（月～金）かどうか</summary>
        public bool IsWeekday()
        {
            return CurrentDay >= DayOfWeek.Monday && CurrentDay <= DayOfWeek.Friday;
        }

        /// <summary>土日かどうか</summary>
        public bool IsWeekend()
        {
            return CurrentDay == DayOfWeek.Saturday || CurrentDay == DayOfWeek.Sunday;
        }

        /// <summary>長期休暇中かどうか</summary>
        public bool IsVacation()
        {
            return CurrentVacation != VacationType.None;
        }

        /// <summary>現在の章の最後の週かどうか</summary>
        public bool IsChapterEnd()
        {
            return CurrentWeek == CHAPTER1_END
                || CurrentWeek == CHAPTER2_END
                || CurrentWeek == CHAPTER3_END
                || CurrentWeek == CHAPTER4_END;
        }

        /// <summary>週番号から章を判定する</summary>
        private Chapter DetermineChapter(int week)
        {
            if (week <= SUMMER1_END) return Chapter.Chapter1;
            if (week <= WINTER_END) return Chapter.Chapter2;
            if (week <= SUMMER2_END) return Chapter.Chapter3;
            return Chapter.Chapter4;
        }

        /// <summary>週番号から休暇種別を判定する</summary>
        private VacationType DetermineVacation(int week)
        {
            if (week >= SUMMER1_START && week <= SUMMER1_END) return VacationType.Summer1;
            if (week >= WINTER_START && week <= WINTER_END) return VacationType.Winter;
            if (week >= SUMMER2_START && week <= SUMMER2_END) return VacationType.Summer2;
            return VacationType.None;
        }
    }
}
