using System.Collections.Generic;
using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.Tests.EditMode.TestHelpers;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class BattleFlowControllerTests
    {
        private ATBSystem _atb;
        private BreakSystem _breakSystem;
        private ActionExecutor _executor;
        private BattleFlowController _controller;
        private FixedRandom _random;

        private CharacterStats _hero;
        private CharacterStats _enemy;

        [SetUp]
        public void SetUp()
        {
            _random = new FixedRandom(50);
            _atb = new ATBSystem();
            _breakSystem = new BreakSystem();
            var damageCalc = new DamageCalculator(_random);
            _executor = new ActionExecutor(damageCalc, _breakSystem, _random);
            _controller = new BattleFlowController(_atb, _executor, _breakSystem);

            _hero = new CharacterStats("Hero", 200, 50, 30, 20, 10);
            _enemy = new CharacterStats("Enemy", 100, 50, 15, 10, 8);
        }

        [Test]
        public void StartBattle_SetsCorrectState()
        {
            Assert.AreEqual(BattleFlowState.NotStarted, _controller.State);

            _controller.StartBattle(
                new List<CharacterStats> { _hero },
                new List<CharacterStats> { _enemy });

            Assert.AreEqual(BattleFlowState.TickingATB, _controller.State);
        }

        [Test]
        public void Tick_AdvancesATBGauges()
        {
            _controller.StartBattle(
                new List<CharacterStats> { _hero },
                new List<CharacterStats> { _enemy });

            _controller.Tick(0.5f);

            Assert.Greater(_atb.GetGauge(_hero), 0f);
            Assert.Greater(_atb.GetGauge(_enemy), 0f);
        }

        [Test]
        public void GetCurrentActor_WhenReady_ReturnsCharacter()
        {
            _controller.StartBattle(
                new List<CharacterStats> { _hero },
                new List<CharacterStats> { _enemy });

            // Tick enough to fill hero's gauge (Spd=10, GaugeRate=10, need 100)
            _controller.Tick(1.0f);

            var actor = _controller.GetCurrentActor();
            Assert.IsNotNull(actor);
        }

        [Test]
        public void GetCurrentActor_WhenNoneReady_ReturnsNull()
        {
            _controller.StartBattle(
                new List<CharacterStats> { _hero },
                new List<CharacterStats> { _enemy });

            // Tiny tick, nobody ready
            _controller.Tick(0.01f);

            var actor = _controller.GetCurrentActor();
            Assert.IsNull(actor);
        }

        [Test]
        public void SubmitCommand_ExecutesAndReturnsResult()
        {
            _controller.StartBattle(
                new List<CharacterStats> { _hero },
                new List<CharacterStats> { _enemy });

            _controller.Tick(1.0f);

            var command = new BattleCommand
            {
                Attacker = _hero,
                Target = _enemy,
                Type = CommandType.Attack,
                Element = ElementType.None
            };

            var result = _controller.SubmitCommand(command);
            Assert.IsNotNull(result);
            Assert.Greater(result.DamageDealt, 0);
            Assert.AreEqual(_hero, result.Attacker);
        }

        [Test]
        public void CheckBattleEnd_AllEnemiesDead_PlayerVictory()
        {
            _controller.StartBattle(
                new List<CharacterStats> { _hero },
                new List<CharacterStats> { _enemy });

            _enemy.CurrentHp = 0;

            var battleResult = _controller.CheckBattleEnd();
            Assert.AreEqual(BattleResult.PlayerVictory, battleResult);
        }

        [Test]
        public void CheckBattleEnd_AllPartyDead_EnemyVictory()
        {
            _controller.StartBattle(
                new List<CharacterStats> { _hero },
                new List<CharacterStats> { _enemy });

            _hero.CurrentHp = 0;

            var battleResult = _controller.CheckBattleEnd();
            Assert.AreEqual(BattleResult.EnemyVictory, battleResult);
        }

        [Test]
        public void CheckBattleEnd_BothAlive_ReturnsNone()
        {
            _controller.StartBattle(
                new List<CharacterStats> { _hero },
                new List<CharacterStats> { _enemy });

            var battleResult = _controller.CheckBattleEnd();
            Assert.AreEqual(BattleResult.None, battleResult);
        }

        [Test]
        public void GetCurrentActor_DeadCharacter_IsSkipped()
        {
            var hero2 = new CharacterStats("Hero2", 200, 50, 30, 20, 5);
            _controller.StartBattle(
                new List<CharacterStats> { _hero, hero2 },
                new List<CharacterStats> { _enemy });

            _controller.Tick(1.0f);
            _hero.CurrentHp = 0;

            var actor = _controller.GetCurrentActor();
            Assert.AreNotEqual(_hero, actor);
        }

        [Test]
        public void SubmitCommand_KillTarget_RemovesFromATB()
        {
            var weakEnemy = new CharacterStats("Weak", 1, 0, 5, 0, 5);
            _controller.StartBattle(
                new List<CharacterStats> { _hero },
                new List<CharacterStats> { weakEnemy });

            _controller.Tick(1.0f);

            var cmd = new BattleCommand
            {
                Attacker = _hero,
                Target = weakEnemy,
                Type = CommandType.Attack,
                Element = ElementType.None
            };

            var result = _controller.SubmitCommand(cmd);
            Assert.IsTrue(result.TargetDefeated);
            Assert.AreEqual(0f, _atb.GetGauge(weakEnemy));
        }
    }
}
