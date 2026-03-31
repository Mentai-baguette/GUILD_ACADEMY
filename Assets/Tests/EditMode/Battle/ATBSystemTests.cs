using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class ATBSystemTests
    {
        private ATBSystem _atb;

        [SetUp]
        public void SetUp()
        {
            _atb = new ATBSystem();
        }

        [Test]
        public void NewATBSystem_AllGaugesAtZero()
        {
            var char1 = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            _atb.AddCombatant(char1);
            Assert.AreEqual(0f, _atb.GetGauge(char1));
        }

        [Test]
        public void Tick_IncreasesGaugeBySpeedTimeDelta()
        {
            var char1 = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            _atb.AddCombatant(char1);
            _atb.Tick(1.0f);
            Assert.AreEqual(100f, _atb.GetGauge(char1));
        }

        [Test]
        public void Tick_FasterCharacter_FillsFaster()
        {
            var fast = new CharacterStats("Fast", 100, 50, 30, 20, 20);
            var slow = new CharacterStats("Slow", 100, 50, 30, 20, 10);
            _atb.AddCombatant(fast);
            _atb.AddCombatant(slow);
            _atb.Tick(0.3f);
            Assert.Greater(_atb.GetGauge(fast), _atb.GetGauge(slow));
        }

        [Test]
        public void Tick_GaugeReaches100_CharacterIsReady()
        {
            var char1 = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            _atb.AddCombatant(char1);
            _atb.Tick(1.0f);
            var ready = _atb.GetReadyCharacters();
            Assert.Contains(char1, ready);
        }

        [Test]
        public void Tick_GaugeCapsAt100_DoesNotExceed()
        {
            var char1 = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            _atb.AddCombatant(char1);
            _atb.Tick(2.0f);
            Assert.AreEqual(100f, _atb.GetGauge(char1));
        }

        [Test]
        public void GetReadyCharacters_NoOneReady_ReturnsEmpty()
        {
            var char1 = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            _atb.AddCombatant(char1);
            _atb.Tick(0.1f);
            var ready = _atb.GetReadyCharacters();
            Assert.IsEmpty(ready);
        }

        [Test]
        public void GetReadyCharacters_MultipleReady_SortedBySpeedDescending()
        {
            var fast = new CharacterStats("Fast", 100, 50, 30, 20, 20);
            var slow = new CharacterStats("Slow", 100, 50, 30, 20, 10);
            _atb.AddCombatant(slow);
            _atb.AddCombatant(fast);
            _atb.Tick(1.0f);
            var ready = _atb.GetReadyCharacters();
            Assert.AreEqual(2, ready.Count);
            Assert.AreEqual(fast, ready[0]);
            Assert.AreEqual(slow, ready[1]);
        }

        [Test]
        public void ResetGauge_SetsGaugeToZero()
        {
            var char1 = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            _atb.AddCombatant(char1);
            _atb.Tick(1.0f);
            _atb.ResetGauge(char1);
            Assert.AreEqual(0f, _atb.GetGauge(char1));
            Assert.IsEmpty(_atb.GetReadyCharacters());
        }

        [Test]
        public void Tick_SpeedZero_GaugeNeverIncreases()
        {
            var char1 = new CharacterStats("Frozen", 100, 50, 30, 20, 0);
            _atb.AddCombatant(char1);
            _atb.Tick(10.0f);
            Assert.AreEqual(0f, _atb.GetGauge(char1));
        }

        [Test]
        public void Tick_SameSpeed_BothBecomeReadySimultaneously()
        {
            var char1 = new CharacterStats("A", 100, 50, 30, 20, 10);
            var char2 = new CharacterStats("B", 100, 50, 30, 20, 10);
            _atb.AddCombatant(char1);
            _atb.AddCombatant(char2);
            _atb.Tick(1.0f);
            var ready = _atb.GetReadyCharacters();
            Assert.AreEqual(2, ready.Count);
        }

        [Test]
        public void AddCombatant_DuringBattle_StartsAtZeroGauge()
        {
            var char1 = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            _atb.AddCombatant(char1);
            _atb.Tick(0.5f);
            var char2 = new CharacterStats("Late", 100, 50, 30, 20, 15);
            _atb.AddCombatant(char2);
            Assert.AreEqual(0f, _atb.GetGauge(char2));
        }

        [Test]
        public void RemoveCombatant_NoLongerTicked()
        {
            var char1 = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            _atb.AddCombatant(char1);
            _atb.RemoveCombatant(char1);
            _atb.Tick(1.0f);
            Assert.AreEqual(0f, _atb.GetGauge(char1));
            Assert.IsEmpty(_atb.GetReadyCharacters());
        }

        [Test]
        public void Tick_NegativeDeltaTime_GaugeDoesNotChange()
        {
            var char1 = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            _atb.AddCombatant(char1);
            _atb.Tick(0.5f);
            float before = _atb.GetGauge(char1);
            _atb.Tick(-1.0f);
            Assert.AreEqual(before, _atb.GetGauge(char1));
        }

        [Test]
        public void GetGauge_UnregisteredCharacter_ReturnsZero()
        {
            var unknown = new CharacterStats("Unknown", 100, 50, 30, 20, 10);
            Assert.AreEqual(0f, _atb.GetGauge(unknown));
        }

        [Test]
        public void ResetGauge_UnregisteredCharacter_DoesNotThrow()
        {
            var unknown = new CharacterStats("Unknown", 100, 50, 30, 20, 10);
            Assert.DoesNotThrow(() => _atb.ResetGauge(unknown));
        }
    }
}
