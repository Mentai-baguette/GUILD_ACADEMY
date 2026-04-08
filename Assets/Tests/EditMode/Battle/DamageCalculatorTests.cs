using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.Tests.EditMode.TestHelpers;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class DamageCalculatorTests
    {
        private DamageCalculator _calc;

        [SetUp]
        public void SetUp()
        {
            _calc = new DamageCalculator(new FixedRandom(0));
        }

        // --- Basic Damage (ATK×2 - DEF formula) ---

        [Test]
        public void Calculate_BasicDamage_AtkTimes2MinusDef()
        {
            // 50×2 - 20 + 0(variance) = 80
            int damage = _calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(80, damage);
        }

        [Test]
        public void Calculate_WithVariance_AddedToDamage()
        {
            var calc = new DamageCalculator(new FixedRandom(5));
            // 50×2 - 20 + 5 = 85
            int damage = calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(85, damage);
        }

        [Test]
        public void Calculate_DefGreaterThanDoubleAtk_MinimumDamage1()
        {
            // 10×2 - 50 + 0 = -30 → min 1
            int damage = _calc.Calculate(10, 50, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(1, damage);
        }

        [Test]
        public void Calculate_DoubleAtkEqualsDefZeroVariance_MinimumDamage1()
        {
            // 15×2 - 30 + 0 = 0 → min 1
            int damage = _calc.Calculate(15, 30, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(1, damage);
        }

        // --- Element Multipliers ---

        [Test]
        public void Calculate_WeakElement_Multiplier1Point5()
        {
            // 50×2 - 20 = 80 × 1.5 = 120
            int damage = _calc.Calculate(50, 20, ElementType.Fire, ElementType.Fire, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(120, damage);
        }

        [Test]
        public void Calculate_ResistElement_Multiplier0Point5()
        {
            // 50×2 - 20 = 80 × 0.5 = 40
            int damage = _calc.Calculate(50, 20, ElementType.Ice, ElementType.None, ElementType.Ice, ElementType.None, false, false);
            Assert.AreEqual(40, damage);
        }

        [Test]
        public void Calculate_NullElement_Damage0()
        {
            int damage = _calc.Calculate(50, 20, ElementType.Dark, ElementType.None, ElementType.None, ElementType.Dark, false, false);
            Assert.AreEqual(0, damage);
        }

        [Test]
        public void Calculate_NormalElement_Multiplier1()
        {
            // 50×2 - 20 = 80
            int damage = _calc.Calculate(50, 20, ElementType.Fire, ElementType.Ice, ElementType.Wind, ElementType.Earth, false, false);
            Assert.AreEqual(80, damage);
        }

        // --- Critical & Break ---

        [Test]
        public void Calculate_CriticalHit_Multiplier1Point5()
        {
            // 80 × 1.5 = 120
            int damage = _calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, true, false);
            Assert.AreEqual(120, damage);
        }

        [Test]
        public void Calculate_BreakState_Multiplier2()
        {
            // 80 × 2.0 = 160
            int damage = _calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, true);
            Assert.AreEqual(160, damage);
        }

        [Test]
        public void Calculate_CriticalAndBreak_StackMultiplicatively()
        {
            // 80 × 1.5 × 2.0 = 240
            int damage = _calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, true, true);
            Assert.AreEqual(240, damage);
        }

        [Test]
        public void Calculate_WeakAndCritical_StackMultiplicatively()
        {
            // 80 × 1.5(weak) × 1.5(crit) = 180
            int damage = _calc.Calculate(50, 20, ElementType.Fire, ElementType.Fire, ElementType.None, ElementType.None, true, false);
            Assert.AreEqual(180, damage);
        }

        [Test]
        public void Calculate_AllMultipliers_WeakCritBreak()
        {
            // 80 × 1.5 × 1.5 × 2.0 = 360
            int damage = _calc.Calculate(50, 20, ElementType.Fire, ElementType.Fire, ElementType.None, ElementType.None, true, true);
            Assert.AreEqual(360, damage);
        }

        [Test]
        public void Calculate_LowDamageWithResist_StillMinimum1()
        {
            // 22×2 - 20 = 24 × 0.5 = 12
            int damage = _calc.Calculate(22, 20, ElementType.Ice, ElementType.None, ElementType.Ice, ElementType.None, false, false);
            Assert.AreEqual(12, damage);
        }

        [Test]
        public void Calculate_ZeroAtk_StillMinimum1()
        {
            // 0×2 - 0 + 0 = 0 → min 1
            int damage = _calc.Calculate(0, 0, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(1, damage);
        }

        [Test]
        public void GetElementMultiplier_NoneAttackElement_Returns1()
        {
            float mult = DamageCalculator.GetElementMultiplier(ElementType.None, ElementType.Fire, ElementType.Ice, ElementType.Dark);
            Assert.AreEqual(1.0f, mult);
        }

        // --- Skill Power ---

        [Test]
        public void Calculate_WithSkillPower150_DealsMoreDamage()
        {
            // (50×2 × 150/100) - 20 + 0 = 130
            int damage = _calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false, skillPower: 150);
            Assert.AreEqual(130, damage);
        }

        [Test]
        public void Calculate_WithSkillPower50_DealsLessDamage()
        {
            // (50×2 × 50/100) - 20 + 0 = 30
            int damage = _calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false, skillPower: 50);
            Assert.AreEqual(30, damage);
        }

        [Test]
        public void Calculate_SkillPowerZero_SameAsBasicAttack()
        {
            var calcA = new DamageCalculator(new FixedRandom(0));
            var calcB = new DamageCalculator(new FixedRandom(0));
            int withoutSkill = calcA.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            int withZeroSkill = calcB.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false, skillPower: 0);
            Assert.AreEqual(withoutSkill, withZeroSkill);
        }

        // --- Heal (INT×2 based) ---

        [Test]
        public void CalculateHeal_BasicHeal_ReturnsPositive()
        {
            var calc = new DamageCalculator(new FixedRandom(0));
            // 20×2 × 100/100 + 0 = 40
            int heal = calc.CalculateHeal(20, 100);
            Assert.AreEqual(40, heal);
        }

        [Test]
        public void CalculateHeal_MinimumHeal_IsOne()
        {
            var calc = new DamageCalculator(new FixedRandom(0));
            // max(1, 1×2 × 1/100 + 0) = max(1, 0) = 1
            int heal = calc.CalculateHeal(1, 1);
            Assert.AreEqual(1, heal);
        }

        [Test]
        public void CalculateHeal_HighPower_ScalesWithInt()
        {
            var calc = new DamageCalculator(new FixedRandom(0));
            // 50×2 × 200/100 + 0 = 200
            int heal = calc.CalculateHeal(50, 200);
            Assert.AreEqual(200, heal);
        }

        // --- Critical Chance (DEX based) ---

        [Test]
        public void GetCriticalChancePercent_Dex0_Returns0()
        {
            Assert.AreEqual(0, DamageCalculator.GetCriticalChancePercent(0));
        }

        [Test]
        public void GetCriticalChancePercent_Dex10_Returns5()
        {
            Assert.AreEqual(5, DamageCalculator.GetCriticalChancePercent(10));
        }

        [Test]
        public void GetCriticalChancePercent_Dex100_Returns50()
        {
            Assert.AreEqual(50, DamageCalculator.GetCriticalChancePercent(100));
        }

        [Test]
        public void GetCriticalChancePercent_Dex200_Returns100()
        {
            Assert.AreEqual(100, DamageCalculator.GetCriticalChancePercent(200));
        }

        [Test]
        public void GetCriticalChancePercent_Dex300_CapsAt100()
        {
            Assert.AreEqual(100, DamageCalculator.GetCriticalChancePercent(300));
        }
    }
}
