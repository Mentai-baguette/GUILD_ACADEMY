using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class FormationSystemTests
    {
        private FormationSystem _formation;
        private CharacterStats _warrior;
        private CharacterStats _mage;

        [SetUp]
        public void SetUp()
        {
            _formation = new FormationSystem();
            _warrior = new CharacterStats("Warrior", 100, 20, 50, 30, 10);
            _mage = new CharacterStats("Mage", 60, 80, 20, 15, 8);
        }

        // --- 初期状態 ---

        [Test]
        public void GetRow_Default_ReturnsFront()
        {
            Assert.AreEqual(FormationRow.Front, _formation.GetRow(_warrior));
        }

        // --- 隊列設定/取得 ---

        [Test]
        public void SetRow_Back_ReturnsBack()
        {
            _formation.SetRow(_warrior, FormationRow.Back);
            Assert.AreEqual(FormationRow.Back, _formation.GetRow(_warrior));
        }

        [Test]
        public void SetRow_Front_ReturnsFront()
        {
            _formation.SetRow(_warrior, FormationRow.Back);
            _formation.SetRow(_warrior, FormationRow.Front);
            Assert.AreEqual(FormationRow.Front, _formation.GetRow(_warrior));
        }

        // --- チェンジ ---

        [Test]
        public void ChangeRow_FrontToBack()
        {
            _formation.SetRow(_warrior, FormationRow.Front);
            _formation.ChangeRow(_warrior);
            Assert.AreEqual(FormationRow.Back, _formation.GetRow(_warrior));
        }

        [Test]
        public void ChangeRow_BackToFront()
        {
            _formation.SetRow(_warrior, FormationRow.Back);
            _formation.ChangeRow(_warrior);
            Assert.AreEqual(FormationRow.Front, _formation.GetRow(_warrior));
        }

        // --- 攻撃補正 ---

        [Test]
        public void GetAttackModifier_FrontPhysical_Returns1()
        {
            _formation.SetRow(_warrior, FormationRow.Front);
            Assert.AreEqual(1.0f, _formation.GetAttackModifier(_warrior, isMagic: false));
        }

        [Test]
        public void GetAttackModifier_BackPhysical_Returns08()
        {
            _formation.SetRow(_warrior, FormationRow.Back);
            Assert.AreEqual(0.8f, _formation.GetAttackModifier(_warrior, isMagic: false));
        }

        [Test]
        public void GetAttackModifier_BackMagic_Returns1()
        {
            _formation.SetRow(_mage, FormationRow.Back);
            Assert.AreEqual(1.0f, _formation.GetAttackModifier(_mage, isMagic: true));
        }

        // --- 防御補正 ---

        [Test]
        public void GetDefenseModifier_FrontPhysical_Returns1()
        {
            _formation.SetRow(_warrior, FormationRow.Front);
            Assert.AreEqual(1.0f, _formation.GetDefenseModifier(_warrior, isMagic: false));
        }

        [Test]
        public void GetDefenseModifier_BackPhysical_Returns08()
        {
            _formation.SetRow(_warrior, FormationRow.Back);
            Assert.AreEqual(0.8f, _formation.GetDefenseModifier(_warrior, isMagic: false));
        }

        [Test]
        public void GetDefenseModifier_BackMagic_Returns1()
        {
            _formation.SetRow(_mage, FormationRow.Back);
            Assert.AreEqual(1.0f, _formation.GetDefenseModifier(_mage, isMagic: true));
        }

        // --- 未登録キャラのデフォルト ---

        [Test]
        public void UnregisteredCharacter_DefaultsFront_AttackModifier1()
        {
            var unknown = new CharacterStats("Unknown", 50, 10, 10, 10, 10);
            Assert.AreEqual(FormationRow.Front, _formation.GetRow(unknown));
            Assert.AreEqual(1.0f, _formation.GetAttackModifier(unknown, isMagic: false));
            Assert.AreEqual(1.0f, _formation.GetDefenseModifier(unknown, isMagic: false));
        }

        [Test]
        public void ChangeRow_UnregisteredCharacter_SwitchesToBack()
        {
            var unknown = new CharacterStats("Unknown", 50, 10, 10, 10, 10);
            _formation.ChangeRow(unknown);
            Assert.AreEqual(FormationRow.Back, _formation.GetRow(unknown));
        }
    }
}
