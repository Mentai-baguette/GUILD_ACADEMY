using System;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class ActionExecutor
    {
        private readonly DamageCalculator _damageCalc;
        private readonly BreakSystem _breakSystem;
        private readonly IRandom _random;

        public ActionExecutor(DamageCalculator damageCalc, BreakSystem breakSystem, IRandom random)
        {
            _damageCalc = damageCalc ?? throw new ArgumentNullException(nameof(damageCalc));
            _breakSystem = breakSystem ?? throw new ArgumentNullException(nameof(breakSystem));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public ActionResult Execute(BattleCommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var result = new ActionResult
            {
                Attacker = command.Attacker,
                Target = command.Target
            };

            if (command.Type == CommandType.Defend)
                return result;

            // クリティカル判定: DEX依存（DEX/200）
            int critChance = DamageCalculator.GetCriticalChancePercent(command.Attacker.Dex);
            bool isCritical = _random.Range(0, 100) < critChance;
            result.WasCritical = isCritical;

            bool isWeakHit = command.Element != ElementType.None &&
                             command.Element == command.Target.WeakElement;
            result.WasWeakHit = isWeakHit;

            bool isBreakState = _breakSystem.IsBreaking(command.Target);

            // 物理/魔法でATK/DEF or INT/RESを使い分け
            int attackStat = command.IsMagic ? command.Attacker.Int : command.Attacker.Atk;
            int defenseStat = command.IsMagic ? command.Target.Res : command.Target.Def;

            int damage = _damageCalc.Calculate(
                attackStat,
                defenseStat,
                command.Element,
                command.Target.WeakElement,
                command.Target.ResistElement,
                command.Target.NullElement,
                isCritical,
                isBreakState,
                command.SkillPower);

            result.DamageDealt = damage;

            bool triggeredBreak = _breakSystem.ApplyHit(command.Target, isWeakHit);
            result.TriggeredBreak = triggeredBreak;

            command.Target.CurrentHp = Math.Max(0, command.Target.CurrentHp - damage);

            result.TargetDefeated = command.Target.CurrentHp <= 0;

            return result;
        }
    }
}
