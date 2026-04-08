using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.Tests.EditMode.TestHelpers;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class ActionExecutorTests
    {
        private BreakSystem _breakSystem;
        private CharacterStats _attacker;
        private CharacterStats _target;

        [SetUp]
        public void SetUp()
        {
            _breakSystem = new BreakSystem();
            _attacker = new CharacterStats("Hero", 200, 50, 30, 20, 10);
            _target = new CharacterStats("Enemy", 100, 50, 15, 10, 8);
            _target.WeakElement = ElementType.Fire;
            _breakSystem.Register(_attacker);
            _breakSystem.Register(_target);
        }

        [Test]
        public void Execute_BasicAttack_DealsDamage()
        {
            // DEX=0 (default) → critChance=0%, so no crit regardless of random value
            var random = new FixedRandom(50);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var command = new BattleCommand
            {
                Attacker = _attacker,
                Target = _target,
                Type = CommandType.Attack,
                Element = ElementType.None
            };

            var result = executor.Execute(command);
            Assert.Greater(result.DamageDealt, 0);
            Assert.AreEqual(_attacker, result.Attacker);
            Assert.AreEqual(_target, result.Target);
        }

        [Test]
        public void Execute_WeakHit_AppliesBreakGauge()
        {
            var random = new FixedRandom(50);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var command = new BattleCommand
            {
                Attacker = _attacker,
                Target = _target,
                Type = CommandType.Attack,
                Element = ElementType.Fire
            };

            var result = executor.Execute(command);
            Assert.IsTrue(result.WasWeakHit);
            Assert.Greater(_breakSystem.GetBreakGaugePercent(_target), 0f);
        }

        [Test]
        public void Execute_TargetInBreakState_DoubleDamage()
        {
            // First, break the target
            for (int i = 0; i < 4; i++)
                _breakSystem.ApplyHit(_target, isWeakElement: true);

            Assert.IsTrue(_breakSystem.IsBreaking(_target));

            // Use FixedRandom(50) for no crit, variance=50 (clamped by DamageCalculator to range)
            // Actually FixedRandom returns constant, so variance=50 and crit roll=50
            var random = new FixedRandom(0);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            // Non-break target for comparison
            var target2 = new CharacterStats("Enemy2", 1000, 50, 15, 10, 8);
            _breakSystem.Register(target2);

            var cmdBreak = new BattleCommand
            {
                Attacker = _attacker,
                Target = _target,
                Type = CommandType.Attack,
                Element = ElementType.None
            };

            var cmdNormal = new BattleCommand
            {
                Attacker = _attacker,
                Target = target2,
                Type = CommandType.Attack,
                Element = ElementType.None
            };

            var resultBreak = executor.Execute(cmdBreak);
            var resultNormal = executor.Execute(cmdNormal);

            // Break state doubles damage
            Assert.Greater(resultBreak.DamageDealt, resultNormal.DamageDealt);
        }

        [Test]
        public void Execute_KillsTarget_SetsTargetDefeated()
        {
            _target.CurrentHp = 1;
            var random = new FixedRandom(50);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var command = new BattleCommand
            {
                Attacker = _attacker,
                Target = _target,
                Type = CommandType.Attack,
                Element = ElementType.None
            };

            var result = executor.Execute(command);
            Assert.IsTrue(result.TargetDefeated);
            Assert.AreEqual(0, _target.CurrentHp);
        }

        [Test]
        public void Execute_MinimumDamage_AtLeastOne()
        {
            // High def, low atk scenario
            var weakAttacker = new CharacterStats("Weak", 100, 50, 1, 10, 10);
            var toughTarget = new CharacterStats("Tough", 1000, 50, 10, 100, 10);
            _breakSystem.Register(weakAttacker);
            _breakSystem.Register(toughTarget);

            var random = new FixedRandom(50);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var command = new BattleCommand
            {
                Attacker = weakAttacker,
                Target = toughTarget,
                Type = CommandType.Attack,
                Element = ElementType.None
            };

            var result = executor.Execute(command);
            Assert.GreaterOrEqual(result.DamageDealt, 1);
        }

        [Test]
        public void Execute_SkillWithPower_UsesSkillPower()
        {
            var random = new FixedRandom(50);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var command = new BattleCommand
            {
                Attacker = _attacker,
                Target = _target,
                Type = CommandType.Skill,
                Element = ElementType.None,
                SkillPower = 150
            };

            var result = executor.Execute(command);
            // SkillPower=150: (30×2×150/100) - 10 + 50(variance) = 80, DEX=0 → no crit
            Assert.Greater(result.DamageDealt, 0);
        }

        [Test]
        public void Execute_DefendCommand_DealsZeroDamage()
        {
            var attacker = new CharacterStats("Ray", 100, 50, 30, 20, 10);
            var target = new CharacterStats("Enemy", 100, 0, 20, 10, 5);
            _breakSystem.Register(target);
            var random = new FixedRandom(50);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var cmd = new BattleCommand
            {
                Attacker = attacker,
                Target = target,
                Type = CommandType.Defend,
                Element = ElementType.None
            };

            var result = executor.Execute(cmd);
            Assert.AreEqual(0, result.DamageDealt);
        }

        [Test]
        public void Execute_NullElement_ZeroDamage()
        {
            _target.NullElement = ElementType.Ice;

            var random = new FixedRandom(50);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var command = new BattleCommand
            {
                Attacker = _attacker,
                Target = _target,
                Type = CommandType.Attack,
                Element = ElementType.Ice
            };

            var result = executor.Execute(command);
            Assert.AreEqual(0, result.DamageDealt);
        }
    }
}
