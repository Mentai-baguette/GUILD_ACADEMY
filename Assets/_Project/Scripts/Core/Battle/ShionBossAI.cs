using System;
using System.Collections.Generic;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class ShionBossAI : IEnemyAI
    {
        private readonly BossPhaseManager _phaseManager;

        public BossPhaseManager PhaseManager => _phaseManager;

        public ShionBossAI(BossPhaseManager phaseManager)
        {
            _phaseManager = phaseManager ?? throw new ArgumentNullException(nameof(phaseManager));
        }

        public BattleCommand DecideAction(EnemyAIContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.Actor == null) throw new ArgumentNullException("context.Actor");
            if (context.Party == null) throw new ArgumentNullException("context.Party");
            if (context.Random == null) throw new ArgumentNullException("context.Random");

            // Check phase transition
            _phaseManager.CheckPhaseTransition(context.Actor);

            var phase = _phaseManager.CurrentPhaseData;
            var skills = phase?.Skills ?? context.AvailableSkills ?? new List<SkillData>();
            var actor = context.Actor;
            var party = context.Party;
            var random = context.Random;

            int currentPhase = _phaseManager.CurrentPhase;

            switch (currentPhase)
            {
                case 1:
                    return Phase1Action(actor, party, skills, random);
                case 2:
                    return Phase2Action(actor, party, skills, random);
                case 3:
                    return Phase3Action(actor, party, skills, random);
                default:
                    return Phase1Action(actor, party, skills, random);
            }
        }

        // Phase 1 (Lv55): Conservative - normal attacks + occasional ice magic
        private BattleCommand Phase1Action(CharacterStats actor, List<CharacterStats> party,
            List<SkillData> skills, IRandom random)
        {
            var target = FindLowestHpTarget(party);
            if (target == null) return CreateDefend(actor);

            // 30% chance to use ice skill if available and has MP
            if (random.Range(0, 100) < 30)
            {
                var iceSkill = FindAffordableSkillByElement(skills, actor, ElementType.Ice);
                if (iceSkill != null)
                {
                    return CreateSkillCommand(actor, target, iceSkill);
                }
            }

            return CreateNormalAttack(actor, target);
        }

        // Phase 2 (Lv75): More aggressive - AoE skills, higher skill usage
        private BattleCommand Phase2Action(CharacterStats actor, List<CharacterStats> party,
            List<SkillData> skills, IRandom random)
        {
            var target = FindLowestHpTarget(party);
            if (target == null) return CreateDefend(actor);

            // 50% chance to use AoE skill
            if (random.Range(0, 100) < 50)
            {
                var aoeSkill = FindAffordableAoESkill(skills, actor);
                if (aoeSkill != null)
                {
                    return CreateSkillCommand(actor, target, aoeSkill);
                }
            }

            // 40% chance to use any elemental skill
            if (random.Range(0, 100) < 40)
            {
                var eleSkill = FindAffordableAttackSkill(skills, actor);
                if (eleSkill != null)
                {
                    return CreateSkillCommand(actor, target, eleSkill);
                }
            }

            return CreateNormalAttack(actor, target);
        }

        // Phase 3 (Lv90): Full power - very aggressive, special moves
        private BattleCommand Phase3Action(CharacterStats actor, List<CharacterStats> party,
            List<SkillData> skills, IRandom random)
        {
            var target = FindLowestHpTarget(party);
            if (target == null) return CreateDefend(actor);

            // 70% chance to use strongest skill available
            if (random.Range(0, 100) < 70)
            {
                var strongestSkill = FindStrongestAffordableSkill(skills, actor);
                if (strongestSkill != null)
                {
                    return CreateSkillCommand(actor, target, strongestSkill);
                }
            }

            return CreateNormalAttack(actor, target);
        }

        // Helper methods
        private static CharacterStats FindLowestHpTarget(List<CharacterStats> party)
        {
            CharacterStats lowest = null;
            foreach (var m in party)
            {
                if (m.CurrentHp <= 0) continue;
                if (lowest == null || m.CurrentHp < lowest.CurrentHp) lowest = m;
            }
            return lowest;
        }

        private static SkillData FindAffordableSkillByElement(List<SkillData> skills, CharacterStats actor, ElementType element)
        {
            foreach (var s in skills)
                if (!s.IsHealing && s.Element == element && s.MpCost <= actor.CurrentMp)
                    return s;
            return null;
        }

        private static SkillData FindAffordableAoESkill(List<SkillData> skills, CharacterStats actor)
        {
            foreach (var s in skills)
                if (!s.IsHealing && s.TargetType == SkillTargetType.AllEnemies && s.MpCost <= actor.CurrentMp)
                    return s;
            return null;
        }

        private static SkillData FindAffordableAttackSkill(List<SkillData> skills, CharacterStats actor)
        {
            foreach (var s in skills)
                if (!s.IsHealing && s.MpCost <= actor.CurrentMp)
                    return s;
            return null;
        }

        private static SkillData FindStrongestAffordableSkill(List<SkillData> skills, CharacterStats actor)
        {
            SkillData best = null;
            foreach (var s in skills)
            {
                if (s.IsHealing || s.MpCost > actor.CurrentMp) continue;
                if (best == null || s.Power > best.Power) best = s;
            }
            return best;
        }

        private static BattleCommand CreateNormalAttack(CharacterStats actor, CharacterStats target)
        {
            return new BattleCommand { Attacker = actor, Target = target, Type = CommandType.Attack, Element = actor.Element };
        }

        private static BattleCommand CreateSkillCommand(CharacterStats actor, CharacterStats target, SkillData skill)
        {
            return new BattleCommand
            {
                Attacker = actor, Target = target, Type = CommandType.Skill,
                SkillPower = skill.Power, MpCost = skill.MpCost,
                Element = skill.Element, IsMagic = skill.IsMagic
            };
        }

        private static BattleCommand CreateDefend(CharacterStats actor)
        {
            return new BattleCommand { Attacker = actor, Target = actor, Type = CommandType.Defend };
        }
    }
}
