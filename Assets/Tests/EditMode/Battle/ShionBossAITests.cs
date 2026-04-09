using NUnit.Framework;
using System;
using System.Collections.Generic;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.Tests.EditMode.TestHelpers;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class ShionBossAITests
    {
        private BossPhaseManager _phaseManager;
        private ShionBossAI _ai;

        private List<SkillData> _shionSkills;

        [SetUp]
        public void SetUp()
        {
            _phaseManager = new BossPhaseManager();

            var p1Stats = new CharacterStats("シオン", 5000, 300, 55, 40, 35, ElementType.Ice, intStat: 50, res: 40, dex: 38);
            var p2Stats = new CharacterStats("シオン", 7000, 400, 75, 55, 45, ElementType.Ice, intStat: 70, res: 55, dex: 45);
            var p3Stats = new CharacterStats("シオン", 9000, 500, 90, 65, 55, ElementType.Ice, intStat: 85, res: 65, dex: 55);

            _shionSkills = new List<SkillData>
            {
                new SkillData { Name = "氷結斬", Power = 80, MpCost = 15, Element = ElementType.Ice, TargetType = SkillTargetType.SingleEnemy, IsMagic = true },
                new SkillData { Name = "吹雪", Power = 60, MpCost = 25, Element = ElementType.Ice, TargetType = SkillTargetType.AllEnemies, IsMagic = true },
                new SkillData { Name = "絶対零度", Power = 150, MpCost = 50, Element = ElementType.Ice, TargetType = SkillTargetType.SingleEnemy, IsMagic = true }
            };

            _phaseManager.AddPhase(new BossPhaseData(1, 100f, p1Stats, _shionSkills));
            _phaseManager.AddPhase(new BossPhaseData(2, 50f, p2Stats, _shionSkills));
            _phaseManager.AddPhase(new BossPhaseData(3, 25f, p3Stats, _shionSkills));

            _ai = new ShionBossAI(_phaseManager);
        }

        private EnemyAIContext CreateContext(CharacterStats actor, int randomValue = 50)
        {
            return new EnemyAIContext
            {
                Actor = actor,
                Party = new List<CharacterStats>
                {
                    new CharacterStats("レイ", 450, 80, 42, 30, 28, ElementType.Dark),
                    new CharacterStats("ユナ", 380, 150, 28, 25, 22, ElementType.Light)
                },
                Enemies = new List<CharacterStats> { actor },
                BreakSystem = new BreakSystem(),
                AvailableSkills = _shionSkills,
                Random = new FixedRandom(randomValue)
            };
        }

        [Test]
        public void Phase1_NormalAttack_WhenRandomHigh()
        {
            var actor = new CharacterStats("シオン", 5000, 300, 55, 40, 35, ElementType.Ice);
            var context = CreateContext(actor, 80); // >30, so no ice skill
            var cmd = _ai.DecideAction(context);
            Assert.AreEqual(CommandType.Attack, cmd.Type);
        }

        [Test]
        public void Phase1_IceSkill_WhenRandomLow()
        {
            var actor = new CharacterStats("シオン", 5000, 300, 55, 40, 35, ElementType.Ice);
            var context = CreateContext(actor, 10); // <30, uses ice skill
            var cmd = _ai.DecideAction(context);
            Assert.AreEqual(CommandType.Skill, cmd.Type);
            Assert.AreEqual(ElementType.Ice, cmd.Element);
        }

        [Test]
        public void Phase2_MoreAggressive_UsesSkillsMoreOften()
        {
            var actor = new CharacterStats("シオン", 5000, 300, 55, 40, 35, ElementType.Ice);
            actor.CurrentHp = 2000; // 40% → Phase 2
            var context = CreateContext(actor, 30); // <50, uses AoE
            var cmd = _ai.DecideAction(context);
            Assert.AreEqual(CommandType.Skill, cmd.Type);
        }

        [Test]
        public void Phase3_VeryAggressive_UsesStrongestSkill()
        {
            var actor = new CharacterStats("シオン", 5000, 300, 55, 40, 35, ElementType.Ice);
            actor.CurrentHp = 1000; // 20% → Phase 3
            var context = CreateContext(actor, 30); // <70, uses strongest skill
            var cmd = _ai.DecideAction(context);
            Assert.AreEqual(CommandType.Skill, cmd.Type);
            Assert.AreEqual(150, cmd.SkillPower); // 絶対零度 = power 150
        }

        [Test]
        public void Constructor_NullPhaseManager_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ShionBossAI(null));
        }

        [Test]
        public void DecideAction_NullContext_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _ai.DecideAction(null));
        }

        [Test]
        public void PhaseTransition_OccursDuringDecision()
        {
            var actor = new CharacterStats("シオン", 5000, 300, 55, 40, 35, ElementType.Ice);
            actor.CurrentHp = 2000; // Will trigger phase 2

            var context = CreateContext(actor, 90);
            _ai.DecideAction(context);

            Assert.AreEqual(2, _phaseManager.CurrentPhase);
        }

        [Test]
        public void AllPartDead_ReturnsDefend()
        {
            var actor = new CharacterStats("シオン", 5000, 300, 55, 40, 35, ElementType.Ice);
            var context = new EnemyAIContext
            {
                Actor = actor,
                Party = new List<CharacterStats>
                {
                    new CharacterStats("Dead1", 100, 50, 20, 15, 10) { CurrentHp = 0 },
                    new CharacterStats("Dead2", 100, 50, 20, 15, 10) { CurrentHp = 0 }
                },
                Enemies = new List<CharacterStats> { actor },
                AvailableSkills = _shionSkills,
                Random = new FixedRandom(50)
            };
            var cmd = _ai.DecideAction(context);
            Assert.AreEqual(CommandType.Defend, cmd.Type);
        }
    }
}
