using System.Collections.Generic;
using NUnit.Framework;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.Tests.EditMode.TestHelpers;

namespace GuildAcademy.Tests.EditMode.Battle
{
    [TestFixture]
    public class EnemyAITests
    {
        private BasicEnemyAI _ai;
        private BreakSystem _breakSystem;

        [SetUp]
        public void SetUp()
        {
            _ai = new BasicEnemyAI();
            _breakSystem = new BreakSystem();
        }

        [Test]
        public void TargetsLowestHpPartyMember_WhenNoSpecialConditions()
        {
            var actor = CreateActor(100, 100, 50);
            var memberA = CreatePartyMember("A", 80, ElementType.None);
            var memberB = CreatePartyMember("B", 30, ElementType.None);

            _breakSystem.Register(memberA);
            _breakSystem.Register(memberB);

            var context = CreateContext(actor, new List<CharacterStats> { memberA, memberB },
                new List<SkillData>(), new FixedRandom(50));

            var cmd = _ai.DecideAction(context);

            Assert.AreEqual(CommandType.Attack, cmd.Type);
            Assert.AreEqual(memberB, cmd.Target);
        }

        [Test]
        public void TargetsBreakStateEnemy_WhenAvailable()
        {
            var actor = CreateActor(100, 100, 50);
            var memberA = CreatePartyMember("A", 80, ElementType.None);
            var memberB = CreatePartyMember("B", 60, ElementType.None);

            _breakSystem.Register(memberA);
            _breakSystem.Register(memberB, 1);
            // Force memberB into break state
            _breakSystem.ApplyHit(memberB, true);

            var context = CreateContext(actor, new List<CharacterStats> { memberA, memberB },
                new List<SkillData>(), new FixedRandom(50));

            var cmd = _ai.DecideAction(context);

            Assert.AreEqual(memberB, cmd.Target);
        }

        [Test]
        public void UsesElementalSkill_WhenTargetHasWeakness()
        {
            var actor = CreateActor(100, 100, 50);
            var member = CreatePartyMember("A", 80, ElementType.Fire);

            _breakSystem.Register(member);

            var fireSkill = new SkillData
            {
                Name = "Fireball",
                Power = 50,
                MpCost = 10,
                Element = ElementType.Fire,
                IsHealing = false
            };

            var context = CreateContext(actor, new List<CharacterStats> { member },
                new List<SkillData> { fireSkill }, new FixedRandom(50));

            var cmd = _ai.DecideAction(context);

            Assert.AreEqual(CommandType.Skill, cmd.Type);
            Assert.AreEqual(ElementType.Fire, cmd.Element);
            Assert.AreEqual(member, cmd.Target);
        }

        [Test]
        public void DefendsWhenLowHp_AndRandomDecides()
        {
            // Actor with HP < 30%
            var actor = CreateActor(100, 100, 10);
            var member = CreatePartyMember("A", 80, ElementType.None);

            _breakSystem.Register(member);

            // FixedRandom(0) → roll < 20 triggers defend
            // No healing skill, so defend path is checked
            var context = CreateContext(actor, new List<CharacterStats> { member },
                new List<SkillData>(), new FixedRandom(0));

            var cmd = _ai.DecideAction(context);

            Assert.AreEqual(CommandType.Defend, cmd.Type);
            Assert.AreEqual(actor, cmd.Attacker);
            Assert.AreEqual(actor, cmd.Target);
        }

        [Test]
        public void HealsWhenLowHp_AndHasHealingSkill()
        {
            // Actor with HP < 30%
            var actor = CreateActor(100, 100, 10);
            var member = CreatePartyMember("A", 80, ElementType.None);

            _breakSystem.Register(member);

            var healSkill = new SkillData
            {
                Name = "Heal",
                Power = 30,
                MpCost = 5,
                Element = ElementType.None,
                IsHealing = true
            };

            // FixedRandom(0) → roll < 25 triggers heal
            var context = CreateContext(actor, new List<CharacterStats> { member },
                new List<SkillData> { healSkill }, new FixedRandom(0));

            var cmd = _ai.DecideAction(context);

            Assert.AreEqual(CommandType.Skill, cmd.Type);
            Assert.AreEqual(actor, cmd.Target);
            Assert.IsTrue(cmd.MpCost == 5);
        }

        [Test]
        public void NormalAttack_WhenNoSkillsMeetConditions()
        {
            var actor = CreateActor(100, 100, 80);
            var member = CreatePartyMember("A", 80, ElementType.None);

            _breakSystem.Register(member);

            // Skill element doesn't match weakness (None), no break, HP is fine
            var iceSkill = new SkillData
            {
                Name = "Ice",
                Power = 40,
                MpCost = 10,
                Element = ElementType.Ice,
                IsHealing = false
            };

            var context = CreateContext(actor, new List<CharacterStats> { member },
                new List<SkillData> { iceSkill }, new FixedRandom(50));

            var cmd = _ai.DecideAction(context);

            Assert.AreEqual(CommandType.Attack, cmd.Type);
            Assert.AreEqual(member, cmd.Target);
        }

        [Test]
        public void DoesNotUseSkill_WhenNotEnoughMp()
        {
            var actor = CreateActor(100, 100, 80);
            actor.CurrentMp = 0;
            var member = CreatePartyMember("A", 80, ElementType.Fire);

            _breakSystem.Register(member);

            var fireSkill = new SkillData
            {
                Name = "Fireball",
                Power = 50,
                MpCost = 10,
                Element = ElementType.Fire,
                IsHealing = false
            };

            var context = CreateContext(actor, new List<CharacterStats> { member },
                new List<SkillData> { fireSkill }, new FixedRandom(50));

            var cmd = _ai.DecideAction(context);

            // Should fall back to normal attack since not enough MP
            Assert.AreEqual(CommandType.Attack, cmd.Type);
        }

        [Test]
        public void ReturnsValidCommand_WithCorrectAttacker()
        {
            var actor = CreateActor(100, 100, 80);
            var member = CreatePartyMember("A", 80, ElementType.None);

            _breakSystem.Register(member);

            var context = CreateContext(actor, new List<CharacterStats> { member },
                new List<SkillData>(), new FixedRandom(50));

            var cmd = _ai.DecideAction(context);

            Assert.IsNotNull(cmd);
            Assert.AreEqual(actor, cmd.Attacker);
        }

        // --- Helper methods ---

        private CharacterStats CreateActor(int maxHp, int maxMp, int currentHp)
        {
            var actor = new CharacterStats("Enemy", maxHp, maxMp, 30, 10, 10, ElementType.None);
            actor.CurrentHp = currentHp;
            return actor;
        }

        private CharacterStats CreatePartyMember(string name, int currentHp, ElementType weakElement)
        {
            var member = new CharacterStats(name, 100, 50, 20, 10, 10, ElementType.None);
            member.CurrentHp = currentHp;
            member.WeakElement = weakElement;
            return member;
        }

        private EnemyAIContext CreateContext(CharacterStats actor, List<CharacterStats> party,
            List<SkillData> skills, IRandom random)
        {
            return new EnemyAIContext
            {
                Actor = actor,
                Party = party,
                Enemies = new List<CharacterStats> { actor },
                BreakSystem = _breakSystem,
                AvailableSkills = skills,
                Random = random
            };
        }
    }
}
