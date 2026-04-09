using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.Tests.EditMode.TestHelpers;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class StatusEffectSystemTests
    {
        private StatusEffectSystem _system;
        private CharacterStats _target;
        private IRandom _alwaysSucceed; // roll=0 → 常に成功

        [SetUp]
        public void SetUp()
        {
            _system = new StatusEffectSystem();
            _target = new CharacterStats("TestChar", 1000, 100, 50, 30, 20);
            _alwaysSucceed = new FixedRandom(0);
        }

        // ===== 毒 =====

        [Test]
        public void Poison_Apply_Succeeds()
        {
            bool result = _system.TryApply(_target, StatusEffectType.Poison, 100, 0f, _alwaysSucceed);

            Assert.IsTrue(result);
            Assert.IsTrue(_system.HasEffect(_target, StatusEffectType.Poison));
        }

        [Test]
        public void Poison_DealsDamage_8PercentPerTurn()
        {
            _system.TryApply(_target, StatusEffectType.Poison, 100, 0f, _alwaysSucceed);
            int expectedDamage = 80; // MaxHp(1000) * 8%

            _system.OnTurnEnd(_target);

            Assert.AreEqual(1000 - expectedDamage, _target.CurrentHp);
        }

        [Test]
        public void Poison_PersistsUntilBattleEnd()
        {
            _system.TryApply(_target, StatusEffectType.Poison, 100, 0f, _alwaysSucceed);

            // 10ターン経過しても毒は残る
            for (int i = 0; i < 10; i++)
            {
                _system.OnTurnEnd(_target);
            }

            Assert.IsTrue(_system.HasEffect(_target, StatusEffectType.Poison));
        }

        // ===== 麻痺 =====

        [Test]
        public void Paralysis_Apply_Succeeds()
        {
            bool result = _system.TryApply(_target, StatusEffectType.Paralysis, 100, 0f, _alwaysSucceed);

            Assert.IsTrue(result);
            Assert.IsTrue(_system.HasEffect(_target, StatusEffectType.Paralysis));
        }

        [Test]
        public void Paralysis_BlocksAction_WhenRollBelow30()
        {
            _system.TryApply(_target, StatusEffectType.Paralysis, 100, 0f, _alwaysSucceed);
            var lowRandom = new FixedRandom(15); // 15 < 30 → 行動不能

            bool blocked = _system.IsActionBlocked(_target, lowRandom);

            Assert.IsTrue(blocked);
        }

        [Test]
        public void Paralysis_AllowsAction_WhenRollAbove30()
        {
            _system.TryApply(_target, StatusEffectType.Paralysis, 100, 0f, _alwaysSucceed);
            var highRandom = new FixedRandom(50); // 50 >= 30 → 行動可能

            bool blocked = _system.IsActionBlocked(_target, highRandom);

            Assert.IsFalse(blocked);
        }

        [Test]
        public void Paralysis_NoBlock_WhenNotApplied()
        {
            bool blocked = _system.IsActionBlocked(_target, _alwaysSucceed);

            Assert.IsFalse(blocked);
        }

        // ===== 暗闇 =====

        [Test]
        public void Blindness_ReducesPhysicalHit_By50Percent()
        {
            _system.TryApply(_target, StatusEffectType.Blindness, 100, 0f, _alwaysSucceed);

            float modifier = _system.GetPhysicalHitModifier(_target);

            Assert.AreEqual(-0.5f, modifier, 0.001f);
        }

        [Test]
        public void Blindness_NoReduction_WhenNotApplied()
        {
            float modifier = _system.GetPhysicalHitModifier(_target);

            Assert.AreEqual(0f, modifier, 0.001f);
        }

        // ===== 沈黙 =====

        [Test]
        public void Silence_BlocksSkills()
        {
            _system.TryApply(_target, StatusEffectType.Silence, 100, 0f, _alwaysSucceed);

            bool blocked = _system.IsSkillBlocked(_target);

            Assert.IsTrue(blocked);
        }

        [Test]
        public void Silence_NoBlock_WhenNotApplied()
        {
            bool blocked = _system.IsSkillBlocked(_target);

            Assert.IsFalse(blocked);
        }

        // ===== スロウ =====

        [Test]
        public void Slow_ReducesAtbSpeed_By50Percent()
        {
            _system.TryApply(_target, StatusEffectType.Slow, 100, 0f, _alwaysSucceed);

            float multiplier = _system.GetAtbSpeedMultiplier(_target);

            Assert.AreEqual(0.5f, multiplier, 0.001f);
        }

        // ===== ストップ =====

        [Test]
        public void Stop_FreezesAtb()
        {
            _system.TryApply(_target, StatusEffectType.Stop, 100, 0f, _alwaysSucceed);

            float multiplier = _system.GetAtbSpeedMultiplier(_target);

            Assert.AreEqual(0f, multiplier, 0.001f);
        }

        [Test]
        public void Stop_LastsOnly2Turns()
        {
            _system.TryApply(_target, StatusEffectType.Stop, 100, 0f, _alwaysSucceed);

            _system.OnTurnEnd(_target);
            Assert.IsTrue(_system.HasEffect(_target, StatusEffectType.Stop));

            _system.OnTurnEnd(_target);
            Assert.IsFalse(_system.HasEffect(_target, StatusEffectType.Stop));
        }

        [Test]
        public void Stop_BossImmune_WithResistance1()
        {
            // ボスは耐性1.0 → ストップ完全無効
            bool result = _system.TryApply(_target, StatusEffectType.Stop, 100, 1.0f, _alwaysSucceed);

            Assert.IsFalse(result);
            Assert.IsFalse(_system.HasEffect(_target, StatusEffectType.Stop));
        }
        [Test]
        public void Poison_WithResistance1_EffectiveChanceZero_Fails()
        {
            // 毒+耐性1.0 → 成功率0%で無効（計算結果が0以下）
            bool result = _system.TryApply(_target, StatusEffectType.Poison, 100, 1.0f, _alwaysSucceed);

            Assert.IsFalse(result);
            Assert.IsFalse(_system.HasEffect(_target, StatusEffectType.Poison));
        }

        [Test]
        public void Poison_WithResistance05_SuccessRateHalved()
        {
            // 毒+耐性0.5 → 成功率半減（100 * 0.5 = 50%）
            // roll=0 → 0 < 50 → 成功
            bool result = _system.TryApply(_target, StatusEffectType.Poison, 100, 0.5f, _alwaysSucceed);

            Assert.IsTrue(result);
            Assert.IsTrue(_system.HasEffect(_target, StatusEffectType.Poison));
        }

        [Test]
        public void Poison_WithResistance05_HighRoll_Fails()
        {
            // 毒+耐性0.5 → 成功率50%
            // roll=60 → 60 >= 50 → 失敗
            var random = new FixedRandom(60);
            bool result = _system.TryApply(_target, StatusEffectType.Poison, 100, 0.5f, random);

            Assert.IsFalse(result);
        }


        // ===== バフ：ATKアップ =====

        [Test]
        public void AtkUp_Multiplier_1Point2()
        {
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed);

            float multiplier = _system.GetAtkMultiplier(_target);

            Assert.AreEqual(1.2f, multiplier, 0.001f);
        }

        // ===== バフ：DEFアップ =====

        [Test]
        public void DefUp_Multiplier_1Point2()
        {
            _system.TryApply(_target, StatusEffectType.DefUp, 100, 0f, _alwaysSucceed);

            float multiplier = _system.GetDefMultiplier(_target);

            Assert.AreEqual(1.2f, multiplier, 0.001f);
        }

        // ===== バフ：ヘイスト =====

        [Test]
        public void Haste_Multiplier_1Point3()
        {
            _system.TryApply(_target, StatusEffectType.Haste, 100, 0f, _alwaysSucceed);

            float multiplier = _system.GetAtbSpeedMultiplier(_target);

            Assert.AreEqual(1.3f, multiplier, 0.001f);
        }

        // ===== バフ：リジェネ =====

        [Test]
        public void Regen_Heals_3PercentPerTurn()
        {
            _target.CurrentHp = 500; // 半分に減らす
            _system.TryApply(_target, StatusEffectType.Regen, 100, 0f, _alwaysSucceed);
            int expectedHeal = 30; // MaxHp(1000) * 3%

            _system.OnTurnEnd(_target);

            Assert.AreEqual(500 + expectedHeal, _target.CurrentHp);
        }

        [Test]
        public void Regen_DoesNotExceedMaxHp()
        {
            _target.CurrentHp = 990;
            _system.TryApply(_target, StatusEffectType.Regen, 100, 0f, _alwaysSucceed);

            _system.OnTurnEnd(_target);

            Assert.AreEqual(1000, _target.CurrentHp);
        }

        // ===== ステデバフ =====

        [Test]
        public void AtkDown_Multiplier_0Point7()
        {
            _system.TryApply(_target, StatusEffectType.AtkDown, 100, 0f, _alwaysSucceed);

            float multiplier = _system.GetAtkMultiplier(_target);

            Assert.AreEqual(0.7f, multiplier, 0.001f);
        }

        [Test]
        public void DefDown_Multiplier_0Point7()
        {
            _system.TryApply(_target, StatusEffectType.DefDown, 100, 0f, _alwaysSucceed);

            float multiplier = _system.GetDefMultiplier(_target);

            Assert.AreEqual(0.7f, multiplier, 0.001f);
        }

        // ===== エンハンス2段階重ねがけ =====

        [Test]
        public void Enhanced_AtkUp_StacksTo2_Multiplier1Point44()
        {
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed);
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed, isEnhanced: true);

            float multiplier = _system.GetAtkMultiplier(_target);

            // 1.2 × 1.2 = 1.44
            Assert.AreEqual(1.44f, multiplier, 0.001f);
        }

        [Test]
        public void Enhanced_DoesNotExceed2Stacks()
        {
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed);
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed, isEnhanced: true);
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed, isEnhanced: true);

            var effects = _system.GetEffects(_target);
            var atkUp = effects.FirstOrDefault(e => e.Type == StatusEffectType.AtkUp);

            Assert.AreEqual(2, atkUp.StackCount);
        }

        // ===== 耐性 =====

        [Test]
        public void Resistance_ReducesSuccessRate()
        {
            // 基本成功率50%, 耐性0.5 → 有効成功率25%
            // roll=30 → 30 >= 25 → 失敗
            var random = new FixedRandom(30);
            bool result = _system.TryApply(_target, StatusEffectType.Poison, 50, 0.5f, random);

            Assert.IsFalse(result);
        }

        [Test]
        public void Resistance_Zero_FullSuccessRate()
        {
            // 基本成功率80%, 耐性0.0 → 有効成功率80%
            // roll=50 → 50 < 80 → 成功
            var random = new FixedRandom(50);
            bool result = _system.TryApply(_target, StatusEffectType.Poison, 80, 0f, random);

            Assert.IsTrue(result);
        }

        // ===== 持続ターン管理 =====

        [Test]
        public void BuffExpires_After3Turns()
        {
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed);

            _system.OnTurnEnd(_target);
            Assert.IsTrue(_system.HasEffect(_target, StatusEffectType.AtkUp));

            _system.OnTurnEnd(_target);
            Assert.IsTrue(_system.HasEffect(_target, StatusEffectType.AtkUp));

            _system.OnTurnEnd(_target);
            Assert.IsFalse(_system.HasEffect(_target, StatusEffectType.AtkUp));
        }

        // ===== 重ねがけ不可（エンハンスなし時） =====

        [Test]
        public void NonEnhanced_DoesNotStack()
        {
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed);
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed); // エンハンスなし

            var effects = _system.GetEffects(_target);
            var atkUp = effects.FirstOrDefault(e => e.Type == StatusEffectType.AtkUp);

            Assert.AreEqual(1, atkUp.StackCount);
            Assert.AreEqual(1.2f, _system.GetAtkMultiplier(_target), 0.001f);
        }

        // ===== バトル終了時の全クリア =====

        [Test]
        public void OnBattleEnd_ClearsAllEffects()
        {
            _system.TryApply(_target, StatusEffectType.Poison, 100, 0f, _alwaysSucceed);
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed);
            _system.TryApply(_target, StatusEffectType.Silence, 100, 0f, _alwaysSucceed);

            _system.OnBattleEnd();

            Assert.IsFalse(_system.HasEffect(_target, StatusEffectType.Poison));
            Assert.IsFalse(_system.HasEffect(_target, StatusEffectType.AtkUp));
            Assert.IsFalse(_system.HasEffect(_target, StatusEffectType.Silence));
        }

        // ===== Remove / RemoveAll =====

        [Test]
        public void Remove_SingleEffect()
        {
            _system.TryApply(_target, StatusEffectType.Poison, 100, 0f, _alwaysSucceed);
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed);

            _system.Remove(_target, StatusEffectType.Poison);

            Assert.IsFalse(_system.HasEffect(_target, StatusEffectType.Poison));
            Assert.IsTrue(_system.HasEffect(_target, StatusEffectType.AtkUp));
        }

        [Test]
        public void RemoveAll_ClearsTargetEffects()
        {
            _system.TryApply(_target, StatusEffectType.Poison, 100, 0f, _alwaysSucceed);
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed);

            _system.RemoveAll(_target);

            Assert.AreEqual(0, _system.GetEffects(_target).Count);
        }

        // ===== 複合判定 =====

        [Test]
        public void AtkUpAndDown_CombineMultipliers()
        {
            _system.TryApply(_target, StatusEffectType.AtkUp, 100, 0f, _alwaysSucceed);
            _system.TryApply(_target, StatusEffectType.AtkDown, 100, 0f, _alwaysSucceed);

            float multiplier = _system.GetAtkMultiplier(_target);

            // 1.2 × 0.7 = 0.84
            Assert.AreEqual(0.84f, multiplier, 0.001f);
        }

        [Test]
        public void NoEffects_DefaultMultiplier()
        {
            Assert.AreEqual(1.0f, _system.GetAtkMultiplier(_target), 0.001f);
            Assert.AreEqual(1.0f, _system.GetDefMultiplier(_target), 0.001f);
            Assert.AreEqual(1.0f, _system.GetAtbSpeedMultiplier(_target), 0.001f);
        }
    }
}
