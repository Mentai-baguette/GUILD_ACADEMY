using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Character;
using NUnit.Framework;

namespace GuildAcademy.Tests.EditMode.Character
{
    [TestFixture]
    public class ErosionSystemTests
    {
        private ErosionSystem _system;

        [SetUp]
        public void SetUp()
        {
            _system = new ErosionSystem();
        }

        // --- テスト用の固定乱数 ---
        private class FixedRandom : IRandom
        {
            private readonly int _value;
            public FixedRandom(int value) { _value = value; }
            public int Range(int minInclusive, int maxExclusive) => _value;
        }

        // === 初期状態テスト ===

        [Test]
        public void InitialState_IsNormalAndZero()
        {
            Assert.AreEqual(0f, _system.CurrentErosion);
            Assert.AreEqual(ErosionStage.Normal, _system.CurrentStage);
            Assert.AreEqual(1.0f, _system.AtkMultiplier);
            Assert.IsFalse(_system.IsEltDefeated);
        }

        // === 侵蝕増加テスト ===

        [Test]
        public void AddErosion_IncreasesValue()
        {
            _system.AddErosion(10f);
            Assert.AreEqual(10f, _system.CurrentErosion);
        }

        [Test]
        public void AddErosion_NegativeOrZero_DoesNothing()
        {
            _system.AddErosion(10f);
            _system.AddErosion(-5f);
            _system.AddErosion(0f);
            Assert.AreEqual(10f, _system.CurrentErosion);
        }

        [Test]
        public void AddErosion_TransitionsToUnstableAt25()
        {
            _system.AddErosion(24f);
            Assert.AreEqual(ErosionStage.Normal, _system.CurrentStage);

            _system.AddErosion(1f);
            Assert.AreEqual(ErosionStage.Unstable, _system.CurrentStage);
        }

        [Test]
        public void AddErosion_TransitionsToDangerousAt50()
        {
            _system.AddErosion(49f);
            Assert.AreEqual(ErosionStage.Unstable, _system.CurrentStage);

            _system.AddErosion(1f);
            Assert.AreEqual(ErosionStage.Dangerous, _system.CurrentStage);
        }

        [Test]
        public void AddErosion_TransitionsToCriticalAt75()
        {
            _system.AddErosion(74f);
            Assert.AreEqual(ErosionStage.Dangerous, _system.CurrentStage);

            _system.AddErosion(1f);
            Assert.AreEqual(ErosionStage.Critical, _system.CurrentStage);
        }

        // === 上限テスト ===

        [Test]
        public void AddErosion_ClampsAtMax100()
        {
            _system.AddErosion(150f);
            Assert.AreEqual(ErosionSystem.MAX_EROSION, _system.CurrentErosion);
        }

        // === ATK倍率テスト ===

        [Test]
        public void AtkMultiplier_Normal_Is1x()
        {
            Assert.AreEqual(ErosionSystem.NORMAL_ATK_MULTIPLIER, _system.AtkMultiplier);
        }

        [Test]
        public void AtkMultiplier_Unstable_Is1_3x()
        {
            _system.AddErosion(25f);
            Assert.AreEqual(ErosionSystem.UNSTABLE_ATK_MULTIPLIER, _system.AtkMultiplier);
        }

        [Test]
        public void AtkMultiplier_Dangerous_Is1_6x()
        {
            _system.AddErosion(50f);
            Assert.AreEqual(ErosionSystem.DANGEROUS_ATK_MULTIPLIER, _system.AtkMultiplier);
        }

        [Test]
        public void AtkMultiplier_Critical_Is2x()
        {
            _system.AddErosion(75f);
            Assert.AreEqual(ErosionSystem.CRITICAL_ATK_MULTIPLIER, _system.AtkMultiplier);
        }

        // === バトル中浄化テスト ===

        [Test]
        public void PurifyInBattle_DropsOneStage()
        {
            _system.AddErosion(50f); // Dangerous
            _system.OnBattleStart();

            bool result = _system.PurifyInBattle();

            Assert.IsTrue(result);
            Assert.AreEqual(ErosionStage.Unstable, _system.CurrentStage);
        }

        [Test]
        public void PurifyInBattle_FromCritical_GoesToDangerous()
        {
            _system.AddErosion(80f); // Critical
            _system.OnBattleStart();

            _system.PurifyInBattle();

            Assert.AreEqual(ErosionStage.Dangerous, _system.CurrentStage);
            Assert.AreEqual(ErosionSystem.DANGEROUS_THRESHOLD, _system.CurrentErosion);
        }

        [Test]
        public void PurifyInBattle_LimitedToOncePerBattle()
        {
            _system.AddErosion(80f); // Critical
            _system.OnBattleStart();

            bool first = _system.PurifyInBattle();
            bool second = _system.PurifyInBattle();

            Assert.IsTrue(first);
            Assert.IsFalse(second);
        }

        [Test]
        public void PurifyInBattle_AtNormal_ReturnsFalse()
        {
            _system.OnBattleStart();
            bool result = _system.PurifyInBattle();
            Assert.IsFalse(result);
        }

        [Test]
        public void PurifyInBattle_ResetsOnNewBattle()
        {
            _system.AddErosion(80f); // Critical
            _system.OnBattleStart();
            _system.PurifyInBattle(); // Dangerous (50)

            _system.AddErosion(30f); // Critical again (80)
            _system.OnBattleStart();

            bool result = _system.PurifyInBattle();
            Assert.IsTrue(result);
        }

        // === フィールド浄化テスト ===

        [Test]
        public void PurifyField_DecreasesErosion()
        {
            _system.AddErosion(50f);
            _system.PurifyField(20f);
            Assert.AreEqual(30f, _system.CurrentErosion);
        }

        [Test]
        public void PurifyField_ClampsAtZero()
        {
            _system.AddErosion(10f);
            _system.PurifyField(50f);
            Assert.AreEqual(0f, _system.CurrentErosion);
        }

        [Test]
        public void PurifyField_NegativeOrZero_DoesNothing()
        {
            _system.AddErosion(50f);
            _system.PurifyField(-10f);
            _system.PurifyField(0f);
            Assert.AreEqual(50f, _system.CurrentErosion);
        }

        // === 暴走判定テスト ===

        [Test]
        public void CheckRampage_NotCritical_ReturnsFalse()
        {
            _system.AddErosion(50f); // Dangerous
            var random = new FixedRandom(0); // Would always trigger
            Assert.IsFalse(_system.CheckRampage(random));
        }

        [Test]
        public void CheckRampage_Critical_RollUnderChance_ReturnsTrue()
        {
            _system.AddErosion(80f); // Critical
            var random = new FixedRandom(10); // 10 < 30 default
            Assert.IsTrue(_system.CheckRampage(random));
        }

        [Test]
        public void CheckRampage_Critical_RollOverChance_ReturnsFalse()
        {
            _system.AddErosion(80f); // Critical
            var random = new FixedRandom(50); // 50 >= 30 default
            Assert.IsFalse(_system.CheckRampage(random));
        }

        [Test]
        public void CheckRampage_Critical_ExactBoundary_ReturnsFalse()
        {
            _system.AddErosion(80f); // Critical
            var random = new FixedRandom(30); // 30 is not < 30
            Assert.IsFalse(_system.CheckRampage(random));
        }

        [Test]
        public void CheckRampage_CustomChance()
        {
            _system.AddErosion(80f); // Critical
            var random = new FixedRandom(49);
            Assert.IsTrue(_system.CheckRampage(random, 50));
            Assert.IsFalse(_system.CheckRampage(random, 49));
        }

        // === エルト撃破後の閾値変更テスト ===

        [Test]
        public void EltDefeated_ThresholdsAreEnhanced()
        {
            _system.SetEltDefeated();

            _system.AddErosion(34f);
            Assert.AreEqual(ErosionStage.Normal, _system.CurrentStage);

            _system.AddErosion(1f); // 35
            Assert.AreEqual(ErosionStage.Unstable, _system.CurrentStage);
        }

        [Test]
        public void EltDefeated_DangerousAt60()
        {
            _system.SetEltDefeated();

            _system.AddErosion(59f);
            Assert.AreEqual(ErosionStage.Unstable, _system.CurrentStage);

            _system.AddErosion(1f); // 60
            Assert.AreEqual(ErosionStage.Dangerous, _system.CurrentStage);
        }

        [Test]
        public void EltDefeated_CriticalAt85()
        {
            _system.SetEltDefeated();

            _system.AddErosion(84f);
            Assert.AreEqual(ErosionStage.Dangerous, _system.CurrentStage);

            _system.AddErosion(1f); // 85
            Assert.AreEqual(ErosionStage.Critical, _system.CurrentStage);
        }

        [Test]
        public void EltDefeated_PurifyInBattle_UsesEnhancedThresholds()
        {
            _system.SetEltDefeated();
            _system.AddErosion(85f); // Critical (enhanced)
            _system.OnBattleStart();

            _system.PurifyInBattle();

            Assert.AreEqual(ErosionStage.Dangerous, _system.CurrentStage);
            Assert.AreEqual(ErosionSystem.DANGEROUS_THRESHOLD_ENHANCED, _system.CurrentErosion);
        }
    }
}
