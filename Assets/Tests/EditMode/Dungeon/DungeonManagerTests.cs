using NUnit.Framework;
using GuildAcademy.Core.Calendar;
using GuildAcademy.Core.Dungeon;
using GuildAcademy.Core.Schedule;

namespace GuildAcademy.Tests.EditMode.Dungeon
{
    [TestFixture]
    public class DungeonManagerTests
    {
        private CalendarManager _calendar;
        private ScheduleManager _schedule;
        private DungeonManager _dungeon;
        private FakeRandom _fakeRandom;
        private DungeonData _testDungeon;

        [SetUp]
        public void SetUp()
        {
            _calendar = new CalendarManager();
            _schedule = new ScheduleManager(_calendar);
            _fakeRandom = new FakeRandom();
            _dungeon = new DungeonManager(_schedule, _fakeRandom);

            _testDungeon = new DungeonData
            {
                DungeonId = "emerald_forest",
                Name = "翠風の森",
                TotalFloors = 15,
                PortalInterval = 5,
                BossFloor = 15,
                MidBossFloor = 8,
                BaseEncounterRate = 0.065f,
                DepthEncounterBonus = 0.02f
            };
        }

        // --- Enter / Exit ---

        [Test]
        public void EnterDungeon_SetsCurrentDungeon()
        {
            _dungeon.EnterDungeon(_testDungeon);
            Assert.IsTrue(_dungeon.IsInDungeon);
            Assert.AreEqual("emerald_forest", _dungeon.CurrentDungeon.DungeonId);
            Assert.AreEqual(1, _dungeon.CurrentFloor);
        }

        [Test]
        public void ExitDungeon_ClearsState()
        {
            _dungeon.EnterDungeon(_testDungeon);
            _dungeon.ExitDungeon();
            Assert.IsFalse(_dungeon.IsInDungeon);
            Assert.AreEqual(0, _dungeon.CurrentFloor);
        }

        [Test]
        public void ExitDungeon_FiresEvent()
        {
            bool fired = false;
            _dungeon.OnDungeonExited += () => fired = true;
            _dungeon.EnterDungeon(_testDungeon);
            _dungeon.ExitDungeon();
            Assert.IsTrue(fired);
        }

        // --- AdvanceFloor ---

        [Test]
        public void AdvanceFloor_IncrementsFloor()
        {
            _dungeon.EnterDungeon(_testDungeon);
            bool result = _dungeon.AdvanceFloor();
            Assert.IsTrue(result);
            Assert.AreEqual(2, _dungeon.CurrentFloor);
        }

        [Test]
        public void AdvanceFloor_ConsumesLayer()
        {
            _dungeon.EnterDungeon(_testDungeon);
            int initialLayers = _dungeon.LayersRemaining;
            _dungeon.AdvanceFloor();
            Assert.AreEqual(initialLayers - 1, _dungeon.LayersRemaining);
        }

        [Test]
        public void AdvanceFloor_BeyondBudget_ReturnsFalse()
        {
            _schedule.SetTodayLessonCount(4); // 2 layers
            _dungeon.EnterDungeon(_testDungeon);
            Assert.IsTrue(_dungeon.AdvanceFloor()); // layer 1
            Assert.IsTrue(_dungeon.AdvanceFloor()); // layer 2
            Assert.IsFalse(_dungeon.AdvanceFloor()); // no more layers
        }

        [Test]
        public void AdvanceFloor_FiresFloorChangedEvent()
        {
            int eventFloor = 0;
            _dungeon.OnFloorChanged += f => eventFloor = f;
            _dungeon.EnterDungeon(_testDungeon);
            _dungeon.AdvanceFloor();
            Assert.AreEqual(2, eventFloor);
        }

        // --- Layer Budget ---

        [Test]
        public void LayerBudget_0Lessons_4Layers()
        {
            _schedule.SetTodayLessonCount(0);
            _dungeon.EnterDungeon(_testDungeon);
            Assert.AreEqual(4, _dungeon.LayersRemaining);
        }

        [Test]
        public void LayerBudget_NightBonus_AddsOneLayer()
        {
            _calendar.AdvanceTime(); // -> Afternoon
            _calendar.AdvanceTime(); // -> Night
            _schedule.SetTodayLessonCount(0);
            _dungeon.EnterDungeon(_testDungeon);
            Assert.AreEqual(5, _dungeon.LayersRemaining); // 4 + 1 night
        }

        // --- Max Reached Floor ---

        [Test]
        public void GetMaxReachedFloor_TracksHighestFloor()
        {
            _dungeon.EnterDungeon(_testDungeon);
            _dungeon.AdvanceFloor(); // floor 2
            _dungeon.AdvanceFloor(); // floor 3
            _dungeon.ExitDungeon();

            Assert.AreEqual(3, _dungeon.GetMaxReachedFloor("emerald_forest"));
        }

        // --- Portal ---

        [Test]
        public void AdvanceFloor_PortalFloor_ActivatesPortal()
        {
            _dungeon.EnterDungeon(_testDungeon);
            // Advance to floor 5 (portal)
            for (int i = 0; i < 4; i++)
                _dungeon.AdvanceFloor();

            Assert.AreEqual(5, _dungeon.CurrentFloor);
            Assert.IsTrue(_dungeon.IsPortalActivated("emerald_forest", 5));
        }

        [Test]
        public void ReturnToPortal_GoesToNearestActivatedPortal()
        {
            _dungeon.EnterDungeon(_testDungeon);
            for (int i = 0; i < 4; i++) // go to floor 5
                _dungeon.AdvanceFloor();

            // Now advance past portal
            _dungeon.AdvanceFloor(); // floor 6

            int portal = _dungeon.ReturnToPortal();
            Assert.AreEqual(5, portal);
            Assert.AreEqual(5, _dungeon.CurrentFloor);
        }

        // --- Boss Floor ---

        [Test]
        public void AdvanceFloor_BossFloor_FiresBossEvent()
        {
            var smallDungeon = new DungeonData
            {
                DungeonId = "test", Name = "Test", TotalFloors = 3,
                PortalInterval = 5, BossFloor = 3, BaseEncounterRate = 0
            };

            int bossFloor = 0;
            _dungeon.OnBossFloorReached += f => bossFloor = f;
            _dungeon.EnterDungeon(smallDungeon);
            _dungeon.AdvanceFloor(); // floor 2
            _dungeon.AdvanceFloor(); // floor 3 (boss)
            Assert.AreEqual(3, bossFloor);
        }

        // --- Encounter ---

        [Test]
        public void CheckEncounter_HighRoll_NoEncounter()
        {
            _fakeRandom.NextValue = 0.99f; // Way above rate
            _dungeon.EnterDungeon(_testDungeon);
            Assert.IsFalse(_dungeon.CheckEncounter());
        }

        [Test]
        public void CheckEncounter_LowRoll_EncounterOccurs()
        {
            _fakeRandom.NextValue = 0.01f; // Below base rate
            _dungeon.EnterDungeon(_testDungeon);
            Assert.IsTrue(_dungeon.CheckEncounter());
        }

        [Test]
        public void CheckEncounter_OnBossFloor_NeverTriggers()
        {
            var smallDungeon = new DungeonData
            {
                DungeonId = "test", Name = "Test", TotalFloors = 2,
                PortalInterval = 5, BossFloor = 2, BaseEncounterRate = 1.0f
            };

            _fakeRandom.NextValue = 0.0f;
            _dungeon.EnterDungeon(smallDungeon);
            _dungeon.AdvanceFloor(); // floor 2 (boss)
            Assert.IsFalse(_dungeon.CheckEncounter());
        }

        [Test]
        public void CheckEncounter_FiresEvent()
        {
            float firedRate = -1f;
            _dungeon.OnEncounterTriggered += r => firedRate = r;
            _fakeRandom.NextValue = 0.01f;
            _dungeon.EnterDungeon(_testDungeon);
            _dungeon.CheckEncounter();
            Assert.That(firedRate, Is.GreaterThanOrEqualTo(0f));
        }

        // --- Night Multiplier ---

        [Test]
        public void GetCurrentEnemyMultiplier_Day_Returns1()
        {
            _dungeon.EnterDungeon(_testDungeon);
            Assert.AreEqual(1.0f, _dungeon.GetCurrentEnemyMultiplier());
        }

        [Test]
        public void GetCurrentEnemyMultiplier_Night_Returns1Point5()
        {
            _calendar.AdvanceTime(); // Afternoon
            _calendar.AdvanceTime(); // Night
            _dungeon.EnterDungeon(_testDungeon);
            Assert.AreEqual(1.5f, _dungeon.GetCurrentEnemyMultiplier());
        }

        // --- DungeonData ---

        [Test]
        public void DungeonData_IsPortalFloor_CorrectForInterval5()
        {
            Assert.IsTrue(_testDungeon.IsPortalFloor(5));
            Assert.IsTrue(_testDungeon.IsPortalFloor(10));
            Assert.IsFalse(_testDungeon.IsPortalFloor(3));
            Assert.IsFalse(_testDungeon.IsPortalFloor(0));
        }

        [Test]
        public void DungeonData_GetEncounterRate_IncreasesWithDepth()
        {
            float rateFloor1 = _testDungeon.GetEncounterRate(1);
            float rateFloor15 = _testDungeon.GetEncounterRate(15);
            Assert.That(rateFloor15, Is.GreaterThan(rateFloor1));
        }

        // === Test Helper ===

        private class FakeRandom : IDungeonRandom
        {
            public float NextValue { get; set; }
            public float NextFloat() => NextValue;
        }
    }
}
