using System;
using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Calendar;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Events
{
    /// <summary>
    /// CalendarManagerの現在状態に基づいてイベントを配置・優先度制御するスケジューラ。
    /// Pure C# 実装。
    /// </summary>
    public class EventScheduler
    {
        private readonly FlagSystem _flagSystem;
        private readonly TrustSystem _trustSystem;
        private readonly List<EventData> _events = new List<EventData>();
        private readonly HashSet<string> _completedEventIds = new HashSet<string>();

        /// <summary>完了済みイベントID一覧</summary>
        public IReadOnlyList<string> CompletedEventIds => _completedEventIds.ToList().AsReadOnly();

        public EventScheduler(FlagSystem flagSystem, TrustSystem trustSystem)
        {
            _flagSystem = flagSystem ?? throw new ArgumentNullException(nameof(flagSystem));
            _trustSystem = trustSystem ?? throw new ArgumentNullException(nameof(trustSystem));
        }

        /// <summary>イベントを登録する</summary>
        public void RegisterEvent(EventData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _events.Add(data);
        }

        /// <summary>複数イベントを一括登録する</summary>
        public void RegisterEvents(IEnumerable<EventData> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            foreach (var e in events)
                RegisterEvent(e);
        }

        /// <summary>
        /// 現在の時間帯に利用可能なイベントを優先度順で取得する。
        /// 強制イベントが存在する場合、強制イベントのみを返す。
        /// </summary>
        public List<EventData> GetAvailableEvents(int week, CalendarDay day, TimeOfDay time, Chapter chapter)
        {
            var chapterIndex = ChapterToIndex(chapter);

            var matched = _events.Where(e => MatchesConditions(e, week, day, time, chapterIndex)).ToList();

            // 優先度順 + 同一優先度内はEventId昇順
            matched.Sort((a, b) =>
            {
                var typeCmp = a.Type.CompareTo(b.Type);
                if (typeCmp != 0) return typeCmp;
                return string.Compare(a.EventId, b.EventId, StringComparison.Ordinal);
            });

            // 強制イベントが存在する場合、強制イベントのみを返す
            var forced = matched.Where(e => e.Type == EventType.Forced).ToList();
            if (forced.Count > 0)
                return forced;

            return matched;
        }

        /// <summary>最優先イベントを1つ取得する。なければnull。</summary>
        public EventData GetNextEvent(int week, CalendarDay day, TimeOfDay time, Chapter chapter)
        {
            var available = GetAvailableEvents(week, day, time, chapter);
            return available.Count > 0 ? available[0] : null;
        }

        /// <summary>イベントを完了済みにする</summary>
        public void CompleteEvent(string eventId)
        {
            if (eventId == null) throw new ArgumentNullException(nameof(eventId));
            _completedEventIds.Add(eventId);
        }

        /// <summary>個別イベントが条件にマッチするか判定</summary>
        private bool MatchesConditions(EventData e, int week, CalendarDay day, TimeOfDay time, int chapterIndex)
        {
            // 完了済み（非リピータブル）を除外
            if (!e.IsRepeatable && _completedEventIds.Contains(e.EventId))
                return false;

            // 章チェック
            if (e.Chapter != 0 && e.Chapter != chapterIndex)
                return false;

            // 週チェック (0 = any)
            if (e.Week != 0 && e.Week != week)
                return false;

            // 曜日チェック (null = any)
            if (e.Day.HasValue && e.Day.Value != day)
                return false;

            // 時間帯チェック
            if (e.TimeSlot != time)
                return false;

            // 前提フラグチェック
            if (e.PrerequisiteFlags != null && e.PrerequisiteFlags.Length > 0)
            {
                foreach (var flag in e.PrerequisiteFlags)
                {
                    try
                    {
                        if (!_flagSystem.Get(flag))
                            return false;
                    }
                    catch (ArgumentException)
                    {
                        // 未知のフラグは未達成扱い
                        return false;
                    }
                }
            }

            // SLイベント信頼度チェック
            if (e.Type == EventType.SoulLink && !string.IsNullOrEmpty(e.TargetCharacterId))
            {
                if (Enum.TryParse<CharacterId>(e.TargetCharacterId, out var charId))
                {
                    if (!_trustSystem.IsTrustTarget(charId) || !_trustSystem.MeetsThreshold(charId, e.RequiredTrust))
                        return false;
                }
                else
                {
                    // パースできないキャラIDは条件不達成
                    return false;
                }
            }

            return true;
        }

        /// <summary>Chapter enum を 1-based int に変換</summary>
        private static int ChapterToIndex(Chapter chapter)
        {
            switch (chapter)
            {
                case Chapter.Chapter1: return 1;
                case Chapter.Chapter2: return 2;
                case Chapter.Chapter3: return 3;
                case Chapter.Chapter4: return 4;
                default: return 0;
            }
        }
    }
}
