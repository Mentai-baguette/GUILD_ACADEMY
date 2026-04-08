using System;
using NUnit.Framework;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Data
{
    [TestFixture]
    public class DifficultyManagerTests
    {
        [TestCase(Difficulty.Easy, 1.0f)]
        [TestCase(Difficulty.Normal, 1.5f)]
        [TestCase(Difficulty.Hard, 2.0f)]
        [TestCase(Difficulty.Nightmare, 4.0f)]
        public void EnemyStatMultiplier_ReturnsCorrectValue(Difficulty difficulty, float expected)
        {
            var settings = DifficultyManager.GetSettings(difficulty);
            Assert.AreEqual(expected, settings.EnemyStatMultiplier);
        }

        [TestCase(Difficulty.Easy, 1.2f)]
        [TestCase(Difficulty.Normal, 1.0f)]
        [TestCase(Difficulty.Hard, 0.9f)]
        [TestCase(Difficulty.Nightmare, 0.75f)]
        public void ExpMultiplier_ReturnsCorrectValue(Difficulty difficulty, float expected)
        {
            var settings = DifficultyManager.GetSettings(difficulty);
            Assert.AreEqual(expected, settings.ExpMultiplier);
        }

        [TestCase(Difficulty.Easy, SaveRestriction.Anywhere)]
        [TestCase(Difficulty.Normal, SaveRestriction.Anywhere)]
        [TestCase(Difficulty.Hard, SaveRestriction.DormOnly)]
        [TestCase(Difficulty.Nightmare, SaveRestriction.Disabled)]
        public void SaveRule_ReturnsCorrectRestriction(Difficulty difficulty, SaveRestriction expected)
        {
            var settings = DifficultyManager.GetSettings(difficulty);
            Assert.AreEqual(expected, settings.SaveRule);
        }

        [TestCase(Difficulty.Easy, DefeatPenalty.Retry)]
        [TestCase(Difficulty.Normal, DefeatPenalty.Retry)]
        [TestCase(Difficulty.Hard, DefeatPenalty.LastSave)]
        [TestCase(Difficulty.Nightmare, DefeatPenalty.ChapterStart)]
        public void MobDefeatPenalty_ReturnsCorrectPenalty(Difficulty difficulty, DefeatPenalty expected)
        {
            var settings = DifficultyManager.GetSettings(difficulty);
            Assert.AreEqual(expected, settings.MobDefeatPenalty);
        }

        [Test]
        public void GoddessBlessing_NightmareOnly_CanUse()
        {
            var manager = new DifficultyManager(Difficulty.Nightmare);
            Assert.IsTrue(manager.CanUseGoddessBlessing());
        }

        [Test]
        public void GoddessBlessing_Nightmare_UsedOnce_CannotUseAgain()
        {
            var manager = new DifficultyManager(Difficulty.Nightmare);
            manager.UseGoddessBlessing();
            Assert.IsFalse(manager.CanUseGoddessBlessing());
        }

        [Test]
        public void GoddessBlessing_Nightmare_UseAfterUsed_ThrowsException()
        {
            var manager = new DifficultyManager(Difficulty.Nightmare);
            manager.UseGoddessBlessing();
            Assert.Throws<InvalidOperationException>(() => manager.UseGoddessBlessing());
        }

        [Test]
        public void GoddessBlessing_Nightmare_ResetRestoresUsage()
        {
            var manager = new DifficultyManager(Difficulty.Nightmare);
            manager.UseGoddessBlessing();
            manager.ResetGoddessBlessing();
            Assert.IsTrue(manager.CanUseGoddessBlessing());
        }

        [TestCase(Difficulty.Easy)]
        [TestCase(Difficulty.Normal)]
        [TestCase(Difficulty.Hard)]
        public void GoddessBlessing_NonNightmare_CannotUse(Difficulty difficulty)
        {
            var manager = new DifficultyManager(difficulty);
            Assert.IsFalse(manager.CanUseGoddessBlessing());
        }

        [TestCase(Difficulty.Easy)]
        [TestCase(Difficulty.Normal)]
        [TestCase(Difficulty.Hard)]
        public void GoddessBlessing_NonNightmare_UseThrowsException(Difficulty difficulty)
        {
            var manager = new DifficultyManager(difficulty);
            Assert.Throws<InvalidOperationException>(() => manager.UseGoddessBlessing());
        }

        [Test]
        public void Constructor_SetsCurrentDifficultyAndSettings()
        {
            var manager = new DifficultyManager(Difficulty.Hard);
            Assert.AreEqual(Difficulty.Hard, manager.CurrentDifficulty);
            Assert.AreEqual(2.0f, manager.CurrentSettings.EnemyStatMultiplier);
        }
    }
}
