using System.Collections.Generic;
using NUnit.Framework;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Calendar;
using GuildAcademy.Core.Events;

namespace GuildAcademy.Tests.EditMode.Events
{
    [TestFixture]
    public class EventSchedulerTests
    {
        private FlagSystem _flagSystem;
        private TrustSystem _trustSystem;
        private EventScheduler _scheduler;

        [SetUp]
        public void SetUp()
        {
            _flagSystem = new FlagSystem();
            _trustSystem = new TrustSystem();
            _scheduler = new EventScheduler(_flagSystem, _trustSystem);
        }

        // ── 登録と取得 ──

        [Test]
        public void RegisterEvent_And_GetAvailable_ReturnsSingleEvent()
        {
            var evt = CreateEvent("evt1", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1);
            _scheduler.RegisterEvent(evt);

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("evt1", result[0].EventId);
        }

        [Test]
        public void RegisterEvents_Batch_AllRetrievable()
        {
            var events = new List<EventData>
            {
                CreateEvent("a", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1),
                CreateEvent("b", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1)
            };
            _scheduler.RegisterEvents(events);

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(2, result.Count);
        }

        // ── 優先度順ソート ──

        [Test]
        public void GetAvailableEvents_SortsByPriority_WithoutForced()
        {
            _scheduler.RegisterEvent(CreateEvent("free1", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("main1", EventType.Main, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("random1", EventType.Random, timeSlot: TimeOfDay.Morning, chapter: 1));

            // Forced がある場合、Forced のみ返る
            // Forced なしの場合のソートをテストするため、Forced は別テストで
            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(EventType.Main, result[0].Type);
            Assert.AreEqual(EventType.Random, result[1].Type);
            Assert.AreEqual(EventType.Free, result[2].Type);
        }

        [Test]
        public void GetAvailableEvents_SameTypeSortedByEventIdAscending()
        {
            _scheduler.RegisterEvent(CreateEvent("c_event", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("a_event", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("b_event", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1));

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);

            Assert.AreEqual("a_event", result[0].EventId);
            Assert.AreEqual("b_event", result[1].EventId);
            Assert.AreEqual("c_event", result[2].EventId);
        }

        [Test]
        public void GetAvailableEvents_FullPriorityOrder()
        {
            _scheduler.RegisterEvent(CreateEvent("free1", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("sl1", EventType.SoulLink, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("main1", EventType.Main, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("teacher1", EventType.Teacher, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("random1", EventType.Random, timeSlot: TimeOfDay.Morning, chapter: 1));

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);

            Assert.AreEqual(EventType.Main, result[0].Type);
            Assert.AreEqual(EventType.SoulLink, result[1].Type);
            Assert.AreEqual(EventType.Teacher, result[2].Type);
            Assert.AreEqual(EventType.Random, result[3].Type);
            Assert.AreEqual(EventType.Free, result[4].Type);
        }

        // ── 強制イベント上書き ──

        [Test]
        public void ForcedEvent_OverridesAllOthers()
        {
            _scheduler.RegisterEvent(CreateEvent("main1", EventType.Main, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("forced1", EventType.Forced, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("free1", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1));

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("forced1", result[0].EventId);
        }

        [Test]
        public void GetNextEvent_ReturnsForcedWhenPresent()
        {
            _scheduler.RegisterEvent(CreateEvent("main1", EventType.Main, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("forced1", EventType.Forced, timeSlot: TimeOfDay.Morning, chapter: 1));

            var next = _scheduler.GetNextEvent(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);

            Assert.AreEqual("forced1", next.EventId);
        }

        // ── フラグ前提条件フィルタリング ──

        [Test]
        public void PrerequisiteFlags_Unmet_EventExcluded()
        {
            var evt = CreateEvent("flag_evt", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1);
            evt.PrerequisiteFlags = new[] { FlagSystem.Flags.ShionPast };
            _scheduler.RegisterEvent(evt);

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void PrerequisiteFlags_Met_EventIncluded()
        {
            var evt = CreateEvent("flag_evt", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1);
            evt.PrerequisiteFlags = new[] { FlagSystem.Flags.ShionPast };
            _scheduler.RegisterEvent(evt);

            _flagSystem.Set(FlagSystem.Flags.ShionPast, true);

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void PrerequisiteFlags_PartiallyMet_EventExcluded()
        {
            var evt = CreateEvent("flag_evt", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1);
            evt.PrerequisiteFlags = new[] { FlagSystem.Flags.ShionPast, FlagSystem.Flags.CarlosPlan };
            _scheduler.RegisterEvent(evt);

            _flagSystem.Set(FlagSystem.Flags.ShionPast, true);
            // CarlosPlan は未設定

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(0, result.Count);
        }

        // ── SLイベント信頼度条件 ──

        [Test]
        public void SoulLinkEvent_TrustNotMet_Excluded()
        {
            var evt = CreateEvent("sl_evt", EventType.SoulLink, timeSlot: TimeOfDay.Morning, chapter: 1);
            evt.TargetCharacterId = "Yuna";
            evt.RequiredTrust = 50;
            _scheduler.RegisterEvent(evt);

            _trustSystem.SetTrust(GuildAcademy.Core.Data.CharacterId.Yuna, 30);

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void SoulLinkEvent_TrustMet_Included()
        {
            var evt = CreateEvent("sl_evt", EventType.SoulLink, timeSlot: TimeOfDay.Morning, chapter: 1);
            evt.TargetCharacterId = "Yuna";
            evt.RequiredTrust = 50;
            _scheduler.RegisterEvent(evt);

            _trustSystem.SetTrust(GuildAcademy.Core.Data.CharacterId.Yuna, 50);

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void SoulLinkEvent_InvalidCharacterId_Excluded()
        {
            var evt = CreateEvent("sl_evt", EventType.SoulLink, timeSlot: TimeOfDay.Morning, chapter: 1);
            evt.TargetCharacterId = "NonExistentChar";
            evt.RequiredTrust = 10;
            _scheduler.RegisterEvent(evt);

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(0, result.Count);
        }

        // ── 章/週/曜日/時間帯フィルタリング ──

        [Test]
        public void Filter_WrongChapter_Excluded()
        {
            _scheduler.RegisterEvent(CreateEvent("ch2", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 2));

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Filter_ChapterZero_MatchesAny()
        {
            _scheduler.RegisterEvent(CreateEvent("any_ch", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 0));

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void Filter_WrongWeek_Excluded()
        {
            _scheduler.RegisterEvent(CreateEvent("w5", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1, week: 5));

            var result = _scheduler.GetAvailableEvents(3, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Filter_WeekZero_MatchesAny()
        {
            _scheduler.RegisterEvent(CreateEvent("any_w", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1, week: 0));

            var result = _scheduler.GetAvailableEvents(7, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void Filter_WrongDay_Excluded()
        {
            var evt = CreateEvent("tue", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1);
            evt.Day = CalendarDay.Tuesday;
            _scheduler.RegisterEvent(evt);

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Filter_NullDay_MatchesAny()
        {
            var evt = CreateEvent("any_day", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1);
            evt.Day = null;
            _scheduler.RegisterEvent(evt);

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Wednesday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void Filter_WrongTimeSlot_Excluded()
        {
            _scheduler.RegisterEvent(CreateEvent("night", EventType.Free, timeSlot: TimeOfDay.Night, chapter: 1));

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(0, result.Count);
        }

        // ── 完了済みイベント除外 ──

        [Test]
        public void CompletedEvent_NonRepeatable_Excluded()
        {
            var evt = CreateEvent("done", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1);
            evt.IsRepeatable = false;
            _scheduler.RegisterEvent(evt);

            _scheduler.CompleteEvent("done");

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void CompletedEventIds_TracksCompletions()
        {
            _scheduler.CompleteEvent("a");
            _scheduler.CompleteEvent("b");

            Assert.IsTrue(_scheduler.CompletedEventIds.Contains("a"));
            Assert.IsTrue(_scheduler.CompletedEventIds.Contains("b"));
        }

        // ── リピータブルイベント ──

        [Test]
        public void RepeatableEvent_StillAvailableAfterCompletion()
        {
            var evt = CreateEvent("repeat", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1);
            evt.IsRepeatable = true;
            _scheduler.RegisterEvent(evt);

            _scheduler.CompleteEvent("repeat");

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual(1, result.Count);
        }

        // ── GetNextEvent ──

        [Test]
        public void GetNextEvent_ReturnsHighestPriority()
        {
            _scheduler.RegisterEvent(CreateEvent("free1", EventType.Free, timeSlot: TimeOfDay.Morning, chapter: 1));
            _scheduler.RegisterEvent(CreateEvent("main1", EventType.Main, timeSlot: TimeOfDay.Morning, chapter: 1));

            var next = _scheduler.GetNextEvent(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.AreEqual("main1", next.EventId);
        }

        // ── 空の場合のnull安全性 ──

        [Test]
        public void GetAvailableEvents_Empty_ReturnsEmptyList()
        {
            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetNextEvent_Empty_ReturnsNull()
        {
            var next = _scheduler.GetNextEvent(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.IsNull(next);
        }

        [Test]
        public void GetNextEvent_NoMatchingEvents_ReturnsNull()
        {
            _scheduler.RegisterEvent(CreateEvent("night", EventType.Free, timeSlot: TimeOfDay.Night, chapter: 1));

            var next = _scheduler.GetNextEvent(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            Assert.IsNull(next);
        }

        // ── JSON読み込み ──
        // Note: EventDataLoader is implemented with a lightweight parser without using JsonUtility.
        // These tests validate JSON loading behavior in EditMode with the Unity test runner.

        [Test]
        public void LoadFromJson_ValidJson_ReturnsEvents()
        {
            var json = @"{
                ""events"": [
                    {
                        ""eventId"": ""ch1_w1_morning_forced_intro"",
                        ""type"": ""Forced"",
                        ""chapter"": 1,
                        ""week"": 1,
                        ""day"": ""Monday"",
                        ""timeSlot"": ""Morning"",
                        ""prerequisiteFlags"": [],
                        ""dialogueKey"": ""ch1_intro"",
                        ""location"": ""classroom"",
                        ""isRepeatable"": false
                    }
                ]
            }";

            var events = EventDataLoader.LoadFromJson(json);
            Assert.AreEqual(1, events.Count);
            Assert.AreEqual("ch1_w1_morning_forced_intro", events[0].EventId);
            Assert.AreEqual(EventType.Forced, events[0].Type);
            Assert.AreEqual(1, events[0].Chapter);
            Assert.AreEqual(1, events[0].Week);
            Assert.AreEqual(CalendarDay.Monday, events[0].Day);
            Assert.AreEqual(TimeOfDay.Morning, events[0].TimeSlot);
            Assert.AreEqual("ch1_intro", events[0].DialogueKey);
            Assert.AreEqual("classroom", events[0].Location);
            Assert.IsFalse(events[0].IsRepeatable);
        }

        [Test]
        public void LoadFromJson_EmptyString_ReturnsEmptyList()
        {
            var result = EventDataLoader.LoadFromJson("");
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void LoadFromJson_Null_ReturnsEmptyList()
        {
            var result = EventDataLoader.LoadFromJson(null);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void LoadFromJson_MultipleEvents_AllLoaded()
        {
            var json = @"{
                ""events"": [
                    {
                        ""eventId"": ""evt1"",
                        ""type"": ""Main"",
                        ""chapter"": 1,
                        ""week"": 0,
                        ""timeSlot"": ""Morning"",
                        ""isRepeatable"": false
                    },
                    {
                        ""eventId"": ""evt2"",
                        ""type"": ""Free"",
                        ""chapter"": 2,
                        ""week"": 0,
                        ""timeSlot"": ""Afternoon"",
                        ""isRepeatable"": true
                    }
                ]
            }";

            var events = EventDataLoader.LoadFromJson(json);
            Assert.AreEqual(2, events.Count);
            Assert.AreEqual("evt1", events[0].EventId);
            Assert.AreEqual("evt2", events[1].EventId);
            Assert.AreEqual(EventType.Main, events[0].Type);
            Assert.AreEqual(EventType.Free, events[1].Type);
            Assert.IsTrue(events[1].IsRepeatable);
        }

        [Test]
        public void LoadFromJson_ThenRegister_IntegrationTest()
        {
            var json = @"{
                ""events"": [
                    {
                        ""eventId"": ""forced1"",
                        ""type"": ""Forced"",
                        ""chapter"": 1,
                        ""week"": 1,
                        ""day"": ""Monday"",
                        ""timeSlot"": ""Morning"",
                        ""isRepeatable"": false
                    },
                    {
                        ""eventId"": ""free1"",
                        ""type"": ""Free"",
                        ""chapter"": 1,
                        ""week"": 1,
                        ""day"": ""Monday"",
                        ""timeSlot"": ""Morning"",
                        ""isRepeatable"": false
                    }
                ]
            }";

            var events = EventDataLoader.LoadFromJson(json);
            _scheduler.RegisterEvents(events);

            var result = _scheduler.GetAvailableEvents(1, CalendarDay.Monday, TimeOfDay.Morning, Chapter.Chapter1);
            // Forced overrides all
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("forced1", result[0].EventId);
        }

        // ── ヘルパー ──

        private static EventData CreateEvent(
            string eventId,
            EventType type,
            TimeOfDay timeSlot = TimeOfDay.Morning,
            int chapter = 0,
            int week = 0)
        {
            return new EventData
            {
                EventId = eventId,
                Type = type,
                Chapter = chapter,
                Week = week,
                Day = null,
                TimeSlot = timeSlot,
                PrerequisiteFlags = System.Array.Empty<string>(),
                TargetCharacterId = "",
                RequiredTrust = 0,
                DialogueKey = "",
                Location = "",
                IsRepeatable = false
            };
        }
    }
}
