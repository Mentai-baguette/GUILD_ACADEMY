using NUnit.Framework;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Battle
{
    /// <summary>
    /// BattleUIManager関連のPure C#ロジックテスト。
    /// CommandType enumの拡張と、BattleCommand生成ロジックを検証する。
    /// </summary>
    [TestFixture]
    public class BattleUICommandTests
    {
        [Test]
        public void CommandType_HasAllRequiredValues()
        {
            // Verify all command types exist
            Assert.That(System.Enum.IsDefined(typeof(CommandType), CommandType.Attack), Is.True);
            Assert.That(System.Enum.IsDefined(typeof(CommandType), CommandType.Skill), Is.True);
            Assert.That(System.Enum.IsDefined(typeof(CommandType), CommandType.Item), Is.True);
            Assert.That(System.Enum.IsDefined(typeof(CommandType), CommandType.Defend), Is.True);
            Assert.That(System.Enum.IsDefined(typeof(CommandType), CommandType.Flee), Is.True);
            Assert.That(System.Enum.IsDefined(typeof(CommandType), CommandType.DualArts), Is.True);
            Assert.That(System.Enum.IsDefined(typeof(CommandType), CommandType.Change), Is.True);
            Assert.That(System.Enum.IsDefined(typeof(CommandType), CommandType.Swap), Is.True);
            Assert.That(System.Enum.IsDefined(typeof(CommandType), CommandType.Special), Is.True);
        }

        [Test]
        public void BattleCommand_AttackCommand_HasCorrectProperties()
        {
            var attacker = new CharacterStats("テスト戦士", 100, 50, 30, 20, 15, ElementType.Fire);
            var target = new CharacterStats("スライム", 50, 0, 10, 5, 8);

            var command = new BattleCommand
            {
                Attacker = attacker,
                Target = target,
                Type = CommandType.Attack,
                Element = attacker.Element
            };

            Assert.That(command.Attacker, Is.EqualTo(attacker));
            Assert.That(command.Target, Is.EqualTo(target));
            Assert.That(command.Type, Is.EqualTo(CommandType.Attack));
            Assert.That(command.Element, Is.EqualTo(ElementType.Fire));
            Assert.That(command.IsMagic, Is.False);
        }

        [Test]
        public void BattleCommand_DefendCommand_TargetsSelf()
        {
            var actor = new CharacterStats("テスト魔法使い", 80, 100, 15, 10, 12);

            var command = new BattleCommand
            {
                Attacker = actor,
                Target = actor,
                Type = CommandType.Defend
            };

            Assert.That(command.Attacker, Is.EqualTo(command.Target));
            Assert.That(command.Type, Is.EqualTo(CommandType.Defend));
        }

        [Test]
        public void BattleCommand_SkillCommand_HasSkillProperties()
        {
            var attacker = new CharacterStats("テスト魔法使い", 80, 100, 15, 10, 12, ElementType.Ice);
            var target = new CharacterStats("ゴブリン", 60, 0, 12, 8, 10);

            var command = new BattleCommand
            {
                Attacker = attacker,
                Target = target,
                Type = CommandType.Skill,
                Element = ElementType.Ice,
                SkillPower = 150,
                MpCost = 20,
                IsMagic = true
            };

            Assert.That(command.Type, Is.EqualTo(CommandType.Skill));
            Assert.That(command.Element, Is.EqualTo(ElementType.Ice));
            Assert.That(command.SkillPower, Is.EqualTo(150));
            Assert.That(command.MpCost, Is.EqualTo(20));
            Assert.That(command.IsMagic, Is.True);
        }

        [Test]
        public void BattleCommand_SwapCommand_TargetsAlly()
        {
            var actor = new CharacterStats("前衛", 100, 50, 30, 20, 15);
            var swapTarget = new CharacterStats("後衛", 70, 80, 20, 15, 18);

            var command = new BattleCommand
            {
                Attacker = actor,
                Target = swapTarget,
                Type = CommandType.Swap
            };

            Assert.That(command.Type, Is.EqualTo(CommandType.Swap));
            Assert.That(command.Attacker, Is.Not.EqualTo(command.Target));
        }

        [Test]
        public void ErosionGaugeStages_CorrectThresholds()
        {
            // Test the erosion gauge stage determination logic
            // Normal: 0-24%, Unstable: 25-49%, Dangerous: 50-74%, Critical: 75-100%
            Assert.That(GetErosionStage(0f), Is.EqualTo("Normal"));
            Assert.That(GetErosionStage(24f), Is.EqualTo("Normal"));
            Assert.That(GetErosionStage(25f), Is.EqualTo("Unstable"));
            Assert.That(GetErosionStage(49f), Is.EqualTo("Unstable"));
            Assert.That(GetErosionStage(50f), Is.EqualTo("Dangerous"));
            Assert.That(GetErosionStage(74f), Is.EqualTo("Dangerous"));
            Assert.That(GetErosionStage(75f), Is.EqualTo("Critical"));
            Assert.That(GetErosionStage(100f), Is.EqualTo("Critical"));
        }

        /// <summary>
        /// Mirrors the erosion stage logic from BattleUIManager.UpdateErosionGauge()
        /// </summary>
        private static string GetErosionStage(float value)
        {
            if (value < 25f) return "Normal";
            if (value < 50f) return "Unstable";
            if (value < 75f) return "Dangerous";
            return "Critical";
        }
    }
}
