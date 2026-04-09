using NUnit.Framework;
using System.Collections.Generic;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class BattleSetupDataTests
    {
        [Test]
        public void Constructor_SetsProperties()
        {
            var party = new List<CharacterStats> { new CharacterStats("Hero", 100, 50, 20, 15, 10) };
            var enemies = new List<CharacterStats> { new CharacterStats("Slime", 50, 10, 10, 5, 5) };

            var setup = new BattleSetupData(party, enemies, "Field");

            Assert.AreEqual(1, setup.Party.Count);
            Assert.AreEqual(1, setup.Enemies.Count);
            Assert.AreEqual("Field", setup.ReturnSceneName);
            Assert.IsTrue(setup.CanFlee);
            Assert.IsFalse(setup.IsBossBattle);
        }

        [Test]
        public void Constructor_NullParty_Throws()
        {
            var enemies = new List<CharacterStats> { new CharacterStats("Slime", 50, 10, 10, 5, 5) };
            Assert.Throws<System.ArgumentNullException>(() => new BattleSetupData(null, enemies, "Field"));
        }

        [Test]
        public void Constructor_NullEnemies_Throws()
        {
            var party = new List<CharacterStats> { new CharacterStats("Hero", 100, 50, 20, 15, 10) };
            Assert.Throws<System.ArgumentNullException>(() => new BattleSetupData(party, null, "Field"));
        }

        [Test]
        public void Constructor_NullReturnScene_Throws()
        {
            var party = new List<CharacterStats> { new CharacterStats("Hero", 100, 50, 20, 15, 10) };
            var enemies = new List<CharacterStats> { new CharacterStats("Slime", 50, 10, 10, 5, 5) };
            Assert.Throws<System.ArgumentNullException>(() => new BattleSetupData(party, enemies, null));
        }

        [Test]
        public void StaticCurrent_SetAndGet()
        {
            var party = new List<CharacterStats> { new CharacterStats("Hero", 100, 50, 20, 15, 10) };
            var enemies = new List<CharacterStats> { new CharacterStats("Slime", 50, 10, 10, 5, 5) };
            var setup = new BattleSetupData(party, enemies, "Field");

            BattleSetupData.Current = setup;
            Assert.AreSame(setup, BattleSetupData.Current);

            BattleSetupData.Current = null;
            Assert.IsNull(BattleSetupData.Current);
        }

        [Test]
        public void BossBattle_CannotFlee()
        {
            var party = new List<CharacterStats> { new CharacterStats("Hero", 100, 50, 20, 15, 10) };
            var enemies = new List<CharacterStats> { new CharacterStats("Boss", 500, 100, 50, 30, 20) };
            var setup = new BattleSetupData(party, enemies, "Field")
            {
                IsBossBattle = true,
                CanFlee = false
            };

            Assert.IsTrue(setup.IsBossBattle);
            Assert.IsFalse(setup.CanFlee);
        }

        [Test]
        public void DefaultEnemySkills_IsEmptyList()
        {
            var party = new List<CharacterStats> { new CharacterStats("Hero", 100, 50, 20, 15, 10) };
            var enemies = new List<CharacterStats> { new CharacterStats("Slime", 50, 10, 10, 5, 5) };
            var setup = new BattleSetupData(party, enemies, "Field");

            Assert.IsNotNull(setup.EnemySkills);
            Assert.AreEqual(0, setup.EnemySkills.Count);
        }
    }
}
