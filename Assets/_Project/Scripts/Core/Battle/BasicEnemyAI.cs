using System;
using System.Collections.Generic;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class BasicEnemyAI : IEnemyAI
    {
        public BattleCommand DecideAction(EnemyAIContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (context.Actor == null) throw new ArgumentNullException("context.Actor");
            if (context.Party == null) throw new ArgumentNullException("context.Party");
            if (context.Random == null) throw new ArgumentNullException("context.Random");

            var actor = context.Actor;
            var party = context.Party;
            var random = context.Random;
            var skills = context.AvailableSkills ?? new List<SkillData>();
            var breakSystem = context.BreakSystem;

            bool isLowHp = actor.MaxHp <= 0 || actor.CurrentHp * 100 / actor.MaxHp < 30;

            // Priority 1: Low HP + healing skill → 25% chance to heal
            if (isLowHp)
            {
                var healSkill = FindAffordableHealingSkill(skills, actor);
                if (healSkill != null)
                {
                    if (random.Range(0, 100) < 25)
                    {
                        return CreateSkillCommand(actor, actor, healSkill);
                    }
                }
            }

            // Priority 2: Target party member in Break state with strongest attack
            if (breakSystem != null)
            {
                CharacterStats brokenTarget = FindBrokenTarget(party, breakSystem);
                if (brokenTarget != null)
                {
                    var strongestSkill = FindStrongestAffordableAttackSkill(skills, actor);
                    if (strongestSkill != null)
                    {
                        return CreateSkillCommand(actor, brokenTarget, strongestSkill);
                    }
                    return CreateNormalAttack(actor, brokenTarget);
                }
            }

            // Priority 3: Elemental skill matching a party member's WeakElement
            foreach (var skill in skills)
            {
                if (skill.IsHealing || skill.MpCost > actor.CurrentMp)
                    continue;

                foreach (var member in party)
                {
                    if (member.CurrentHp > 0 && member.WeakElement != ElementType.None
                        && member.WeakElement == skill.Element)
                    {
                        return CreateSkillCommand(actor, member, skill);
                    }
                }
            }

            // Priority 4: Low HP defend (also runs if heal was skipped)
            if (isLowHp)
            {
                if (random.Range(0, 100) < 20)
                {
                    return CreateDefendCommand(actor);
                }
            }

            // Priority 5: Normal attack on party member with lowest current HP
            var lowestHpTarget = FindLowestHpTarget(party);
            return CreateNormalAttack(actor, lowestHpTarget);
        }

        private static SkillData FindAffordableHealingSkill(List<SkillData> skills, CharacterStats actor)
        {
            foreach (var skill in skills)
            {
                if (skill.IsHealing && skill.MpCost <= actor.CurrentMp)
                    return skill;
            }
            return null;
        }

        private static SkillData FindStrongestAffordableAttackSkill(List<SkillData> skills, CharacterStats actor)
        {
            SkillData best = null;
            foreach (var skill in skills)
            {
                if (skill.IsHealing || skill.MpCost > actor.CurrentMp)
                    continue;
                if (best == null || skill.Power > best.Power)
                    best = skill;
            }
            return best;
        }

        private static CharacterStats FindBrokenTarget(List<CharacterStats> party, BreakSystem breakSystem)
        {
            foreach (var member in party)
            {
                if (member.CurrentHp > 0 && breakSystem.IsBreaking(member))
                    return member;
            }
            return null;
        }

        private static CharacterStats FindLowestHpTarget(List<CharacterStats> party)
        {
            CharacterStats lowest = null;
            foreach (var member in party)
            {
                if (member.CurrentHp <= 0) continue;
                if (lowest == null || member.CurrentHp < lowest.CurrentHp)
                    lowest = member;
            }
            return lowest;
        }

        private static BattleCommand CreateNormalAttack(CharacterStats actor, CharacterStats target)
        {
            return new BattleCommand
            {
                Attacker = actor,
                Target = target,
                Type = CommandType.Attack,
                Element = actor.Element
            };
        }

        private static BattleCommand CreateDefendCommand(CharacterStats actor)
        {
            return new BattleCommand
            {
                Attacker = actor,
                Target = actor,
                Type = CommandType.Defend
            };
        }

        private static BattleCommand CreateSkillCommand(CharacterStats actor, CharacterStats target, SkillData skill)
        {
            return new BattleCommand
            {
                Attacker = actor,
                Target = target,
                Type = CommandType.Skill,
                SkillPower = skill.Power,
                MpCost = skill.MpCost,
                Element = skill.Element,
                IsMagic = skill.IsMagic
            };
        }
    }
}
