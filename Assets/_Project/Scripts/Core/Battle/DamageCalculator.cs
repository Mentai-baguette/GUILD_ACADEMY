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

        public int Calculate(int atk, int def, ElementType attackElement,
            ElementType defenderWeakElement, ElementType defenderResistElement,
            ElementType defenderNullElement, bool isCritical, bool isBreakState)
        {
            int variance = _random.Range(VarianceMin, VarianceMax);
            int baseDamage = atk - def + variance;

            float elementMult = GetElementMultiplier(attackElement, defenderWeakElement, defenderResistElement, defenderNullElement);

            if (elementMult == NullMultiplier)
                return 0;

            float critMult = isCritical ? CriticalMultiplier : 1.0f;
            float breakMult = isBreakState ? BreakMultiplier : 1.0f;

            int damage = (int)(baseDamage * elementMult * critMult * breakMult);
            return Math.Max(MinDamage, damage);
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
