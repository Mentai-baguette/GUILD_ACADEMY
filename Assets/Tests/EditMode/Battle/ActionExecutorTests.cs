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
            // SkillPower=150: (30×2×150/100) - 10 + 50(variance) = 130, DEX=0 → no crit
            Assert.AreEqual(130, result.DamageDealt);
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

        [Test]
        public void Execute_MagicAttack_UsesIntAndRes()
        {
            // attacker: ATK=30, INT=50 / target: DEF=10, RES=5
            // IsMagic=true → INT(50) vs RES(5): base = 50*2 - 5 = 95
            // IsMagic=false → ATK(30) vs DEF(10): base = 30*2 - 10 = 50
            var magicAttacker = new CharacterStats("Mage", 200, 50, 30, 20, 10,
                intStat: 50, res: 0, dex: 0);
            var magicTarget = new CharacterStats("Target", 500, 50, 15, 10, 8,
                intStat: 0, res: 5, dex: 0);
            _breakSystem.Register(magicAttacker);
            _breakSystem.Register(magicTarget);

            var random = new FixedRandom(0);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var cmdMagic = new BattleCommand
            {
                Attacker = magicAttacker,
                Target = magicTarget,
                Type = CommandType.Attack,
                Element = ElementType.None,
                IsMagic = true
            };

            var cmdPhysical = new BattleCommand
            {
                Attacker = magicAttacker,
                Target = magicTarget,
                Type = CommandType.Attack,
                Element = ElementType.None,
                IsMagic = false
            };

            var magicResult = executor.Execute(cmdMagic);
            var physicalResult = executor.Execute(cmdPhysical);

            // INT(50) vs RES(5) should deal more damage than ATK(30) vs DEF(10)
            Assert.Greater(magicResult.DamageDealt, physicalResult.DamageDealt);
        }

        [Test]
        public void Execute_PhysicalAttack_UsesAtkAndDef()
        {
            // IsMagic=false (default) → ATK vs DEF
            // attacker: ATK=30, INT=0 / target: DEF=10, RES=100
            // Physical: ATK(30) vs DEF(10) = 30*2 - 10 = 50 → damage > 0
            // If it mistakenly used INT/RES: INT(0) vs RES(100) → 0 - 100 → min damage
            var physAttacker = new CharacterStats("Fighter", 200, 50, 30, 20, 10,
                intStat: 0, res: 0, dex: 0);
            var physTarget = new CharacterStats("Target", 500, 50, 15, 10, 8,
                intStat: 0, res: 100, dex: 0);
            _breakSystem.Register(physAttacker);
            _breakSystem.Register(physTarget);

            var random = new FixedRandom(0);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var command = new BattleCommand
            {
                Attacker = physAttacker,
                Target = physTarget,
                Type = CommandType.Attack,
                Element = ElementType.None,
                IsMagic = false
            };

            var result = executor.Execute(command);

            // ATK(30) vs DEF(10) → base=50, should be well above minimum
            Assert.Greater(result.DamageDealt, 1);
        }

        [Test]
        public void Execute_HighDex_HigherCriticalChance()
        {
            // DEX=200 → critChance = min(200/2, 100) = 100%
            // FixedRandom(0) → Range(0,100) returns 0 → 0 < 100 → critical
            var highDexAttacker = new CharacterStats("Assassin", 200, 50, 30, 20, 10,
                intStat: 0, res: 0, dex: 200);
            var target = new CharacterStats("Target", 500, 50, 15, 10, 8);
            _breakSystem.Register(highDexAttacker);
            _breakSystem.Register(target);

            var random = new FixedRandom(0);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var command = new BattleCommand
            {
                Attacker = highDexAttacker,
                Target = target,
                Type = CommandType.Attack,
                Element = ElementType.None
            };

            var result = executor.Execute(command);
            Assert.IsTrue(result.WasCritical);
        }

        [Test]
        public void Execute_ZeroDex_NoCritical()
        {
            // DEX=0 → critChance = min(0/2, 100) = 0%
            // FixedRandom(0) → Range(0,100) returns 0 → 0 < 0 → false → no critical
            var zeroDexAttacker = new CharacterStats("Slowpoke", 200, 50, 30, 20, 10,
                intStat: 0, res: 0, dex: 0);
            var target = new CharacterStats("Target", 500, 50, 15, 10, 8);
            _breakSystem.Register(zeroDexAttacker);
            _breakSystem.Register(target);

            var random = new FixedRandom(0);
            var damageCalc = new DamageCalculator(random);
            var executor = new ActionExecutor(damageCalc, _breakSystem, random);

            var command = new BattleCommand
            {
                Attacker = zeroDexAttacker,
                Target = target,
                Type = CommandType.Attack,
                Element = ElementType.None
            };

            var result = executor.Execute(command);
            Assert.IsFalse(result.WasCritical);
        }
    }
}
