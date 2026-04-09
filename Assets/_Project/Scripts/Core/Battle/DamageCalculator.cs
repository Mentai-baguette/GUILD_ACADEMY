using System;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class DamageCalculator
    {
        public const int MinDamage = 1;
        public const float WeakMultiplier = 1.5f;
        public const float ResistMultiplier = 0.5f;
        public const float NullMultiplier = 0.0f;
        public const float CriticalMultiplier = 1.5f;
        public const float BreakMultiplier = 2.0f;
        public const int VarianceMin = -3;
        public const int VarianceMax = 4;

        private readonly IRandom _random;

        public DamageCalculator(IRandom random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>
        /// ダメージ計算（物理/魔法対応）
        /// skillPower > 0 の場合: ((ATK×2×skillPower/100) - DEF + variance) × element × crit × break
        /// skillPower = 0 の場合: ((ATK×2) - DEF + variance) × element × crit × break
        /// ※ variance は乗算ではなく基礎ダメージへ加算
        /// </summary>
        public int Calculate(int attackStat, int defenseStat, ElementType attackElement,
            ElementType defenderWeakElement, ElementType defenderResistElement,
            ElementType defenderNullElement, bool isCritical, bool isBreakState,
            int skillPower = 0)
        {
            int variance = _random.Range(VarianceMin, VarianceMax);
            int baseDamage = skillPower > 0
                ? attackStat * 2 * skillPower / 100 - defenseStat + variance
                : attackStat * 2 - defenseStat + variance;

            float elementMult = GetElementMultiplier(attackElement, defenderWeakElement, defenderResistElement, defenderNullElement);

            if (elementMult == NullMultiplier)
                return 0;

            float critMult = isCritical ? CriticalMultiplier : 1.0f;
            float breakMult = isBreakState ? BreakMultiplier : 1.0f;

            int damage = (int)(baseDamage * elementMult * critMult * breakMult);
            return Math.Max(MinDamage, damage);
        }

        /// <summary>
        /// 回復量計算（INT依存）
        /// </summary>
        public int CalculateHeal(int healerInt, int skillPower)
        {
            int variance = _random.Range(0, VarianceMax);
            int heal = healerInt * 2 * skillPower / 100 + variance;
            return Math.Max(1, heal);
        }

        /// <summary>
        /// クリティカル判定（DEX依存）
        /// クリティカル率 = DEX/2 %（DEX100→50%、DEX200→100%、上限100%）
        /// </summary>
        public static int GetCriticalChancePercent(int dex)
        {
            return Math.Min(dex / 2, 100);
        }

        public static float GetElementMultiplier(ElementType attackElement,
            ElementType weakElement, ElementType resistElement, ElementType nullElement)
        {
            if (attackElement == ElementType.None)
                return 1.0f;

            if (attackElement == nullElement)
                return NullMultiplier;

            if (attackElement == weakElement)
                return WeakMultiplier;

            if (attackElement == resistElement)
                return ResistMultiplier;

            return 1.0f;
        }
    }
}
