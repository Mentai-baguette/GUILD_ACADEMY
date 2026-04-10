using System.Collections.Generic;
using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class BattleResultCalculatorTests
    {
        private BattleResultCalculator _calculator;

        [SetUp]
        public void SetUp()
        {
            _calculator = new BattleResultCalculator();
        }

        // --- EXP Calculation ---

        [Test]
        public void Calculate_EXP_EnemyLvSumTimes10()
        {
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 1)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Slime", 30, 0, 10, 5, 5, lv: 3),
                new CharacterStats("Goblin", 50, 0, 15, 8, 7, lv: 5)
            };

            var result = _calculator.Calculate(party, enemies);

            // (3 + 5) × 10 = 80
            Assert.AreEqual(80, result.TotalEXP);
        }

        [Test]
        public void Calculate_EXP_SingleEnemy()
        {
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 1)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Dragon", 500, 100, 50, 40, 20, lv: 10)
            };

            var result = _calculator.Calculate(party, enemies);

            Assert.AreEqual(100, result.TotalEXP);
        }

        // --- Gold Calculation ---

        [Test]
        public void Calculate_Gold_EnemyLvSumTimes5()
        {
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 1)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Slime", 30, 0, 10, 5, 5, lv: 3),
                new CharacterStats("Goblin", 50, 0, 15, 8, 7, lv: 5)
            };

            var result = _calculator.Calculate(party, enemies);

            // (3 + 5) × 5 = 40
            Assert.AreEqual(40, result.TotalGold);
        }

        [Test]
        public void Calculate_Gold_SingleEnemy()
        {
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 1)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Dragon", 500, 100, 50, 40, 20, lv: 10)
            };

            var result = _calculator.Calculate(party, enemies);

            Assert.AreEqual(50, result.TotalGold);
        }

        // --- Level Up ---

        [Test]
        public void Calculate_LevelUp_WhenExpExceedsThreshold()
        {
            // Lv1のキャラに100EXPで閾値(100×1=100)ちょうど → LvUP
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 1)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Boss", 500, 100, 50, 40, 20, lv: 10)
            };

            var result = _calculator.Calculate(party, enemies);

            // EXP = 10 × 10 = 100, 閾値 = 100 × 1 = 100 → LvUP to Lv2
            Assert.IsTrue(result.LevelUps.ContainsKey("Hero"));
            Assert.AreEqual(1, result.LevelUps["Hero"].OldLevel);
            Assert.AreEqual(2, result.LevelUps["Hero"].NewLevel);
        }

        [Test]
        public void Calculate_NoLevelUp_WhenExpBelowThreshold()
        {
            // Lv5のキャラに80EXP → 閾値(100×5=500)に達しない
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 5)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Slime", 30, 0, 10, 5, 5, lv: 3),
                new CharacterStats("Goblin", 50, 0, 15, 8, 7, lv: 5)
            };

            var result = _calculator.Calculate(party, enemies);

            Assert.IsFalse(result.LevelUps.ContainsKey("Hero"));
        }

        [Test]
        public void Calculate_MultipleLevelUps_WhenExpIsVeryHigh()
        {
            // Lv1, Exp0 → 敵Lv合計30 → EXP300
            // Lv1閾値=100 → Lv2(残200), Lv2閾値=200 → Lv3(残0)
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 1)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Boss", 999, 100, 50, 40, 20, lv: 30)
            };

            var result = _calculator.Calculate(party, enemies);

            Assert.IsTrue(result.LevelUps.ContainsKey("Hero"));
            Assert.AreEqual(1, result.LevelUps["Hero"].OldLevel);
            Assert.AreEqual(3, result.LevelUps["Hero"].NewLevel);
        }

        // --- SP Calculation ---

        [Test]
        public void Calculate_SP_GainedOnEvenLevel()
        {
            // Lv1 → Lv2 (偶数) → SP+1
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 1)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Boss", 500, 100, 50, 40, 20, lv: 10)
            };

            var result = _calculator.Calculate(party, enemies);

            // Lv1 → Lv2 (偶数Lvで+1)
            Assert.AreEqual(1, result.TotalSP);
        }

        [Test]
        public void Calculate_SP_NotGainedOnOddLevel()
        {
            // Lv2 → EXP enough for Lv3 only (odd) → SP+0
            var hero = new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 2);
            hero.Exp = 0;
            var party = new List<CharacterStats> { hero };
            // 敵Lv合計 = 20 → EXP = 200, Lv2閾値=200 → ちょうどLv3
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Boss", 500, 100, 50, 40, 20, lv: 20)
            };

            var result = _calculator.Calculate(party, enemies);

            Assert.IsTrue(result.LevelUps.ContainsKey("Hero"));
            Assert.AreEqual(3, result.LevelUps["Hero"].NewLevel);
            Assert.AreEqual(0, result.TotalSP);
        }

        [Test]
        public void Calculate_SP_MultipleLevelsGiveCorrectSP()
        {
            // Lv1 → Lv3: Lv2(偶数+1), Lv3(奇数+0) → SP = 1
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 1)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Boss", 999, 100, 50, 40, 20, lv: 30)
            };

            var result = _calculator.Calculate(party, enemies);

            Assert.AreEqual(1, result.TotalSP);
        }

        // --- Multiple Party Members ---

        [Test]
        public void Calculate_MultiplePartyMembers_EachGetsEXP()
        {
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 1),
                new CharacterStats("Mage", 80, 80, 15, 10, 12, lv: 1)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Boss", 500, 100, 50, 40, 20, lv: 10)
            };

            var result = _calculator.Calculate(party, enemies);

            // 両方Lv1 → 100EXP → 両方LvUP
            Assert.IsTrue(result.LevelUps.ContainsKey("Hero"));
            Assert.IsTrue(result.LevelUps.ContainsKey("Mage"));
        }

        // --- DroppedItems default ---

        [Test]
        public void Calculate_DroppedItems_DefaultEmpty()
        {
            var party = new List<CharacterStats>
            {
                new CharacterStats("Hero", 100, 50, 30, 20, 10, lv: 1)
            };
            var enemies = new List<CharacterStats>
            {
                new CharacterStats("Slime", 30, 0, 10, 5, 5, lv: 1)
            };

            var result = _calculator.Calculate(party, enemies);

            Assert.IsNotNull(result.DroppedItems);
            Assert.AreEqual(0, result.DroppedItems.Count);
        }
    }
}
