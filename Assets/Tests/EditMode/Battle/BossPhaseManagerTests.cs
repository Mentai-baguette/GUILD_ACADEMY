using NUnit.Framework;
using System.Collections.Generic;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class BossPhaseManagerTests
    {
        private BossPhaseManager _manager;

        [SetUp]
        public void SetUp()
        {
            _manager = new BossPhaseManager();
        }

        private CharacterStats CreateBoss(int currentHp, int maxHp = 1000)
        {
            var boss = new CharacterStats("Boss", maxHp, 100, 50, 30, 20);
            boss.CurrentHp = currentHp;
            return boss;
        }

        [Test]
        public void InitialPhase_IsZero_WhenNoPhases()
        {
            Assert.AreEqual(0, _manager.CurrentPhase);
        }

        [Test]
        public void AddPhase_IncreasesTotalPhases()
        {
            var stats = new CharacterStats("Boss", 1000, 100, 50, 30, 20);
            _manager.AddPhase(new BossPhaseData(1, 100f, stats));
            _manager.AddPhase(new BossPhaseData(2, 50f, stats));
            Assert.AreEqual(2, _manager.TotalPhases);
        }

        [Test]
        public void CheckPhaseTransition_AtFullHp_StaysPhase1()
        {
            var stats = new CharacterStats("Boss", 1000, 100, 50, 30, 20);
            _manager.AddPhase(new BossPhaseData(1, 100f, stats));
            _manager.AddPhase(new BossPhaseData(2, 50f, stats));

            var boss = CreateBoss(1000);
            _manager.CheckPhaseTransition(boss);
            Assert.AreEqual(1, _manager.CurrentPhase);
        }

        [Test]
        public void CheckPhaseTransition_Below50Percent_TransitionsToPhase2()
        {
            var stats = new CharacterStats("Boss", 1000, 100, 50, 30, 20);
            _manager.AddPhase(new BossPhaseData(1, 100f, stats));
            _manager.AddPhase(new BossPhaseData(2, 50f, stats));

            var boss = CreateBoss(400); // 40% HP
            bool changed = _manager.CheckPhaseTransition(boss);
            Assert.IsTrue(changed);
            Assert.AreEqual(2, _manager.CurrentPhase);
        }

        [Test]
        public void CheckPhaseTransition_FiresEvent()
        {
            var stats = new CharacterStats("Boss", 1000, 100, 50, 30, 20);
            _manager.AddPhase(new BossPhaseData(1, 100f, stats));
            _manager.AddPhase(new BossPhaseData(2, 50f, stats));

            int firedPhase = -1;
            _manager.OnPhaseChanged += (phase, data) => firedPhase = phase;

            var boss = CreateBoss(400);
            _manager.CheckPhaseTransition(boss);
            Assert.AreEqual(2, firedPhase);
        }

        [Test]
        public void CheckPhaseTransition_NoChange_ReturnsFalse()
        {
            var stats = new CharacterStats("Boss", 1000, 100, 50, 30, 20);
            _manager.AddPhase(new BossPhaseData(1, 100f, stats));
            _manager.AddPhase(new BossPhaseData(2, 50f, stats));

            var boss = CreateBoss(800); // 80% HP, still phase 1
            bool changed = _manager.CheckPhaseTransition(boss);
            Assert.IsFalse(changed);
        }

        [Test]
        public void ThreePhases_TransitionsCorrectly()
        {
            var stats = new CharacterStats("Boss", 1000, 100, 50, 30, 20);
            _manager.AddPhase(new BossPhaseData(1, 100f, stats));
            _manager.AddPhase(new BossPhaseData(2, 50f, stats));
            _manager.AddPhase(new BossPhaseData(3, 25f, stats));

            var boss = CreateBoss(1000);
            _manager.CheckPhaseTransition(boss);
            Assert.AreEqual(1, _manager.CurrentPhase);

            boss.CurrentHp = 400;
            _manager.CheckPhaseTransition(boss);
            Assert.AreEqual(2, _manager.CurrentPhase);

            boss.CurrentHp = 200;
            _manager.CheckPhaseTransition(boss);
            Assert.AreEqual(3, _manager.CurrentPhase);
        }

        [Test]
        public void Reset_ReturnsToPhase0()
        {
            var stats = new CharacterStats("Boss", 1000, 100, 50, 30, 20);
            _manager.AddPhase(new BossPhaseData(1, 100f, stats));
            _manager.AddPhase(new BossPhaseData(2, 50f, stats));

            var boss = CreateBoss(400);
            _manager.CheckPhaseTransition(boss);
            Assert.AreEqual(2, _manager.CurrentPhase);

            _manager.Reset();
            Assert.AreEqual(1, _manager.CurrentPhase);
        }

        [Test]
        public void NullBoss_ReturnsFalse()
        {
            Assert.IsFalse(_manager.CheckPhaseTransition(null));
        }
    }
}
