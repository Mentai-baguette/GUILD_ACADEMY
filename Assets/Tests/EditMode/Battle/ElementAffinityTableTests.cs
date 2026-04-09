using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class ElementAffinityTableTests
    {
        // --- None属性 ---

        [Test]
        public void GetMultiplier_NoneAttack_ReturnsNeutral()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.None, ElementType.Fire);
            Assert.AreEqual(1.0f, mult);
        }

        // --- 循環弱点: 火→氷→風→地→火 ---

        [Test]
        public void GetMultiplier_FireVsIce_Weak()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Fire, ElementType.Ice);
            Assert.AreEqual(1.5f, mult);
        }

        [Test]
        public void GetMultiplier_IceVsWind_Weak()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Ice, ElementType.Wind);
            Assert.AreEqual(1.5f, mult);
        }

        [Test]
        public void GetMultiplier_WindVsEarth_Weak()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Wind, ElementType.Earth);
            Assert.AreEqual(1.5f, mult);
        }

        [Test]
        public void GetMultiplier_EarthVsFire_Weak()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Earth, ElementType.Fire);
            Assert.AreEqual(1.5f, mult);
        }

        // --- 循環の逆方向は弱点にならない ---

        [Test]
        public void GetMultiplier_IceVsFire_Neutral()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Ice, ElementType.Fire);
            Assert.AreEqual(1.0f, mult);
        }

        [Test]
        public void GetMultiplier_FireVsWind_Neutral()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Fire, ElementType.Wind);
            Assert.AreEqual(1.0f, mult);
        }

        // --- 相互弱点: 光⇔闇 ---

        [Test]
        public void GetMultiplier_LightVsDark_Weak()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Light, ElementType.Dark);
            Assert.AreEqual(1.5f, mult);
        }

        [Test]
        public void GetMultiplier_DarkVsLight_Weak()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Dark, ElementType.Light);
            Assert.AreEqual(1.5f, mult);
        }

        // --- 同属性: 耐性 ---

        [Test]
        public void GetMultiplier_FireVsFire_Resist()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Fire, ElementType.Fire);
            Assert.AreEqual(0.5f, mult);
        }

        [Test]
        public void GetMultiplier_DarkVsDark_Resist()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Dark, ElementType.Dark);
            Assert.AreEqual(0.5f, mult);
        }

        // --- ボス専用: 吸収 ---

        [Test]
        public void GetMultiplier_WithAbsorb_ReturnsAbsorb()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Dark, ElementType.None, absorbElement: ElementType.Dark);
            Assert.AreEqual(-1.0f, mult);
        }

        [Test]
        public void GetMultiplier_AbsorbDifferentElement_NotAbsorbed()
        {
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Fire, ElementType.None, absorbElement: ElementType.Dark);
            Assert.AreEqual(1.0f, mult);
        }

        [Test]
        public void GetMultiplier_AbsorbTakesPriority_OverWeakness()
        {
            // 闇が光に対して弱点だが、吸収が優先
            float mult = ElementAffinityTable.GetMultiplier(ElementType.Dark, ElementType.Light, absorbElement: ElementType.Dark);
            Assert.AreEqual(-1.0f, mult);
        }

        // --- IsWeakAgainst ---

        [Test]
        public void IsWeakAgainst_FullCycle()
        {
            Assert.IsTrue(ElementAffinityTable.IsWeakAgainst(ElementType.Fire, ElementType.Ice));
            Assert.IsTrue(ElementAffinityTable.IsWeakAgainst(ElementType.Ice, ElementType.Wind));
            Assert.IsTrue(ElementAffinityTable.IsWeakAgainst(ElementType.Wind, ElementType.Earth));
            Assert.IsTrue(ElementAffinityTable.IsWeakAgainst(ElementType.Earth, ElementType.Fire));
        }

        [Test]
        public void IsWeakAgainst_LightDark_Mutual()
        {
            Assert.IsTrue(ElementAffinityTable.IsWeakAgainst(ElementType.Light, ElementType.Dark));
            Assert.IsTrue(ElementAffinityTable.IsWeakAgainst(ElementType.Dark, ElementType.Light));
        }

        [Test]
        public void IsWeakAgainst_None_NeverWeak()
        {
            Assert.IsFalse(ElementAffinityTable.IsWeakAgainst(ElementType.None, ElementType.Fire));
            Assert.IsFalse(ElementAffinityTable.IsWeakAgainst(ElementType.Fire, ElementType.None));
        }

        // --- GetWeakness ---

        [Test]
        public void GetWeakness_Fire_ReturnsEarth()
        {
            Assert.AreEqual(ElementType.Earth, ElementAffinityTable.GetWeakness(ElementType.Fire));
        }

        [Test]
        public void GetWeakness_Ice_ReturnsFire()
        {
            Assert.AreEqual(ElementType.Fire, ElementAffinityTable.GetWeakness(ElementType.Ice));
        }

        [Test]
        public void GetWeakness_Light_ReturnsDark()
        {
            Assert.AreEqual(ElementType.Dark, ElementAffinityTable.GetWeakness(ElementType.Light));
        }

        [Test]
        public void GetWeakness_None_ReturnsNone()
        {
            Assert.AreEqual(ElementType.None, ElementAffinityTable.GetWeakness(ElementType.None));
        }
    }
}
