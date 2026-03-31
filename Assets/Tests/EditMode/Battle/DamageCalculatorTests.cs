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

        [Test]
        public void Calculate_BasicDamage_AtkMinusDef()
        {
            int damage = _calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(30, damage);
        }

        [Test]
        public void Calculate_WithVariance_AddedToDamage()
        {
            var calc = new DamageCalculator(new FixedRandom(5));
            int damage = calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(35, damage);
        }

        [Test]
        public void Calculate_DefGreaterThanAtk_MinimumDamage1()
        {
            int damage = _calc.Calculate(10, 50, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(1, damage);
        }

        [Test]
        public void Calculate_AtkEqualsDefZeroVariance_MinimumDamage1()
        {
            int damage = _calc.Calculate(30, 30, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(1, damage);
        }

        [Test]
        public void Calculate_WeakElement_Multiplier1Point5()
        {
            int damage = _calc.Calculate(50, 20, ElementType.Fire, ElementType.Fire, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(45, damage);
        }

        [Test]
        public void Calculate_ResistElement_Multiplier0Point5()
        {
            int damage = _calc.Calculate(50, 20, ElementType.Ice, ElementType.None, ElementType.Ice, ElementType.None, false, false);
            Assert.AreEqual(15, damage);
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
            int damage = _calc.Calculate(50, 20, ElementType.Fire, ElementType.Ice, ElementType.Wind, ElementType.Earth, false, false);
            Assert.AreEqual(30, damage);
        }

        [Test]
        public void Calculate_CriticalHit_Multiplier1Point5()
        {
            int damage = _calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, true, false);
            Assert.AreEqual(45, damage);
        }

        [Test]
        public void Calculate_BreakState_Multiplier2()
        {
            int damage = _calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, true);
            Assert.AreEqual(60, damage);
        }

        [Test]
        public void Calculate_CriticalAndBreak_StackMultiplicatively()
        {
            int damage = _calc.Calculate(50, 20, ElementType.None, ElementType.None, ElementType.None, ElementType.None, true, true);
            Assert.AreEqual(90, damage);
        }

        [Test]
        public void Calculate_WeakAndCritical_StackMultiplicatively()
        {
            int damage = _calc.Calculate(50, 20, ElementType.Fire, ElementType.Fire, ElementType.None, ElementType.None, true, false);
            Assert.AreEqual(67, damage);
        }

        [Test]
        public void Calculate_AllMultipliers_WeakCritBreak()
        {
            int damage = _calc.Calculate(50, 20, ElementType.Fire, ElementType.Fire, ElementType.None, ElementType.None, true, true);
            Assert.AreEqual(135, damage);
        }

        [Test]
        public void Calculate_LowDamageWithResist_StillMinimum1()
        {
            int damage = _calc.Calculate(22, 20, ElementType.Ice, ElementType.None, ElementType.Ice, ElementType.None, false, false);
            Assert.AreEqual(1, damage);
        }

        [Test]
        public void Calculate_ZeroAtk_StillMinimum1()
        {
            int damage = _calc.Calculate(0, 0, ElementType.None, ElementType.None, ElementType.None, ElementType.None, false, false);
            Assert.AreEqual(1, damage);
        }

        [Test]
        public void GetElementMultiplier_NoneAttackElement_Returns1()
        {
            float mult = DamageCalculator.GetElementMultiplier(ElementType.None, ElementType.Fire, ElementType.Ice, ElementType.Dark);
            Assert.AreEqual(1.0f, mult);
        }
    }
}
