using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class BreakSystemTests
    {
        private BreakSystem _breakSystem;
        private CharacterStats _target;

        [SetUp]
        public void SetUp()
        {
            _breakSystem = new BreakSystem();
            _target = new CharacterStats("Enemy", 100, 50, 20, 10, 10);
            _target.WeakElement = ElementType.Fire;
            _breakSystem.Register(_target);
        }

        [Test]
        public void Register_InitializesGaugeAtZero()
        {
            Assert.AreEqual(0f, _breakSystem.GetBreakGaugePercent(_target));
        }

        [Test]
        public void ApplyHit_WeakElement_IncreasesGaugeBy25()
        {
            _breakSystem.ApplyHit(_target, isWeakElement: true);
            Assert.AreEqual(0.25f, _breakSystem.GetBreakGaugePercent(_target), 0.001f);
        }

        [Test]
        public void ApplyHit_NonWeak_IncreasesGaugeBy5()
        {
            _breakSystem.ApplyHit(_target, isWeakElement: false);
            Assert.AreEqual(0.05f, _breakSystem.GetBreakGaugePercent(_target), 0.001f);
        }

        [Test]
        public void ApplyHit_ReachesMax_TriggersBreak_ReturnsTrue()
        {
            // 4 weak hits = 100 gauge
            for (int i = 0; i < 3; i++)
                _breakSystem.ApplyHit(_target, isWeakElement: true);

            bool triggered = _breakSystem.ApplyHit(_target, isWeakElement: true);
            Assert.IsTrue(triggered);
            Assert.IsTrue(_breakSystem.IsBreaking(_target));
        }

        [Test]
        public void IsBreaking_WhenBroken_ReturnsTrue()
        {
            for (int i = 0; i < 4; i++)
                _breakSystem.ApplyHit(_target, isWeakElement: true);

            Assert.IsTrue(_breakSystem.IsBreaking(_target));
        }

        [Test]
        public void IsBreaking_WhenNotBroken_ReturnsFalse()
        {
            Assert.IsFalse(_breakSystem.IsBreaking(_target));
        }

        [Test]
        public void TickBreakRecovery_AfterFullDuration_Recovers()
        {
            for (int i = 0; i < 4; i++)
                _breakSystem.ApplyHit(_target, isWeakElement: true);

            Assert.IsTrue(_breakSystem.IsBreaking(_target));

            _breakSystem.TickBreakRecovery(_target); // turn 1
            _breakSystem.TickBreakRecovery(_target); // turn 2 -> recovers

            Assert.IsFalse(_breakSystem.IsBreaking(_target));
        }

        [Test]
        public void TickBreakRecovery_PartialDuration_StillBroken()
        {
            for (int i = 0; i < 4; i++)
                _breakSystem.ApplyHit(_target, isWeakElement: true);

            _breakSystem.TickBreakRecovery(_target); // turn 1 only

            Assert.IsTrue(_breakSystem.IsBreaking(_target));
        }

        [Test]
        public void Reset_ClearsGaugeAndState()
        {
            for (int i = 0; i < 4; i++)
                _breakSystem.ApplyHit(_target, isWeakElement: true);

            _breakSystem.Reset(_target);

            Assert.IsFalse(_breakSystem.IsBreaking(_target));
            Assert.AreEqual(0f, _breakSystem.GetBreakGaugePercent(_target));
        }

        [Test]
        public void OnBreakTriggered_EventFires()
        {
            CharacterStats firedFor = null;
            _breakSystem.OnBreakTriggered += c => firedFor = c;

            for (int i = 0; i < 4; i++)
                _breakSystem.ApplyHit(_target, isWeakElement: true);

            Assert.AreEqual(_target, firedFor);
        }
    }
}
